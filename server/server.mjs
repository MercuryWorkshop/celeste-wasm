#!/usr/bin/env node

// celeste-wasm self-hosted server
// Zero external dependencies — uses only Node.js built-ins
// Usage: node server.mjs [--port 8080] [--host 0.0.0.0] [--no-open]

import { createServer } from "node:http";
import { stat, access, mkdir, writeFile } from "node:fs/promises";
import { createReadStream } from "node:fs";
import { join, extname, resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { execSync } from "node:child_process";
import { networkInterfaces } from "node:os";

// ---------------------------------------------------------------------------
// Paths — everything is relative to this script's location
// ---------------------------------------------------------------------------
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const DIST_DIR = resolve(__dirname, "dist");
const TAR_FILE = resolve(__dirname, "celeste-wasm.tar");
const CACHE_DIR = resolve(__dirname, "cache");
const CACHE_MARKER = join(CACHE_DIR, ".extracted");

// ---------------------------------------------------------------------------
// CLI argument parsing (minimal, no deps)
// ---------------------------------------------------------------------------
function parseArgs() {
  const args = process.argv.slice(2);
  const opts = { port: 8080, host: "0.0.0.0", open: true };

  for (let i = 0; i < args.length; i++) {
    switch (args[i]) {
      case "--port":
      case "-p":
        opts.port = parseInt(args[++i], 10);
        break;
      case "--host":
        opts.host = args[++i];
        break;
      case "--no-open":
        opts.open = false;
        break;
      case "--help":
        console.log(`
celeste-wasm server

Usage: node server.mjs [options]

Options:
  --port, -p <number>   Port to listen on (default: 8080)
  --host <string>       Host to bind to (default: 0.0.0.0)
  --no-open             Don't auto-open the browser
  --help                Show this help
`);
        process.exit(0);
    }
  }
  return opts;
}

// ---------------------------------------------------------------------------
// MIME types
// ---------------------------------------------------------------------------
const MIME_TYPES = {
  ".html": "text/html; charset=utf-8",
  ".htm": "text/html; charset=utf-8",
  ".js": "application/javascript; charset=utf-8",
  ".mjs": "application/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".wasm": "application/wasm",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".gif": "image/gif",
  ".webp": "image/webp",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
  ".ttf": "font/ttf",
  ".woff": "font/woff",
  ".woff2": "font/woff2",
  ".otf": "font/otf",
  ".xml": "application/xml",
  ".txt": "text/plain; charset=utf-8",
  ".bin": "application/octet-stream",
  ".dll": "application/octet-stream",
  ".exe": "application/octet-stream",
  ".pdb": "application/octet-stream",
  ".dat": "application/octet-stream",
  ".xnb": "application/octet-stream",
  ".ogg": "audio/ogg",
  ".bank": "application/octet-stream",
  ".obj": "text/plain",
  ".export": "text/plain",
  ".spritefont": "application/xml",
  ".celeste": "application/octet-stream",
  ".tar": "application/x-tar",
  ".zip": "application/zip",
};

function getMimeType(filePath) {
  const ext = extname(filePath).toLowerCase();
  return MIME_TYPES[ext] || "application/octet-stream";
}

// ---------------------------------------------------------------------------
// Tar extraction — uses system `tar` command
// ---------------------------------------------------------------------------
async function extractTarIfNeeded() {
  // Check if already extracted
  try {
    await access(CACHE_MARKER);
    console.log("  Game assets already extracted (cached).");
    return;
  } catch {
    // Not extracted yet
  }

  // Check tar file exists
  try {
    await access(TAR_FILE);
  } catch {
    console.error(`  Error: ${TAR_FILE} not found.`);
    console.error("  Place celeste-wasm.tar next to server.mjs");
    process.exit(1);
  }

  console.log("  Extracting game assets (first run only)...");
  const startTime = Date.now();

  await mkdir(CACHE_DIR, { recursive: true });

  try {
    execSync(`tar xf "${TAR_FILE}" -C "${CACHE_DIR}"`, {
      stdio: "pipe",
      timeout: 300_000, // 5 min timeout
    });
  } catch (err) {
    console.error("  Failed to extract tar:", err.message);
    process.exit(1);
  }

  // Create bundled marker inside content directory
  const contentDir = join(CACHE_DIR, "Content");
  try {
    await access(contentDir);
    await writeFile(join(contentDir, ".bundled"), "");
  } catch {
    console.warn(
      "  Warning: Content/ directory not found in tar -- game may not work correctly"
    );
  }

  // Write extraction marker
  await writeFile(CACHE_MARKER, new Date().toISOString());

  const elapsed = ((Date.now() - startTime) / 1000).toFixed(1);
  console.log(`  Extracted game assets in ${elapsed}s`);
}

// ---------------------------------------------------------------------------
// Check that dist/ directory exists
// ---------------------------------------------------------------------------
async function checkDist() {
  try {
    await access(DIST_DIR);
  } catch {
    console.error(`  Error: ${DIST_DIR} not found.`);
    console.error(
      '  Build the frontend first with "make publish", then copy frontend/dist/ here.'
    );
    process.exit(1);
  }
}

// ---------------------------------------------------------------------------
// Resolve a URL path to a file on disk
// ---------------------------------------------------------------------------
// Route mapping:
//   /content/.bundled   -> virtual 200 OK
//   /content/*          -> cache/Content/*
//   /CustomCeleste.dll  -> cache/CustomCeleste.dll
//   /Celeste.exe        -> cache/Celeste.exe
//   /_framework/*       -> dist/_framework/*
//   /*                  -> dist/*
//   fallback            -> dist/index.html (SPA)

// Files from tar root that should be served at HTTP root
const CACHE_ROOT_FILES = new Set([
  "CustomCeleste.dll",
  "Celeste.exe",
  "MMHOOK_Celeste.dll",
  "Celeste.Mod.mm.dll",
  ".ContentExists",
]);

async function resolveFile(urlPath) {
  // Prevent directory traversal
  const sanitized = urlPath.replace(/\.\./g, "").replace(/\/+/g, "/");

  // Virtual: /content/.bundled always returns 200
  if (sanitized === "/content/.bundled") {
    return { virtual: true, body: "", mime: "text/plain" };
  }

  // /content/* -> cache/Content/*
  if (sanitized.startsWith("/content/")) {
    const subPath = sanitized.slice("/content/".length);
    const filePath = join(CACHE_DIR, "Content", subPath);
    return { filePath };
  }

  // Root-level game files -> cache/
  const baseName = sanitized.slice(1); // strip leading /
  if (CACHE_ROOT_FILES.has(baseName)) {
    const filePath = join(CACHE_DIR, baseName);
    return { filePath };
  }

  // Everything else -> dist/
  let filePath = join(DIST_DIR, sanitized);

  try {
    const s = await stat(filePath);
    if (s.isDirectory()) {
      filePath = join(filePath, "index.html");
    }
  } catch {
    // File doesn't exist in dist — check if it actually exists before SPA fallback
    // Don't fallback for requests to _framework/ or assets/ (those are real 404s)
    if (
      sanitized.startsWith("/_framework/") ||
      sanitized.startsWith("/assets/")
    ) {
      return { filePath }; // Let it 404 naturally
    }
    // SPA fallback for everything else
    filePath = join(DIST_DIR, "index.html");
  }

  return { filePath };
}

// ---------------------------------------------------------------------------
// HTTP request handler
// ---------------------------------------------------------------------------
async function handleRequest(req, res) {
  const url = new URL(req.url, `http://${req.headers.host}`);
  let urlPath = decodeURIComponent(url.pathname);

  // Normalize: strip trailing slash except for root
  if (urlPath !== "/" && urlPath.endsWith("/")) {
    urlPath = urlPath.slice(0, -1);
  }

  // Root -> /index.html
  if (urlPath === "/") {
    urlPath = "/index.html";
  }

  // Required headers for SharedArrayBuffer (WASM threads)
  res.setHeader("Cross-Origin-Embedder-Policy", "require-corp");
  res.setHeader("Cross-Origin-Opener-Policy", "same-origin");

  // CORS for content paths
  if (urlPath.startsWith("/content/")) {
    res.setHeader("Access-Control-Allow-Origin", "*");
    res.setHeader("Cross-Origin-Resource-Policy", "cross-origin");
  }

  // Handle OPTIONS preflight
  if (req.method === "OPTIONS") {
    res.setHeader("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
    res.setHeader("Access-Control-Allow-Headers", "*");
    res.writeHead(204);
    res.end();
    return;
  }

  try {
    const resolved = await resolveFile(urlPath);

    // Virtual responses (e.g., /content/.bundled)
    if (resolved.virtual) {
      res.setHeader("Content-Type", resolved.mime);
      res.writeHead(200);
      res.end(resolved.body);
      return;
    }

    const filePath = resolved.filePath;

    // Check the file exists and get its size
    let fileStat;
    try {
      fileStat = await stat(filePath);
    } catch {
      res.writeHead(404, { "Content-Type": "text/plain" });
      res.end("Not Found");
      return;
    }

    if (!fileStat.isFile()) {
      res.writeHead(404, { "Content-Type": "text/plain" });
      res.end("Not Found");
      return;
    }

    const mime = getMimeType(filePath);
    const size = fileStat.size;

    // Support Range requests (the .NET WASM runtime may use them)
    const rangeHeader = req.headers.range;
    if (rangeHeader) {
      const match = rangeHeader.match(/bytes=(\d+)-(\d*)/);
      if (match) {
        const start = parseInt(match[1], 10);
        const end = match[2] ? parseInt(match[2], 10) : size - 1;

        if (start >= size || end >= size) {
          res.writeHead(416, {
            "Content-Range": `bytes */${size}`,
          });
          res.end();
          return;
        }

        res.writeHead(206, {
          "Content-Type": mime,
          "Content-Length": end - start + 1,
          "Content-Range": `bytes ${start}-${end}/${size}`,
          "Accept-Ranges": "bytes",
          "Cache-Control": "public, max-age=3600",
        });

        if (req.method === "HEAD") {
          res.end();
          return;
        }

        createReadStream(filePath, { start, end }).pipe(res);
        return;
      }
    }

    // Normal response
    res.writeHead(200, {
      "Content-Type": mime,
      "Content-Length": size,
      "Accept-Ranges": "bytes",
      "Cache-Control": "public, max-age=3600",
    });

    if (req.method === "HEAD") {
      res.end();
      return;
    }

    createReadStream(filePath).pipe(res);
  } catch (err) {
    console.error(`  Error serving ${urlPath}:`, err.message);
    res.writeHead(500, { "Content-Type": "text/plain" });
    res.end("Internal Server Error");
  }
}

// ---------------------------------------------------------------------------
// Get local network IP
// ---------------------------------------------------------------------------
function getLocalIP() {
  const nets = networkInterfaces();
  for (const name of Object.keys(nets)) {
    for (const net of nets[name]) {
      if (net.family === "IPv4" && !net.internal) {
        return net.address;
      }
    }
  }
  return null;
}

// ---------------------------------------------------------------------------
// Open browser
// ---------------------------------------------------------------------------
function openBrowser(url) {
  const platform = process.platform;
  try {
    if (platform === "darwin") {
      execSync(`open "${url}"`, { stdio: "ignore" });
    } else if (platform === "win32") {
      execSync(`start "" "${url}"`, { stdio: "ignore" });
    } else {
      execSync(
        `xdg-open "${url}" 2>/dev/null || sensible-browser "${url}" 2>/dev/null`,
        { stdio: "ignore" }
      );
    }
  } catch {
    // Silently fail — user can open the URL manually
  }
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------
async function main() {
  const opts = parseArgs();

  console.log("");
  console.log("  celeste-wasm server");
  console.log("  -------------------");
  console.log("");

  await checkDist();
  await extractTarIfNeeded();

  console.log("");

  const server = createServer(handleRequest);

  server.listen(opts.port, opts.host, () => {
    const localUrl = `http://localhost:${opts.port}`;
    const lanIP = getLocalIP();
    const networkUrl = lanIP ? `http://${lanIP}:${opts.port}` : null;

    console.log(`  Local:   ${localUrl}`);
    if (networkUrl) {
      console.log(`  Network: ${networkUrl}`);
    }
    console.log("");
    console.log("  Press Ctrl+C to stop.");
    console.log("");

    if (opts.open) {
      openBrowser(localUrl);
    }
  });

  server.on("error", (err) => {
    if (err.code === "EADDRINUSE") {
      console.error(`  Error: Port ${opts.port} is already in use.`);
      console.error(`  Try: node server.mjs --port ${opts.port + 1}`);
      process.exit(1);
    }
    throw err;
  });
}

main();
