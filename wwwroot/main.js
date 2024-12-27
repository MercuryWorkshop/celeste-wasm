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
	dotnet,
	runtime,
	config,
	exports,
	canvas
};

const loop = () => {
	try {
		exports.Main.MainLoop();
	} catch(err) {
		console.error("err", err);
		return;
	}
	requestAnimationFrame(loop);
}
requestAnimationFrame(loop);
