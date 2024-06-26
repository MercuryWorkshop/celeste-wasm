import { ZSTDDecoder } from 'zstddec';
let zdecoder = new ZSTDDecoder();
window.SINGLEFILE = 1;

// only runs in singlefile mode, runs before the rest of the frontend
// loads assets and wasm.pak from the html 

let ofetch = fetch;
const importmap = {};

function getblob(url) {
	url = decodeURI(url);
	url = url.replaceAll("./", "");

	let folder = location.href.split("/").slice(0, -1).join("/") + "/";
	let folder2 = location.href.split("/").slice(0, -2).join("/") + "/";
	url = url.replaceAll(folder, "");
	url = url.replaceAll(folder2, "");

	let q = url.indexOf("?");
	if (q !== -1) {
		url = url.substring(0, q);
	}

	let blob = importmap[url];

	if (!blob) throw new Error("Asset not found: " + url);

	return URL.createObjectURL(blob);
}


window.unpak_wasm = async function() {
	if (wasm_pak.innerText.length > 100) {
		let zbuf = Uint8Array.from(atob(wasm_pak.innerText), c => c.charCodeAt(0))
		wasm_pak.remove();
		await unpak(zbuf);
	} else {
		await new Promise((resolve) => {
			document.querySelector("h1").innerText = "Please upload wasm.pak";
			document.querySelector("#interstitial").style.height = "10vh";
			let input = document.createElement("input");
			input.type = "file";
			document.body.appendChild(input);
			input.addEventListener("change", async () => {
				let file = input.files[0];

				let buf = await file.arrayBuffer();

				await unpak(new Uint8Array(buf));

				input.remove();
				resolve();
			});
		});
	}
}

async function unpak(zbuf) {
	await zdecoder.init();
	let buf = zdecoder.decode(zbuf, WASM_PACK_SIZE);
	let decoder = new TextDecoder();

	if (buf.length == 0) {
		alert("bad wasm.pak!! redownload it and reload");
		window.location.reload();
	}
	console.log("loaded wasm.pak: ", buf.length);

	let dv = new DataView(buf.buffer);


	let i = 0;

	while (i < buf.length) {
		let namelen = dv.getUint32(i, false);
		i += 4;

		let filename = decoder.decode(buf.subarray(i, i + namelen));
		i += namelen;


		let len = dv.getUint32(i, false);
		i += 4;


		let blob = new Blob([buf.subarray(i, i + len)], {
			type: "text/javascript",
		});

		importmap[filename] = blob;
		console.info("loaded from wasm.pak " + filename);

		i += len;
	}


	globalThis.fetch = async (url) => {
		let blob = getblob(url);
		return await ofetch(blob);
	};

	window.importfill = (url) => {
		console.warn("filled import " + url);

		let blob = getblob(url);
		return import(blob);
	};

}

const b64toBlob = (b64Data, contentType = '', sliceSize = 512) => {
	const byteCharacters = atob(b64Data);
	const byteArrays = [];

	for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
		const slice = byteCharacters.slice(offset, offset + sliceSize);

		const byteNumbers = new Array(slice.length);
		for (let i = 0; i < slice.length; i++) {
			byteNumbers[i] = slice.charCodeAt(i);
		}

		const byteArray = new Uint8Array(byteNumbers);
		byteArrays.push(byteArray);
	}

	const blob = new Blob(byteArrays, { type: contentType });
	return blob;
}

window.loadfrompacked = async function() {
	if (game_data.innerText.length > 100) {
		let blob = b64toBlob(game_data.innerText, "text/javascript");
		game_data.remove();
		window.xorbuf = new Uint8Array(await blob.arrayBuffer());
	} else {
		console.log("loading from file");
		await loadfromfile();
	}
}

async function loadfromfile() {
	await new Promise((resolve) => {
		document.querySelector("h1").innerText = "Please upload data.data";
		let input = document.createElement("input");
		input.type = "file";
		document.body.appendChild(input);
		input.addEventListener("change", async () => {
			let file = input.files[0];

			let buf = await file.arrayBuffer();
			window.xorbuf = new Uint8Array(buf);

			input.remove();
			resolve();
		});
	});
}
