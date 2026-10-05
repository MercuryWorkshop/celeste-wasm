import { DotnetHostBuilder, MonoConfig } from "./dotnetdefs";
import { calculateXXH64, recursiveGetDirectory, rootFolder } from "../fs";
import { SteamJS } from "../achievements";
import { JsSplash } from "./loading";
import { epoxyFetch, EpxTcpWs, EpxWs, getWispUrl } from "../epoxy";
import { steamState } from "../steam";
import { event } from "../analytics";
import { store } from "../store";
import { downloadWithProgress, modsDir } from "../modinstaller";
import {
	cachedEverestVersions,
	everestIdentityMatches,
	normalizeEverestIdentity,
	selectEverestBuild,
	type EverestBuild,
} from "./everest";
import { DEFAULT_STEAM_CONNECTION_DISABLED } from "../store";

const isSteamConnectionDisabled = (value: unknown): boolean => value === true;

export type Log = { color: string; log: string };
export type UpdateEntry = {
	kind: "everest" | "mod";
	name: string;
	installed: string;
	latest: string;

	selector: string;
};

export type MissingDep = {
	mod: string;
	name: string;
	required: string;
	installed: string | null;
	file: string | null;
};
export const gameState: Stateful<{
	ready: boolean;
	initting: boolean;
	playing: boolean;
	hasEverest: boolean;
	memory: number;
	initError: string;
	updatesAvailable: number;
	updates: UpdateEntry[];
	missingDeps: MissingDep[];
	patchFlowOpen: boolean;
}> = $state({
	ready: false,
	initting: false,
	playing: false,
	hasEverest: false,
	memory: -1,
	initError: "",
	updatesAvailable: 0,
	updates: [],
	missingDeps: [],
	patchFlowOpen: false,
});
(globalThis as any).gameState = gameState;
export const loglisteners: ((log: Log) => void)[] = [];

let logs: string[] = [];
(globalThis as any).logs = logs;

function proxyConsole(name: string, color: string) {
	// @ts-expect-error ts sucks
	const old = console[name].bind(console);
	// @ts-expect-error ts sucks
	console[name] = (...args) => {
		let str;
		try {
			str = args.join(" ");
		} catch {
			str = "<failed to render>";
		}
		old(...args);
		for (const logger of loglisteners) {
			logger({ color, log: str });
		}
		logs.push(str);
	};
	return old;
}
export const bypassError = proxyConsole("error", "var(--error)");
export const bypassWarn = proxyConsole("warn", "var(--warning)");
export const bypassLog = proxyConsole("log", "var(--fg)");
export const bypassInfo = proxyConsole("info", "var(--info)");
export const bypassDebug = proxyConsole("debug", "var(--fg4)");
(globalThis as any).bypassLog = bypassLog;

function hookfmod() {
	let contexts: AudioContext[] = [];

	(AudioContext as any) = new Proxy(AudioContext, {
		construct(target, argArray) {
			let ctx = new target(...argArray);
			contexts.push(ctx);
			return ctx;
		},
	});

	window.addEventListener("visibilitychange", async () => {
		if (document.visibilityState === "visible") {
			for (let context of contexts) {
				try {
					await context.resume();
				} catch {}
			}
		} else {
			for (let context of contexts) {
				try {
					await context.suspend();
				} catch {}
			}
		}
	});
}
hookfmod();

useChange([gameState.playing, gameState.initting], () => {
	try {
		if (gameState.playing && !gameState.initting) {
			// @ts-expect-error
			navigator.keyboard.lock();
		} else {
			// @ts-expect-error
			navigator.keyboard.unlock();
		}
	} catch (err) {}
});

let nativefetch = window.fetch;
let wasm: any;
let dotnet: DotnetHostBuilder;
let exports: any;
let runtimePromise: Promise<void> | null = null;

export function getDlls(): (readonly [string, string])[] {
	const config: MonoConfig = wasm.dotnet.instance.config;
	const resources = [
		...(config.resources?.coreAssembly || []),
		...(config.resources?.assembly || []),
	];
	return resources.map((x) => [x.name, x.virtualPath] as const);
}

// the funny custom rsa
// https://github.com/MercuryWorkshop/wispcraft/blob/main/src/connection/crypto.ts
function encryptRSA(data: Uint8Array, n: bigint, e: bigint): Uint8Array {
	const modExp = (base: bigint, exp: bigint, mod: bigint) => {
		let result = 1n;
		base = base % mod;
		while (exp > 0n) {
			if (exp % 2n === 1n) {
				result = (result * base) % mod;
			}
			exp = exp >> 1n;
			base = (base * base) % mod;
		}
		return result;
	};

	// thank you jippity
	const pkcs1v15Pad = (messageBytes: Uint8Array, n: bigint) => {
		const messageLength = messageBytes.length;
		const nBytes = Math.ceil(n.toString(16).length / 2);

		if (messageLength > nBytes - 11) {
			throw new Error("Message too long for RSA encryption");
		}

		const paddingLength = nBytes - messageLength - 3;
		const padding = Array(paddingLength).fill(0xff);

		return BigInt(
			"0x" +
				[
					"00",
					"02",
					...padding.map((byte) => byte.toString(16).padStart(2, "0")),
					"00",
					...Array.from(messageBytes).map((byte: any) =>
						byte.toString(16).padStart(2, "0")
					),
				].join("")
		);
	};
	const paddedMessage = pkcs1v15Pad(data, n);
	let int = modExp(paddedMessage, e, n);

	let hex = int.toString(16);
	if (hex.length % 2) {
		hex = "0" + hex;
	}

	// ????
	return new Uint8Array(
		Array.from(hex.match(/.{2}/g) || []).map((byte) => parseInt(byte, 16))
	);
}

export {
	everestIdentityMatches,
	normalizeEverestIdentity,
	selectEverestBuild,
	type EverestBuild,
} from "./everest";

export async function fetchEverestVersions(): Promise<EverestBuild[]> {
	return cachedEverestVersions(async () => {
		const res = await epoxyFetch(
			"https://everestapi.github.io/everestupdater.txt"
		);
		if (!res.ok) {
			throw new Error(`Everest updater index failed: HTTP ${res.status}`);
		}
		const versionsUrl = (await res.text()).trim();
		if (!versionsUrl.startsWith("https://")) {
			throw new Error("Everest updater index returned an invalid versions URL");
		}
		const versRes = await epoxyFetch(
			versionsUrl + "?supportsNativeBuilds=true"
		);
		if (!versRes.ok) {
			throw new Error(`Everest version list failed: HTTP ${versRes.status}`);
		}
		const versions = (await versRes.json()) as EverestBuild[];
		if (!Array.isArray(versions) || versions.length === 0) {
			throw new Error("Everest version list is empty");
		}
		return versions;
	});
}

export type PatchPhase =
	| "starting"
	| "everest-download"
	| "everest-extract"
	| "patch"
	| "done";

async function downloadEverestBuild(
	build: EverestBuild,
	onProgress?: (frac: number) => void
) {
	if (!build.mainDownload?.startsWith("https://")) {
		throw new Error(
			"Selected Everest build does not have a valid download URL"
		);
	}

	console.log(
		`Installing Everest ${build.branch} ${build.commit} ${build.date} from ${build.mainDownload}`
	);
	const zipres = await epoxyFetch(build.mainDownload);
	if (!zipres.ok) {
		throw new Error(`Everest download failed: HTTP ${zipres.status}`);
	}
	if (!zipres.body) throw new Error("Everest download returned no body");

	const partialName = "everest.zip.partial";
	const partial = await rootFolder.getFileHandle(partialName, { create: true });

	try {
		await downloadWithProgress(
			zipres,
			await partial.createWritable(),
			onProgress
		);
	} catch (err) {
		try {
			await rootFolder.removeEntry(partialName);
		} catch {}
		throw err;
	}

	try {
		await rootFolder.removeEntry("everest.zip");
	} catch {}

	const file = await rootFolder.getFileHandle("everest.zip", { create: true });
	const finalWritable = await file.createWritable();
	try {
		await (await partial.getFile()).stream().pipeTo(finalWritable);
	} catch (err) {
		try {
			await finalWritable.abort();
		} catch {}
		try {
			await rootFolder.removeEntry("everest.zip");
		} catch {}
		try {
			await rootFolder.removeEntry(partialName);
		} catch {}
		throw err;
	}
	await rootFolder.removeEntry(partialName);

	console.log("Successfully downloaded Everest");
	return build;
}

let downloadsFolder: FileSystemDirectoryHandle | null = null;

export async function pickDownloadsFolder() {
	let d = await showDirectoryPicker();
	downloadsFolder = d;
}

let libcurlresolver: any;
export const loadedLibcurlPromise = new Promise((r) => (libcurlresolver = r));
let preInitPromise: Promise<void> | null = null;

export async function preInit() {
	if (gameState.ready) return;
	if (preInitPromise) return preInitPromise;
	preInitPromise = runPreInit();
	return preInitPromise;
}

async function runPreInit() {
	gameState.initting = true;
	gameState.initError = "";
	try {
		await initRuntime();
		gameState.ready = true;

		if (gameState.hasEverest) {
			import("../updates")
				.then((m) => m.checkForUpdates())
				.catch((err) => console.warn("Update check failed", err));
		} else {
			installedModsState.mods = [];
			installedModsState.loaded = true;
		}
	} catch (err) {
		gameState.initError =
			err instanceof Error
				? err.message
				: String(err ?? "initialization failed");
		gameState.ready = false;
		throw err;
	} finally {
		gameState.initting = false;
		preInitPromise = null;
	}
}

async function initRuntime() {
	if (runtimePromise) return runtimePromise;
	runtimePromise = createRuntime();
	try {
		await runtimePromise;
	} catch (err) {
		runtimePromise = null;
		throw err;
	}
}

async function createRuntime() {
	let url = "../_framework/dotnet.js";
	if (import.meta.env.DEV) {
		url = "/_framework/dotnet.js";
	}

	wasm = await eval(`import("${url}")`);
	dotnet = wasm.dotnet;

	console.debug("initializing dotnet");
	const runtime = await dotnet
		.withConfig({
			pthreadPoolInitialSize: 16,
		})
		.withRuntimeOptions([
			// jit functions quickly and jit more functions
			`--jiterpreter-minimum-trace-hit-count=${500}`,

			// monitor jitted functions for less time
			`--jiterpreter-trace-monitoring-period=${100}`,

			// reject less funcs
			`--jiterpreter-trace-monitoring-max-average-penalty=${150}`,

			// increase jit function limits
			`--jiterpreter-wasm-bytes-limit=${64 * 1024 * 1024}`,
			`--jiterpreter-table-size=${32 * 1024}`,

			// print jit stats
			...(import.meta.env.DEV ? [`--jiterpreter-stats-enabled`] : []),
		])
		.withResourceLoader((type, _name, defaultUri, _integrity, behavior) => {
			if (type === "dotnetwasm" && behavior === "dotnetwasm") {
				// for split wasm
				const SPLIT_SIZE = 20 * 1024 * 1024;
				return (async () => {
					let idx = 0;

					let fetchNext = async () => {
						let res = await nativefetch(defaultUri + idx);
						idx++;
						if (!res.body) throw new Error("no body in fetch response");
						return res.status === 200 &&
							!(res.headers.get("content-type") || "").includes("text/html")
							? res.body.getReader()
							: null;
					};

					let chunk = await fetchNext();
					if (!chunk) throw new Error("failed to fetch first chunk");
					let currentStream: ReadableStreamDefaultReader<Uint8Array> = chunk;
					let chunkBytes = 0;

					let stream = new ReadableStream({
						async pull(controller) {
							let { value, done } = await currentStream.read();
							if (done || !value) {
								if (chunkBytes < SPLIT_SIZE) {
									controller.close();
									return;
								}
								chunkBytes = 0;
								chunk = await fetchNext();

								if (chunk) {
									currentStream = chunk;
									await this.pull!(controller);
								} else {
									controller.close();
								}
							} else {
								chunkBytes += value.byteLength;
								controller.enqueue(value);
							}
						},
					});

					let res = new Response(stream, {
						headers: new Headers({ "Content-Type": "application/wasm" }),
					});
					return res;
				})();
			}
		})
		.create();

	runtime.setModuleImports("SteamJS", SteamJS);
	runtime.setModuleImports("JsSplash", JsSplash);

	console.log("loading epoxy");

	window.WebSocket = new Proxy(WebSocket, {
		construct(t, a, n) {
			let url: URL;
			try {
				url = new URL(a[0]);
			} catch {
				return Reflect.construct(t, a, n);
			}
			if (a[0] === getWispUrl() || url.host === location.host)
				return Reflect.construct(t, a, n);
			if (url.hostname.startsWith("__celestewasm_wisp_proxy_ws__"))
				return new EpxTcpWs(
					url.pathname.slice(1),
					url.hostname.replace("__celestewasm_wisp_proxy_ws__", "")
				);

			// @ts-expect-error
			return new EpxWs(...a);
		},
	});

	let dl = document.createElement("a");
	dl.style.display = "none";
	document.body.appendChild(dl);

	window.fetch = async (...args) => {
		// don't try native for steam depots
		if (typeof args[0] !== "string" || !args[0].includes("/depot/")) {
			try {
				return await nativefetch(...args);
			} catch (e) {
				bypassLog(
					"native fetch failed for",
					args,
					", fetching with epoxy instead"
				);
			}
		} else if (downloadsFolder != null) {
			let last = args[0].split("/").pop()!;
			try {
				let file = await downloadsFolder.getFileHandle(last, { create: false });
				let h = await file.getFile();
				console.log("got file cached", last);
				return new Response(h.stream());
			} catch {}
			dl.download = "cross origin lol";
			dl.href = args[0];
			dl.click();

			while (true) {
				try {
					let file = await downloadsFolder.getFileHandle(last, {
						create: false,
					});
					let h = await file.getFile();
					console.log("got file", last);
					return new Response(h.stream());
				} catch {}
				await new Promise((r) => setTimeout(r, 100));
			}
		}

		// @ts-expect-error
		return await epoxyFetch(...args);
	};
	libcurlresolver();

	const config = runtime.getConfig();
	exports = await runtime.getAssemblyExports(config.mainAssemblyName!);
	exports.SteamJS = (
		await runtime.getAssemblyExports("Steamworks.NET.dll")
	).Steamworks.SteamJS;

	// TODO: replace with native openssl
	runtime.setModuleImports("interop.js", {
		encryptrsa: (
			publicKeyModulusHex: string,
			publicKeyExponentHex: string,
			data: Uint8Array
		) => {
			let modulus = BigInt("0x" + publicKeyModulusHex);
			let exponent = BigInt("0x" + publicKeyExponentHex);
			let encrypted = encryptRSA(data, modulus, exponent);
			return new Uint8Array(encrypted);
		},
		XXHash64_Fast: async (path: string) => {
			if (!path.startsWith("/libsdl/"))
				throw new Error("can't hash things not in opfs");
			path = path.slice("/libsdl/".length);

			let start = performance.now();
			let hex = await calculateXXH64(path);
			let bytes = new Uint8Array(8);
			for (let i = 0; i < 8; i++) {
				bytes[i] = parseInt(hex.slice(i * 2, i * 2 + 2), 16);
			}
			let end = performance.now();

			console.debug("xxh64_fast:", path, (end - start).toFixed(1));

			return { ret: bytes };
		},
	});

	(self as any).wasm = {
		Module: runtime.Module,
		// @ts-expect-error
		FS: runtime.Module.FS,
		dotnet,
		runtime,
		config,
		exports,
	};

	const dlls = getDlls();

	const loc = location.pathname;

	await runtime.runMain();
	await exports.CelesteBootstrap.MountFilesystems(
		loc.substring(0, loc.lastIndexOf("/")) + "/",
		dlls.map((x) => `${x[0]}|${x[1]}`)
	);
	await exports.CelesteLoader.PreInit();
	console.debug("dotnet initialized");

	if (!isSteamConnectionDisabled(store.steamConnectionDisabled)) {
		await exports.SteamJS.Init();

		void connectSavedSteam();
	}

	try {
		await recursiveGetDirectory(rootFolder, ["Celeste", "Everest"]);
		gameState.hasEverest = true;
	} catch {
		gameState.hasEverest = false;
	}
}

export type InstalledMod = {
	file: string;
	name: string;
	version: string;
	dependencies: { name: string; version: string; optional: boolean }[];
};

export type InstalledModsState = {
	loaded: boolean;
	mods: InstalledMod[];
};

const toInstalledMod = (m: any): InstalledMod | null => {
	if (!m || typeof m.file !== "string" || !m.file) return null;
	const base = m.file.split("/").pop() ?? m.file;
	const name =
		typeof m.name === "string" && m.name ? m.name : base.replace(/\.zip$/i, "");
	const version = typeof m.version === "string" ? m.version : "";
	const dependencies = Array.isArray(m.dependencies)
		? m.dependencies
				.filter((d: any) => d && typeof d.name === "string" && d.name.trim())
				.map((d: any) => ({
					name: String(d.name),
					version: typeof d.version === "string" ? d.version : "",
					optional: !!d.optional,
				}))
		: [];
	return { file: m.file, name, version, dependencies };
};

const installedModsState: InstalledModsState = $state({
	loaded: false,
	mods: [],
});

export const installedMods = () => installedModsState;

const INSTALLED_MODS_TIMEOUT_MS = 20_000;

const installedModsTimeout = (): Promise<null> =>
	new Promise((res) => setTimeout(() => res(null), INSTALLED_MODS_TIMEOUT_MS));

// The OPFS listing must never hang the UI: even if the storage backend stalls,
// fall back to an empty list so `loaded` is always published promptly.
const OPFS_LIST_TIMEOUT_MS = 10_000;

const withTimeout = <T>(promise: Promise<T>, ms: number, fallback: T): Promise<T> =>
	Promise.race([
		promise,
		new Promise<T>((resolve) => setTimeout(() => resolve(fallback), ms)),
	]);

// OPFS is the source of truth for which mod zips exist. This never needs the
// .NET runtime, so the mod list can render even before (or without) it.
async function listModsFromOpfs(): Promise<InstalledMod[]> {
	try {
		const mods = await modsDir();
		const out: InstalledMod[] = [];
		for await (const [name] of mods) {
			const lower = name.toLowerCase();
			if (!lower.endsWith(".zip") || lower.endsWith(".partial")) continue;
			out.push({
				file: name,
				name: name.replace(/\.zip$/i, ""),
				version: "",
				dependencies: [],
			});
		}
		out.sort((a, b) => a.name.localeCompare(b.name));
		return out;
	} catch {
		return [];
	}
}

const getRuntimeMods = async (): Promise<InstalledMod[]> => {
	const raw = await getRawInstalledMods();
	return raw
		.map(toInstalledMod)
		.filter((mod): mod is InstalledMod => mod !== null);
};

export async function getInstalledMods(): Promise<InstalledMod[]> {
	try {
		const viaRuntime = getRuntimeMods().then(
			(mods): InstalledMod[] | null => mods,
			(): InstalledMod[] | null => null
		);
		const mods = await Promise.race([viaRuntime, installedModsTimeout()]);
		if (mods !== null) return mods;
		console.debug("Installed mod metadata timed out, using file list instead");
	} catch (err) {
		console.warn("Could not list installed mods via runtime", err);
	}
	return await withTimeout(listModsFromOpfs(), OPFS_LIST_TIMEOUT_MS, []);
}

export async function refreshInstalledMods(): Promise<InstalledMod[]> {
	try {
		// Publish the on-disk truth immediately so the UI never sticks on
		// "Loading installed mods..." while the runtime is still starting
		// (or if its metadata call hangs), then enrich with versions and
		// dependencies when the runtime answers.
		const fromDisk = await withTimeout(listModsFromOpfs(), OPFS_LIST_TIMEOUT_MS, []);
		installedModsState.mods = fromDisk;
		installedModsState.loaded = true;
		try {
			const viaRuntime = getRuntimeMods().then(
				(mods): InstalledMod[] | null => mods,
				(): InstalledMod[] | null => null
			);
			const enriched = await Promise.race([viaRuntime, installedModsTimeout()]);
			if (enriched !== null) {
				const byFile = new Map(enriched.map((m) => [m.file.toLowerCase(), m]));
				const merged = fromDisk.map(
					(d) => byFile.get(d.file.toLowerCase()) ?? d
				);
				for (const m of enriched) {
					if (
						!merged.some((d) => d.file.toLowerCase() === m.file.toLowerCase())
					) {
						merged.push(m);
					}
				}
				installedModsState.mods = merged;
			}
		} catch (err) {
			console.warn("Mod metadata enrichment failed", err);
		}
		installedModsState.loaded = true;
		return installedModsState.mods;
	} catch (err) {
		console.warn("Could not list installed mods", err);
		installedModsState.mods = [];
		installedModsState.loaded = true;
		return [];
	}
}

export async function deleteModZip(file: string): Promise<void> {
	(await modsDir()).removeEntry(file);
}

const getRawInstalledMods = async (): Promise<any[]> => {
	const json: string = await exports.Patcher.GetInstalledMods();
	let parsed: unknown;
	try {
		parsed = JSON.parse(json);
	} catch (err) {
		console.error("Could not parse installed mod list", err);
		return [];
	}
	if (!Array.isArray(parsed)) return [];

	return parsed.filter((m: any) => m && typeof m.file === "string" && m.file);
};

export async function hasInstalledEverest(): Promise<boolean> {
	try {
		const everest = await recursiveGetDirectory(rootFolder, [
			"Celeste",
			"Everest",
		]);
		await everest.getFileHandle("Celeste.Mod.mm.dll", { create: false });
		await everest.getFileHandle("MMHOOK_Celeste.dll", { create: false });
		return true;
	} catch {
		return false;
	}
}

async function removeInstalledEverest() {
	for (const name of ["Celeste.Mod.mm.dll", "MMHOOK_Celeste.dll"]) {
		try {
			await rootFolder.removeEntry(name);
		} catch {}
	}
	for (const name of ["Celeste", "everest.zip"]) {
		try {
			await rootFolder.removeEntry(name, { recursive: true });
		} catch {}
	}
}

export async function removeUploadedCopy() {
	for (const name of [
		"Celeste.exe",
		"Celeste.dll",
		".ContentExists",
		"CustomCeleste.dll",
		"CustomCeleste.patchver",
		"everest.zip",
	]) {
		try {
			await rootFolder.removeEntry(name);
		} catch {}
	}
	for (const name of ["Content", "orig", "Celeste"]) {
		try {
			await rootFolder.removeEntry(name, { recursive: true });
		} catch {}
	}

	store.logs = 0;
	store.theme =
		window.matchMedia &&
		window.matchMedia("(prefers-color-scheme: light)").matches
			? "light"
			: "dark";
	store.wispServer = import.meta.env.VITE_WISP_URL || "wss://anura.pro";
	store.everestVersion = "stable";
	store.installedEverestVersion = "";
	store.epoxyVersion = "";
	store.accentColor = undefined;
	store.analytics = true;
	store.steamConnectionDisabled = DEFAULT_STEAM_CONNECTION_DISABLED;

	try {
		localStorage.removeItem("options");
	} catch {}
	try {
		sessionStorage.clear();
	} catch {}
}

export const PATCH_VERSION = 1;
const PATCH_MARKER = "CustomCeleste.patchver";

export async function readPatchVersion(): Promise<string | null> {
	try {
		const file = await rootFolder.getFileHandle(PATCH_MARKER, {
			create: false,
		});
		return (await file.getFile().then((r) => r.text())).trim() || null;
	} catch {
		return null;
	}
}

async function writePatchVersion() {
	const file = await rootFolder.getFileHandle(PATCH_MARKER, { create: true });
	const writable = await file.createWritable();
	try {
		await writable.write(String(PATCH_VERSION));
		await writable.close();
	} catch (err) {
		try {
			await writable.abort();
		} catch {}
		throw err;
	}
}

export async function clearPatchVersion() {
	try {
		await rootFolder.removeEntry(PATCH_MARKER);
	} catch {}
}

export async function PatchCeleste(
	installEverest: boolean,
	onProgress?: (phase: PatchPhase, value: number) => void
) {
	if (!installEverest) {
		await removeInstalledEverest();
	}
	if (installEverest) {
		const { store } = await import("../store");
		const wanted = (store.everestVersion ?? "stable").trim() || "stable";
		const installed = store.installedEverestVersion?.trim() ?? "";
		const versions = await fetchEverestVersions();
		const build = selectEverestBuild(versions, wanted);
		const identity = normalizeEverestIdentity(build) || wanted;
		const alreadyInstalled = await hasInstalledEverest();
		const versionChanged =
			!everestIdentityMatches(installed, identity) || !installed;
		if (!alreadyInstalled || versionChanged) {
			const downloadProgress = (frac: number) =>
				onProgress?.(
					"everest-download",
					frac < 0 ? -1 : Math.min(frac * 0.5, 0.5)
				);
			if (versionChanged) {
				// The requested build differs from what's on disk: a stale
				// everest.zip must never be reused here.
				try {
					await rootFolder.removeEntry("everest.zip");
				} catch {}
				await downloadEverestBuild(build, downloadProgress);
				onProgress?.("everest-download", 0.5);
			} else {
				try {
					await rootFolder.getFileHandle("everest.zip", { create: false });
					onProgress?.("everest-download", 0.5);
				} catch {
					await downloadEverestBuild(build, downloadProgress);
					onProgress?.("everest-download", 0.5);
				}
			}

			onProgress?.("everest-extract", 0.55);
			if (!(await exports.Patcher.ExtractEverest())) {
				throw new Error("Failed to extract the selected Everest build");
			}
			store.installedEverestVersion = identity;
		} else {
			onProgress?.("everest-extract", 0.55);
		}
	}

	onProgress?.("patch", installEverest ? 0.7 : 0.05);
	if (!(await exports.Patcher.PatchCeleste(installEverest))) {
		throw new Error(
			"The loader could not patch Celeste. Check the setup log for details."
		);
	}
	gameState.hasEverest = installEverest;
	await writePatchVersion();
	onProgress?.("done", 1);
}

let steamConnectionPromise: Promise<boolean> | null = null;

async function connectSavedSteam(): Promise<boolean> {
	if (isSteamConnectionDisabled(store.steamConnectionDisabled)) return false;
	if (steamConnectionPromise) return steamConnectionPromise;

	steamConnectionPromise = (async () => {
		try {
			const ok = await exports.SteamJS.InitSteamSaved();
			if (ok) {
				console.log("Logged in via saved login");
				steamState.login = 2;
			}
			return ok;
		} catch (err) {
			console.warn("Saved Steam login failed", err);
			return false;
		} finally {
			steamConnectionPromise = null;
		}
	})();
	return steamConnectionPromise;
}

export async function setSteamConnectionDisabled(disabled: boolean) {
	store.steamConnectionDisabled = disabled;
	if (!disabled) {
		if (!exports?.SteamJS) return;
		await exports.SteamJS.Init();
		void connectSavedSteam();
		return;
	}

	steamState.login = 0;
	steamConnectionPromise = null;
	try {
		await exports?.SteamJS?.ShutdownSteam();
	} catch (err) {
		console.warn("Failed to disconnect Steam", err);
	}
}

export async function initSteam(
	username: string | null,
	password: string | null,
	qr: boolean
): Promise<boolean> {
	if (isSteamConnectionDisabled(store.steamConnectionDisabled)) return false;
	return await exports.SteamJS.InitSteam(username, password, qr);
}

export async function DownloadApp() {
	if (isSteamConnectionDisabled(store.steamConnectionDisabled)) return false;
	return await exports.SteamJS.DownloadApp();
}

export async function DownloadSteamCloud() {
	if (isSteamConnectionDisabled(store.steamConnectionDisabled)) return false;
	return await exports.SteamJS.DownloadSteamCloud();
}
export async function UploadSteamCloud() {
	if (isSteamConnectionDisabled(store.steamConnectionDisabled)) return false;
	return await exports.SteamJS.UploadSteamCloud();
}

const SEAMLESSCOUNT = 5;

function monitorMem(): () => void {
	let stop = false;
	exports.CelesteLoader.WatchMemoryUsage((mem: number) => {
		gameState.memory = mem;
		return stop;
	});
	return () => (stop = true);
}

let playPromise: Promise<void> | null = null;

export function play(): Promise<void> {
	if (playPromise) return playPromise;
	playPromise = runPlay().finally(() => {
		playPromise = null;
	});
	return playPromise;
}

async function runPlay() {
	event("play-begin");
	if (gameState.playing) return;

	try {
		if (!gameState.ready) await preInit();
		if (gameState.playing) return;
		if (!gameState.ready) {
			throw new Error(gameState.initError || "Celeste is not ready");
		}

		gameState.playing = true;
		gameState.initting = true;
		gameState.initError = "";

		let stopMonitoring: (() => void) | null = null;
		let launched = false;
		try {
			stopMonitoring = monitorMem();

			console.debug("Init...");
			const before = performance.now();

			await exports.CelesteLoader.Init(false);

			// run some frames for seamless transition
			for (let i = 0; i < SEAMLESSCOUNT; i++) {
				console.debug(`SeamlessInit${i}...`);
				if (!(await exports.CelesteLoader.RunOneFrame()))
					throw new Error("CelesteLoader.RunOneFrame() Failed!");
			}

			const after = performance.now();
			console.debug(`Init : ${(after - before).toFixed(2)}ms`);
			gameState.initting = false;

			launched = true;
			try {
				await exports.CelesteLoader.MainLoop();
			} catch (err) {
				// The game exited (Escape-hold quit or an in-game restart):
				// fall through to cleanup so the UI never sticks on a dead
				// canvas.
				console.debug("Game loop exited", err);
			}
		} finally {
			stopMonitoring?.();
			try {
				// @ts-expect-error
				navigator.keyboard?.unlock?.();
			} catch (keyboardErr) {
				console.warn("failed to unlock keyboard", keyboardErr);
			}
			gameState.initting = false;
			if (!launched) {
				gameState.playing = false;
			}
		}
		if (!launched) return;

		console.debug("Cleanup...");
		try {
			await exports.CelesteLoader.Cleanup();
		} catch (cleanupErr) {
			console.warn("game cleanup failed", cleanupErr);
		}
		gameState.ready = false;
		gameState.playing = false;

		const wantsRestart = await exports.CelesteLoader.ConsumeRestartRequest();
		if (wantsRestart) {
			location.reload();
			return;
		}
	} catch (err) {
		gameState.initError =
			err instanceof Error
				? err.message
				: String(err ?? "failed to start game");
		throw err;
	}
}
