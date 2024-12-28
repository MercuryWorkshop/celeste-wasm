import { dotnet } from './_framework/dotnet.js'

const div = document.getElementById("console");
console.log = (str) => {
	const el = document.createElement("pre");
	el.style = "color:white;"
	el.textContent = str;
	div.insertBefore(el, div.firstChild);
}
console.warn = (str) => {
	const el = document.createElement("pre");
	el.style = "color:yellow;"
	el.textContent = str;
	div.insertBefore(el, div.firstChild);
}
console.error = (str) => {
	const el = document.createElement("pre");
	el.style = "color:red;"
	el.textContent = str;
	div.insertBefore(el, div.firstChild);
}

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

await new Promise(r => setTimeout(r, 1000));

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
