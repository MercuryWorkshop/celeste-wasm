import { dotnet } from './_framework/dotnet.js'

const runtime = await dotnet
	.withConfig({
		jsThreadBlockingMode: "DangerousAllowBlockingWait",
	})
	.create();
const config = runtime.getConfig();

const exports = await runtime.getAssemblyExports(config.mainAssemblyName);

const canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;

window.wasm = {
	Module: dotnet.instance.Module,
	dotnet,
	runtime,
	config,
	exports,
	canvas
};

await runtime.runMain();

await new Promise(r=>setTimeout(r, 1000));

let avgTop = 0;
let avgBtm = 0;

const mainloop = () => {
	const before = performance.now();
	exports.Program.MainLoop();
	const after = performance.now();

	avgTop += after - before;
	avgBtm++;

	if (avgBtm >= 60) {
		console.log("avg frametime: " + avgTop / avgBtm);
		avgTop = 0;
		avgBtm = 0;
	}

	requestAnimationFrame(mainloop);
}
requestAnimationFrame(mainloop);
