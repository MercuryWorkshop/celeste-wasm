let ofetch = fetch;
window.SINGLEFILE = 1;
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

globalThis.fetch = async (url) => {
  let blob = getblob(url);
  return await ofetch(blob);
};

window.importfill = (url) => {
  console.warn("filled import " + url);

  let blob = getblob(url);
  return import(blob);
};


async function load() {
  let f = await ofetch("data:application/octet-stream;base64," + wasm_pak.innerText);
  let buf = new Uint8Array(await f.arrayBuffer());
  wasm_pak.remove();
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
}

function loadfrompacked() {
  window.assetblob = "data:application/octet-stream;base64," + game_data.innerText;

  game_data.remove();
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

load();
loadfrompacked();
