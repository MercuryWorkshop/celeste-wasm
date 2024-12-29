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

console.log("3...");
await new Promise(r => setTimeout(r, 1000));
console.log("2...");
await new Promise(r => setTimeout(r, 1000));
console.log("1...");
await new Promise(r => setTimeout(r, 1000));

console.log("PreInit...");
await runtime.runMain();
await exports.Program.PreInit();

console.log("Init...");
await new Promise(r => setTimeout(r, 1000));

exports.Program.Init();

let avgTop = 0;
let avgBtm = 0;

setInterval(() => {
	if (avgBtm) {
		console.log(`avg frametime: ${(avgTop / avgBtm).toFixed(3)}ms ${(avgBtm / 2.5).toFixed(3)}FPS`);
		avgTop = 0;
		avgBtm = 0;
	}
}, 2500);

const mainloop = () => {
	const before = performance.now();
	exports.Program.MainLoop();
	const after = performance.now();

	avgTop += after - before;
	avgBtm++;

	requestAnimationFrame(mainloop);
}
requestAnimationFrame(mainloop);
