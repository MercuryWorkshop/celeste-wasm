import { RingBuffer } from "ring-buffer-ts";

export type Log = { color: string, log: string };
export const TIMEBUF_SIZE = 120;
export const gameState: Stateful<{
	ready: boolean,
	playing: boolean,

	// these will NOT work with use()
	logbuf: Log[],
	timebuf: RingBuffer<number>,
}> = $state({
	ready: false,
	playing: false,
	logbuf: [],
	timebuf: new RingBuffer<number>(TIMEBUF_SIZE)
});

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
		gameState.logbuf.push({
			color,
			log: `[${new Date().toISOString()}]: ${str}`
		});
	}
}
proxyConsole("error", "var(--error)");
proxyConsole("warn", "var(--warning)");
proxyConsole("log", "var(--fg)");
proxyConsole("info", "var(--info)");
proxyConsole("debug", "var(--fg6)");

function hookfmod() {
	let contexts: AudioContext[] = [];

	let ctx = AudioContext;
	(AudioContext as any) = function() {
		let context = new ctx();

		contexts.push(context);
		return context;
	};

	window.addEventListener("focus", async () => {
		for (let context of contexts) {
			try {
				await context.resume();
			} catch { }
		}
	});
	window.addEventListener("blur", async () => {
		for (let context of contexts) {
			try {
				await context.suspend();
			} catch { }
		}
	});
}
hookfmod();

const wasm = await eval(`import("/_framework/dotnet.js")`);
const dotnet = wasm.dotnet;
let exports: any;

async function preInit() {
	console.debug("initializing dotnet");
	const runtime = await dotnet.withConfig({
		jsThreadBlockingMode: "DangerousAllowBlockingWait",
	}).create();

	const config = runtime.getConfig();
	exports = await runtime.getAssemblyExports(config.mainAssemblyName);
	const canvas = document.getElementById("canvas")! as HTMLCanvasElement;
	dotnet.instance.Module.canvas = canvas;

	(self as any).wasm = {
		Module: dotnet.instance.Module,
		dotnet,
		runtime,
		config,
		exports,
		canvas,
	};

	console.debug("PreInit...");
	await runtime.runMain();
	await exports.Program.PreInit();
	console.debug("dotnet initialized");

	gameState.ready = true;
};
preInit();

export async function play() {
	gameState.playing = true;

	const before = performance.now();
	console.debug("Init...");
	exports.Program.Init();
	const after = performance.now();
	console.debug(`Init : ${(after - before).toFixed(2)}ms`);

	console.debug("MainLoop...");
	const main = () => {
		const before = performance.now();
		const ret = exports.Program.MainLoop();
		const after = performance.now();

		gameState.timebuf.add(after - before);

		if (!ret) {
			console.debug("Cleanup...");

			gameState.timebuf.clear();

			exports.Program.Cleanup();
			gameState.playing = false;

			return;
		}

		requestAnimationFrame(main);
	}
	requestAnimationFrame(main);
}

useChange([gameState.playing], () => {
	try {
		if (gameState.playing) {
			// @ts-expect-error
			navigator.keyboard.lock()
		} else {
			// @ts-expect-error
			navigator.keyboard.unlock();
		}
	} catch (err) { console.log("keyboard lock error:", err); }
});

document.addEventListener("keydown", (e: KeyboardEvent) => {
	if (gameState.playing && ["Space", "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "Tab"].includes(e.code)) {
		e.preventDefault();
	}
});
