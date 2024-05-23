import "dreamland/dev";

import { Logs, log } from "./Logs.js";
import { App, Logo } from "./App.js";

export let store = $store(
    {
        theme: (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) ? "light" : "dark",
    },
    { ident: "user-options", backing: "localstorage", autosave: "auto" }
);

if (window.SINGLEFILE) {
    document.body.querySelector("#interstitial").remove();
}

function IntroSplash() {
    this.css = `

    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    margin: 0;
    background-color: var(--bg);
    color: var(--fg);

    display: flex;
    justify-content: center;
    align-items: center;

    .info {
        border-radius: 1em;
        background-color: var(--surface0);
        max-width: 90%;

        padding: 2em;

        pre {
            display: inline!important;
        }

        animation: fadein 0.7s ease 0s 1;
    }

    button,
    #bar {
      animation: fadein 0.15s ease 0s 1;
    }

    #progress {
      transition: width 0.2s ease;
    }

    .inner {
      margin-inline: 0.8rem;
    }

    pre {
      font-size: 1rem;
    }

    @keyframes fadein {
        from { opacity: 0; transform: translateY(1em); }
        to { opacity: 1; transform: translateY(0); }
    }
    `

    let encbuf;
    this.progress = 0;

    this.downloading = false;
    this.downloaded = false;
    this.showprogress = true;

    if (window.SINGLEFILE) {
        this.downloaded = true;
        this.downloading = true;
        this.showprogress = false;
        this.progress = 100;
        encbuf = window.xorbuf;
        window.xorbuf = null;
    }
    this.decrypted = !DRM;

    this.decrypterror = "";



    let download = async () => {
        this.downloading = true;
        this.progress = 0;

        if (SPLIT) {
            encbuf = new Uint8Array(SIZE);

            let cur = 0;
            for (let split of splits) {
                let data = await fetch(`_framework/data/${split}`);
                let buf = new Uint8Array(await data.arrayBuffer());

                encbuf.set(buf, cur);
                cur += buf.length;

                this.progress += 100 / splits.length;
            }
        } else {
            let data = await fetch("_framework/data.data");
            this.progress = 20;
            let buf = await data.arrayBuffer();
            this.progress = 100;
            encbuf = new Uint8Array(buf);
        }

        this.downloaded = true;
    };

    let decrypt = async () => {
        let input = h("input", { type: "file" });

        input.addEventListener("change", async () => {
            if (input.files.length === 0) {
                this.decrypterror = "No file selected";
                return;
            }
            let file = input.files[0];

            if (file.size !== 3072) {
                this.decrypterror = "Invalid key file (or a different version of the game?)";
                return;
            }

            let reader = new FileReader();
            reader.onload = async () => {
                let key = new Uint8Array(reader.result);

                this.progress = 0;
                for (let i = 0; i < encbuf.length; i += 4096) {
                    encbuf[i] ^= key[i % key.length];

                    if (i % (4096 * 200) == 0) {
                        this.progress = i / encbuf.length * 100;
                        await new Promise(r => setTimeout(r, 0));
                    }
                }
                this.decrypted = true;
            };
            reader.readAsArrayBuffer(file);
        });

        document.body.appendChild(input);
        input.click();
        input.remove();
    };

    let finish = async () => {

        window.assetblob = URL.createObjectURL(new Blob([encbuf]));

        await loadfrontend();
        this.root.remove();
    };

    return html`
        <main class=${[use(store.theme)]}>
            <div class="info">
                <${Logo}/>
                <div class="inner">
                <div>
                This is a mostly-complete port of <a href="https://store.steampowered.com/app/504230/Celeste/">Celeste</a> to the browser using dotnet's <pre>wasmbrowser</pre> template and <a href="https://github.com/RedMike/FNA.WASM.Sample">FNA.WASM.Sample</a>.<br>
                </div>
                <div>It needs around 1.6GB of memory and will probably not work on mobile devices.<br><br></div>
                ${DRM && "You will need to own Celeste to play this. Make sure you have it downloaded and installed on your computer." || ""}<br>
                ${!window.SINGLEFILE && "This will download around ~700MB of assets to your browser's local storage." || ""}<br><br>

                ${$if(use(this.downloading),
        $if(use(this.showprogress), html`
          <div>
          <p>Downloading... (${use(this.progress)}% done)</p>
          <div id="bar" style="width: 100%; height: 0.5em; background-color: var(--surface1); border-radius: 0.2em;">
            <div id="progress" style=${{
              width: use`${this.progress}%`,
              height: "0.5em",
              backgroundColor: "var(--accent)",
              borderRadius: "0.2em"
            }}></div>
          </div>
          </div>
        `),
        html`<button class="important" on:click=${download}><span class="material-symbols-rounded">download</span><span class="label">Download Assets</span></button>`,

    )}
    <br>


            ${$if(use(this.downloaded),
        $if(use(this.decrypted),
            html`<button class="important" on:click=${finish}><span class="material-symbols-rounded">stadia_controller</span><span class="label">Play</span></button>`,
            html`
                <div>
                    <p>Downloaded assets. Now you need to decrypt them. Find the game files directory for your copy of Celeste and upload Celeste.Content.dll.</p>
                    <button class="important" on:click=${decrypt}><span class="material-symbols-rounded">encrypt</span><span class="label">Decrypt</span>s</button>
                    <br>
                    <p>${use(this.decrypterror)}</p>
                </div>`
        ))}
            </div>
            </div>
        </main>
    `
}


async function loadfrontend() {
    const app = h(App).$;

    document.body.appendChild(app.root);
}

if (localStorage["vfs_populated"] !== "true") {
    document.body.appendChild(h(IntroSplash));
} else {
    loadfrontend();
}
