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
  // ofetch("./assets.json").then((res) => {
  console.log("fetched..");

  // const reader = res.body.getReader();

  const reader = document.body.querySelector("input[type=file]").files[0].stream().getReader();
  const queueingStrategy = new ByteLengthQueuingStrategy({ highWaterMark: 1024 * 1024 * 1000 });
  return new ReadableStream({
    start(controller) {

      let decoder = new TextDecoder();


      let buf = new Uint8Array();

      let bufs = [];


      let wants = 0;

      return pump();
      function pump() {
        return reader.read().then(async ({ done, value }) => {
          if (done) {
            controller.close();
            console.log("Stream complete");
            return;
          }

          try {

            let wait = false;

            if (wants > 0) {
              bufs.push(value);

              let l = bufs.reduce((a, b) => a + b.length, 0);
              if (l + buf.length < wants) {
                wait = true;
              } else {
                buf = await concatenate([buf, ...bufs]);
                bufs = [];
                wants = 0;
              }
            }
            else
              buf = value;


            if (!wait) {
              // console.log("got data..", buf.length);

              let dv = new DataView(buf.buffer);

              let i = 0;

              while (buf.length > 0) {
                let oldi = i;
                let namelen = dv.getUint32(i, false);
                i += 4;
                if (i > buf.length) {
                  buf = buf.subarray(oldi);
                  wants = namelen + 1000;
                  break;
                }
                let filename = decoder.decode(buf.subarray(i, i + namelen));
                i += namelen;

                if (i > buf.length) {
                  buf = buf.subarray(oldi);
                  wants = namelen + 1000;
                  break;
                }
                let len = dv.getUint32(i, false);
                i += 4;

                if (i + len > buf.length) {
                  buf = buf.subarray(oldi);
                  wants = namelen + len + 1000;
                  break;
                }

                let blob = new Blob([buf.subarray(i, i + len)], { type: "text/javascript" });

                importmap[filename] = blob;
                console.log(filename, len);

                i += len;
              }
            }

          } catch (e) {
            console.error(e);
          }


          controller.enqueue(value);
          return pump();
        });
      }
    },
    queueingStrategy
  });


  // });
}


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
  console.log("fetching..", url);

  let blob = getblob(url);
  return await ofetch(blob);
}

window.importfill = (url) => {
  console.log(url);

  let blob = getblob(url);
  return import(blob);
}
