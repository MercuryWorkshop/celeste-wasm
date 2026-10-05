const CACHE_VERSION = "webleste-v4";
const APP_SHELL = ["./", "./index.html"];

self.addEventListener("install", (event) => {
	event.waitUntil(
		(async () => {
			const cache = await caches.open(CACHE_VERSION);
			await cache.addAll(APP_SHELL);
			try {
				const html = await (await fetch("./index.html")).text();
				const assets = [...html.matchAll(/(?:src|href)="(\.[^"]+)"/g)]
					.map((m) => m[1])
					.filter((u) => !APP_SHELL.includes(u));
				if (assets.length > 0) await cache.addAll(assets);
			} catch {}
			await self.skipWaiting();
		})()
	);
});

self.addEventListener("activate", (event) => {
	event.waitUntil(
		(async () => {
			const keys = await caches.keys();
			await Promise.all(
				keys
					.filter((key) => key !== CACHE_VERSION)
					.map((key) => caches.delete(key))
			);
			await self.clients.claim();
		})()
	);
});

self.addEventListener("fetch", (event) => {
	const { request } = event;
	if (request.method !== "GET") return;
	const url = new URL(request.url);
	if (url.origin !== self.location.origin) return;

	const bypassCache =
		url.pathname.endsWith("/service-worker.js") ||
		url.pathname.endsWith("/_framework/dotnet.js");

	event.respondWith(
		(async () => {
			const cache = await caches.open(CACHE_VERSION);
			if (bypassCache) {
				try {
					const response = await fetch(
						new Request(request, { cache: "reload" })
					);
					if (response.ok && !url.pathname.endsWith("/service-worker.js"))
						cache.put(request, response.clone());
					return response;
				} catch (err) {
					const cached = await caches.match(request);
					if (cached) return cached;
					throw err;
				}
			}
			const isShell =
				request.mode === "navigate" || url.pathname.endsWith("/index.html");
			if (isShell) {
				try {
					const response = await fetch(
						new Request(request, { cache: "reload" })
					);
					if (response.ok) cache.put(request, response.clone());
					return response;
				} catch {}
			}
			const cached = await cache.match(request, { ignoreSearch: false });
			if (cached) return cached;
			try {
				const response = await fetch(request);
				if (response.ok) cache.put(request, response.clone());
				return response;
			} catch (err) {
				const fallback = await cache.match("./index.html");
				if (fallback) return fallback;
				throw err;
			}
		})()
	);
});
