export type Log = { color: string, log: string };
export const gameState: Stateful<{
	ready: boolean,
	playing: boolean,
	logbuf: Log[]
}> = $state({
	ready: false,
	playing: false,
	logbuf: [],
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
		gameState.logbuf = [...gameState.logbuf, {
			color,
			log: `[${new Date().toISOString()}]: ${str}`
		}];
	}
}
proxyConsole("error", "var(--error)");
proxyConsole("warn", "var(--warning)");
proxyConsole("log", "var(--fg)");
proxyConsole("info", "var(--info)");
proxyConsole("debug", "var(--fg6)");

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

	console.debug("Init...");
	exports.Program.Init();

	console.debug("MainLoop...");
	const main = () => {
		if (!exports.Program.MainLoop()) {
			console.debug("Cleanup...");

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
