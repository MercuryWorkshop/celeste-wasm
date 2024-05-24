import { zip, unzip } from "fflate";

export const version = "1.4.0.0";
let setModuleImports, getAssemblyExports, getConfig;
export async function init() {
	({ setModuleImports, getAssemblyExports, getConfig } = await dotnet
		.withModuleConfig({
			onConfigLoaded: (config) => {
				config.disableIntegrityCheck = true;
			},
		})
		.withDiagnosticTracing(false)
		.withApplicationArgumentsFromQuery()
		.create());

	if (!dotnet.instance.Module.FS.analyzePath("/libsdl").path) {
		dotnet.instance.Module.FS.mkdir("/libsdl", 0o755);
		dotnet.instance.Module.FS.mount(
			dotnet.instance.Module.FS.filesystems.IDBFS,
			{},
			"/libsdl",
		);
	}
	await new Promise((r) => dotnet.instance.Module.FS.syncfs(true, r));
	console.log("synced; exposing dotnet FS");

	window.FS = dotnet.instance.Module.FS;
	setModuleImports("main.js", {
		setMainLoop: MainLoop,
		syncFs: (cb) => dotnet.instance.Module.FS.syncfs(false, cb),
	});
}

let ts = performance.now();
export let fps;
const MainLoop = (cb) => {
	dotnet.instance.Module.setMainLoop(() => {
		let now = performance.now();
		let dt = now - ts;
		ts = now;
		fps = 1000 / dt;

		cb();
	});
};

export async function start(canvas) {
	console.info("Starting...");

	if (!dotnet.instance.Module.FS.analyzePath("/Content").path) {
		await new Promise(r => loadData(dotnet.instance.Module, r));
		console.info("Loaded assets into VFS");
	}

	if (window.assetblob) {
		URL.revokeObjectURL(window.assetblob);
	}

	dotnet.instance.Module.canvas = canvas;

	await dotnet.run();

	let Exports = await getAssemblyExports("fna-wasm");

	Exports.Program.StartGame();
};

export async function downloadsave() {
	await new Promise(r => dotnet.instance.Module.FS.syncfs(false, r));

	let tozip = {};

	let saves = dotnet.instance.Module.FS.readdir("/libsdl/Celeste/Saves");
	for (let save of saves) {
		if (save === "." || save === "..") continue;
		let savepath = `/libsdl/Celeste/Saves/${save}`;
		let data = dotnet.instance.Module.FS.readFile(savepath);
		tozip[save] = data;
	}
	const zipped = await new Promise((r, j) => zip(tozip, (e, data) => { if (!e) { r(data) } else { j(e) } }));

	let blob = new Blob([zipped], { type: "application/zip" });
	let url = URL.createObjectURL(blob);
	let a = h("a", { href: url, download: "saves.zip" });
	a.click();
	a.remove();
}

export async function unzipsave(file) {
	let data = await file.arrayBuffer();
	let unzipped = await new Promise((r, j) => unzip(new Uint8Array(data), (e, data) => { if (!e) { r(data) } else { j(e) } }));
	let files = Object.entries(unzipped);

	if (!dotnet.instance.Module.FS.analyzePath("/libsdl/Celeste").path) {
		dotnet.instance.Module.FS.mkdir("/libsdl/Celeste", 0o755);
	}

	if (!dotnet.instance.Module.FS.analyzePath("/libsdl/Celeste/Saves").path) {
		dotnet.instance.Module.FS.mkdir("/libsdl/Celeste/Saves", 0o755);
	}

	for (let [name, data] of files) {
		dotnet.instance.Module.FS.writeFile(`/libsdl/Celeste/Saves/${name}`, data);
	}

	await new Promise(r => dotnet.instance.Module.FS.syncfs(false, r));
	console.log("synced!");
}

export async function uploadsave() {
	let input = h("input", { type: "file" });
	await new Promise(r => {
		input.onchange = async () => {
			let file = input.files[0];
			unzipsave(file);
			r();
		};
		input.click();
		input.remove();
	})
}
