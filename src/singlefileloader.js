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

  let blob = importmap[url];

  if (!blob) throw new Error("Asset not found: " + url);

  return URL.createObjectURL(blob);
}


async function unpak_wasm() {
  let zbuf = Uint8Array.from(atob(wasm_pak.innerText), c => c.charCodeAt(0))
  wasm_pak.remove();

  await zdecoder.init();
  let buf = await zdecoder.decode(zbuf, WASM_PACK_SIZE);
  let decoder = new TextDecoder();

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

async function loadfrompacked() {
  let blob = b64toBlob(game_data.innerText, "text/javascript");
  game_data.remove();
  window.xorbuf = new Uint8Array(await blob.arrayBuffer());
}

function loadfromfile() {
  let input = h("input", { type: "file" });
  document.body.appendChild(input);
  input.addEventListener("change", async () => {
    let file = input.files[0];
    let blob = new Blob([await file.arrayBuffer()], {
      type: "text/javascript",
    });

    window.assetblob = URL.createObjectURL(blob);

  });
  input.click();

}
