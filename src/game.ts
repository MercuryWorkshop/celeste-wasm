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

// tokio::spawn()
(async () => {
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
})();

export async function play() {
	gameState.playing = true;

	console.debug("Init...");
	exports.Program.Init();

	let avgTop = 0;
	let avgBottom = 0;

	console.debug("MainLoop...");
	const main = () => {
		const before = performance.now();
		exports.Program.MainLoop();
		const after = performance.now();

		avgTop += after - before;
		avgBottom++;

		requestAnimationFrame(main);
	}
	requestAnimationFrame(main);
}

useChange([gameState.playing], () => {
	try {
		if (gameState.playing) {
			// @ts-ignore
			navigator.keyboard.lock()
		} else {
			// @ts-ignore
			navigator.keyboard.unlock();
		}
	} catch { }
});


(self as any).copyContent = async () => {
	const opfsRoot = await navigator.storage.getDirectory();
	// @ts-ignore
	const sourceDirEntry = await window.showDirectoryPicker();
	const sourceDirName = sourceDirEntry.name;
	const targetRootDir = await opfsRoot.getDirectoryHandle(sourceDirName, { create: true });
	async function copyDirectoryContents(sourceDirEntry: FileSystemDirectoryHandle, targetDir: FileSystemDirectoryHandle) {
		// @ts-expect-error ts sucks
		const entriesIterator = sourceDirEntry.entries();
		for await (const [entryName, entry] of entriesIterator) {
			if (entry.kind === 'file') {
				const file = await entry.getFile(); // Get the actual file from FileSystemFileHandle
				const fileHandle = await targetDir.getFileHandle(entryName, { create: true });
				const writableStream = await fileHandle.createWritable();
				await writableStream.write(file);  // Write the file Blob directly
				await writableStream.close();
				console.log(`Successfully copied file: ${entryName}`);
			} else if (entry.kind === 'directory') {
				const subDirHandle = await targetDir.getDirectoryHandle(entryName, { create: true });
				console.log(`Created directory: ${entryName}`);
				await copyDirectoryContents(entry, subDirHandle);
			}
		}
	}

	// Start copying from the source directory to the new root directory in OPFS
	await copyDirectoryContents(sourceDirEntry, targetRootDir);
}
