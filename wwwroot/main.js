import { Logs, log } from "./Logs.js";
import { App, Logo } from "./App.js";
import { init } from "./game.js";

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
            display: inline;
        }
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
                This is a mostly-complete port of <a href="https://store.steampowered.com/app/504230/Celeste/">Celeste</a> to the browser using dotnet's <pre>wasmbrowser</pre> template, <a href="https://github.com/RedMike/FNA-WASM-Build">FNA WASM libraries</a>, and <a href="https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM">FNA.WASM.Sample guide</a>.
                <br>
                It needs around 1.6gb of memory and will probably not work on mobile devices.
                <br><br>


                ${DRM && "You will need to own Celeste to play this. Make sure you have it downloaded and installed on your computer." || ""}<br>
                ${!window.SINGLEFILE && "This will download around  ~700mb of assets to your browser's local storage." || ""}<br><br>
            

                ${$if(use(this.downloading),
        $if(use(this.showprogress), html`<p> progress: <progress value=${use(this.progress)} max="100"></progress></p>`),
        html`<button on:click=${download}>Download Assets</button>`,

    )}
    <br>


            ${$if(use(this.downloaded),
        $if(use(this.decrypted),
            html`<button on:click=${finish}>Continue</button>`,
            html`
                <div>
                    <p>Downloaded assets. Now you need to decrypt them. Find the game files directory for celeste and upload Celeste.Content.dll</p>
                    <button on:click=${decrypt}>Decrypt Assets</button>
                    <br>
                    <p>${use(this.decrypterror)}</p>
                </div>`
        ))}
            </div>
        </main>
    `
}


async function loadfrontend() {
    const app = h(App).$;

    document.body.appendChild(app.root);

    log("var(--success)", "Loaded frontend!");
}

if (localStorage["vfs_populated"] !== "true") {
    document.body.appendChild(h(IntroSplash));
} else {
    loadfrontend();
}
