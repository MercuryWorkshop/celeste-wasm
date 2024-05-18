let ofetch = fetch;


console.log("???");
async function concatenate(uint8arrays) {
  // Put the inputs into a Blob.
  const blob = new Blob(uint8arrays);

  // Pull an ArrayBuffer out. (Has to be async.)
  const buffer = await blob.arrayBuffer();

  // Convert that ArrayBuffer to a Uint8Array.
  return new Uint8Array(buffer);
}

const importmap = {};

window.aload = () => {

  let maxassets = 1591;
  let count = 0;
  ofetch("./assets.json").then((res) => {
    console.log("fetched..");

    const reader = res.body.getReader();
    return new ReadableStream({
      start(controller) {

        let decoder = new TextDecoder();


        let buf = new Uint8Array();

        return pump();
        function pump() {
          return reader.read().then(async ({ done, value }) => {
            if (done) {
              controller.close();
              return;
            }

            if (buf.length > 0)
              buf = await concatenate([buf, value]);
            else
              buf = value;

            let nl = buf.indexOf(0x0A);
            while (nl != -1) {
              let line = buf.subarray(0, nl);
              buf = buf.subarray(nl + 1);

              let sep = line.indexOf(0x20);
              let url = decoder.decode(line.subarray(0, sep));
              let content = decoder.decode(line.subarray(sep + 1));


              count++;
              console.log("loading", count, "of", maxassets, url);
              importmap[url] = content;
              nl = buf.indexOf(0x0A);
            }


            controller.enqueue(value);
            return pump();
          });
        }
      },
    });


  });
}

globalThis.fetch = async (url) => {

  url = url.replaceAll("./", "");
  url = url.replaceAll("http://localhost:8081/", "");
  console.log(url);

  return await ofetch("data:text/plain;base64," + importmap[url])
}

window.importfill = (url) => {

  url = url.replaceAll("./", "");
  url = url.replaceAll("http://localhost:8081/", "");

  console.log(url)
  return import("data:text/plain;base64," + importmap[url])
}
