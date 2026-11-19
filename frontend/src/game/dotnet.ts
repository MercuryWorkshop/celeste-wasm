import { DotnetHostBuilder, MonoConfig } from "./dotnetdefs";
import { calculateXXH64, recursiveGetDirectory, rootFolder } from "../fs";
import { SteamJS } from "../achievements";
import { JsSplash } from "./loading";
import { epoxyFetch, EpxTcpWs, EpxWs, getWispUrl } from "../epoxy";
import { steamState } from "../steam";
import { event } from "../analytics";

export type Log = { color: string; log: string };
export const gameState: Stateful<{
	ready: boolean;
	initting: boolean;
	playing: boolean;
	hasEverest: boolean;
	memory: number;
}> = $state({
	ready: false,
	initting: false,
	playing: false,
	hasEverest: false,
	memory: -1,
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

export async function downloadEverest() {
	const branch = "stable";
	const res = await epoxyFetch(
		"https://everestapi.github.io/everestupdater.txt"
	);
	const versionsUrl = await res.text();
	const versRes = await epoxyFetch(
		versionsUrl.trim() + "?supportsNativeBuilds=true"
	);
	const versions = await versRes.json();

	const build = versions.filter((v: any) => v.branch == branch)[0];

	console.log(
		`Installing Everest ${branch} ${build.commit} ${build.date} from ${build.mainDownload}`
	);
	const zipres = await epoxyFetch(build.mainDownload);
	const zipbin = await zipres.arrayBuffer();

	const file = await rootFolder.getFileHandle("everest.zip", { create: true });
	const writable = await file.createWritable();
	await writable.write(new Uint8Array(zipbin));
	await writable.close();

	console.log("Successfully downloaded Everest");
}

let downloadsFolder: FileSystemDirectoryHandle | null = null;

export async function pickDownloadsFolder() {
	let d = await showDirectoryPicker();
	downloadsFolder = d;
}

let libcurlresolver: any;
export const loadedLibcurlPromise = new Promise((r) => (libcurlresolver = r));
export async function preInit() {
	if (gameState.ready) return;

	// dotnet.js is fingerprinted, so find its hashed name at build time
	const dotnetJs = Object.keys(
		import.meta.glob("/public/_framework/dotnet.*.js")
	)
		.find((x) => /\/dotnet\.[a-z0-9]+\.js$/.test(x))
		?.slice("/public".length);
	if (!dotnetJs) throw new Error("no fingerprinted dotnet.js in _framework");

	let url = ".." + dotnetJs;
	if (import.meta.env.DEV) {
		url = dotnetJs;
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
			`--jiterpreter-stats-enabled`,
		])
		.withResourceLoader((type, _name, defaultUri, _integrity, behavior) => {
			// for split wasm
			if (type === "dotnetwasm" && behavior === "dotnetwasm") {
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

					let stream = new ReadableStream({
						async pull(controller) {
							let { value, done } = await currentStream.read();
							if (done || !value) {
								chunk = await fetchNext();

								if (chunk) {
									currentStream = chunk;
									await this.pull!(controller);
								} else {
									controller.close();
								}
							} else {
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
			const url = new URL(a[0]);
			if (a[0] === getWispUrl() || url.host === location.host)
				return Reflect.construct(t, a, n);
			if (url.hostname.startsWith("__celestewasm_wisp_proxy_ws__"))
				return new EpxTcpWs(
					url.pathname.substring(1),
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

	await exports.SteamJS.Init();
	if (await exports.SteamJS.InitSteamSaved()) {
		console.log("Logged in via saved login");
		steamState.login = 2;
	}

	try {
		await recursiveGetDirectory(rootFolder, ["Celeste", "Everest"]);
		gameState.hasEverest = true;
	} catch {
		gameState.hasEverest = false;
	}

	gameState.ready = true;
}

export async function PatchCeleste(installEverest: boolean) {
	if (installEverest) {
		try {
			await (
				await recursiveGetDirectory(rootFolder, ["Celeste", "Everest"])
			).getFileHandle("Celeste.Mod.mm.dll", { create: false });
		} catch {
			try {
				await rootFolder.getFileHandle("everest.zip", { create: false });
			} catch {
				await downloadEverest();
			}

			if (!(await exports.Patcher.ExtractEverest())) {
				throw "failed to extract everest";
			}
		}
	}

	if (!(await exports.Patcher.PatchCeleste(installEverest))) {
		throw "failed to patch celeste";
	}
	gameState.hasEverest = true;
}

export async function initSteam(
	username: string | null,
	password: string | null,
	qr: boolean
): Promise<boolean> {
	return await exports.SteamJS.InitSteam(username, password, qr);
}

export async function DownloadApp() {
	return await exports.SteamJS.DownloadApp();
}

export async function DownloadSteamCloud() {
	return await exports.SteamJS.DownloadSteamCloud();
}
export async function UploadSteamCloud() {
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

export async function play() {
	event("play-begin");
	gameState.playing = true;
	gameState.initting = true;

	let stopMonitoring = monitorMem();

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

	await exports.CelesteLoader.MainLoop();

	stopMonitoring();

	console.debug("Cleanup...");
	await exports.CelesteLoader.Cleanup();
	gameState.ready = false;
	gameState.playing = false;
}
