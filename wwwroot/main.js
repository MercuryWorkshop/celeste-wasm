// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from "./_framework/dotnet.js";

const version = "4.0.0.0";

let store = $store(
	{
		theme: (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) ? "light" : "dark",
		debug: false,
	},
	{ ident: "user-options", backing: "localstorage", autosave: "auto" }
);


function App() {
    this.css = `
    width: 100vw;
    height: 100vh;
    padding: 1em;
    margin: 0;
    position: relative;
    background-color: var(--bg);
    color: var(--fg);

    overflow-x: hidden;



    canvascontainer {
        display: grid;
        width: 100%;
        border: 0.7px solid var(--surface5);
        border-radius: 0.5em;
        overflow: hidden;

        & > * {
            grid-area: 1 / 1;
        }

        & > div {
            user-select: none;
            text-align: center;
            color: var(--fg6);
            font-size: 1.5em;
            font-weight: 550;

            z-index: 5;
            width: 100%;
            height: 100%;
            display: flex;
            justify-content: center;
            align-items: center;

            .material-symbols-rounded {
                font-size: 3em;
            }
        }
    }
    canvas {
        width: 100%;
        display: block;
        background-color: var(--surface0);
    }
    .pinned {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: auto;
    }

    .hidden {
        display: none;
    }

    button,
    input::-webkit-file-upload-button {
        user-select: none;
        background-color: var(--surface1);
        padding: 0.5em 1em;
        color: var(--fg);
        border: none;
        border-radius: 0.5em;
        cursor: pointer;
        transition: background-color 0.2s;
        &:hover {
            background-color: var(--surface3);
        }
        &:active {
            background-color: var(--surface5);
        }

        &:has(.material-symbols-rounded) {
          display: flex;
          align-items: center;
          justify-content: center;
          padding: 0;
          width: 2rem;
          height: 2rem;
          border-radius: 50%;
        }

        .material-symbols-rounded {
            font-size: 1.3rem;
            margin: 0;
            padding: 0;
        }

        &.important {
            background-color: var(--accent);

            &:hover {
                background-color: color-mix(in srgb, var(--accent) 80%, white);
            }

            &:active {
                background-color: color-mix(in srgb, var(--accent) 70%, white);
            }
        }
    }

    pre {
        overflow-y: scroll;
        font-size: 0.8em;
        max-height: 20em;
        border: 0.7px solid var(--surface5);
        border-radius: 0.7em;
        padding: 1em;
        background-color: var(--surface0);
        font-family: monospace;
    }

    #logo {
      image-rendering: pixelated;
      -ms-interpolation-mode: nearest-neighbor;
    }

    h1 {
      font-size: 2rem;
      display: flex;
    }

    h1 subt {
      font-size: 0.5em;
      margin-left: 0.25em;
      color: var(--fg6);
    }
`;

    this.started = false;

  this.fullscreen = false;


    document.addEventListener("fullscreenchange", () => {
        this.fullscreen = document.fullscreen;
    });

    let ts = performance.now();
    let fps;
    const MainLoop = (cb) => {
        dotnet.instance.Module.setMainLoop(() => {
            let now = performance.now();
            let dt = now - ts;
            ts = now;
            fps = 1000 / dt;

            cb();
        });
    };

    setInterval(() => {
        this.fps = fps;

        // todo: get a proper hook for initialization
        //this.canvas.removeAttribute("width");
        //this.canvas.removeAttribute("height");
    }, 5000);

    const start = async () => {
        this.started = true;

        // we load the asset manifest early so that we can use it to set the dotnet config
        let assetManifest = await globalThis.fetch("asset_manifest.csv");
        let assetManifestText = "";
        if (!assetManifest.ok) {
            log("Unable to load asset manifest");
            log(assetManifest);
        } else {
            assetManifestText = await assetManifest.text();
        }
        let assetList = assetManifestText
            .split("\n")
            .filter((i) => i)
            .map((i) => i.trim().replace("\\", "/"));
        log(`Found ${assetList.length} assets in manifest`);

        const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
            .withModuleConfig({
                onConfigLoaded: (config) => {
                    if (!config.resources.vfs) {
                        config.resources.vfs = {};
                    }

                    for (let asset of assetList) {
                        asset = asset.trim().replace(/^\/assets\//, "");
                        log(`Found ${asset}, adding to VFS`);
                        config.resources.vfs[asset] = {};
                        const assetPath = `../assets/${asset}`;
                        config.resources.vfs[asset][assetPath] = null;
                    }
                },
            })
            .withDiagnosticTracing(false)
            .withApplicationArgumentsFromQuery()
            .create();

        dotnet.instance.Module.FS.mkdir("/libsdl", 0o755);
        dotnet.instance.Module.FS.mount(
            dotnet.instance.Module.FS.filesystems.IDBFS,
            {},
            "/libsdl",
        );
        await new Promise((r) => dotnet.instance.Module.FS.syncfs(true, r));
        log("synced; exposing dotnet FS");
        window.FS = dotnet.instance.Module.FS;

        setModuleImports("main.js", {
            setMainLoop: MainLoop,
            syncFs: (cb) => dotnet.instance.Module.FS.syncfs(false, cb),
        });

        dotnet.instance.Module.canvas = this.canvas;

        await dotnet.run();

        let Exports = await getAssemblyExports("fna-wasm");

    Exports.Program.SetConfig(store.debug);

        Exports.Program.StartGame();
    };

  return html`
    <main class=${[
      use(store.theme)
    ]}>
      <div class="flex vcenter gap space-between" style="padding-bottom: 1em;">
        <span class="flex vcenter gap left">
          <span class="flex vcenter">
            <img id="logo" src="/assets/app.ico" width="64" height="64" />
            <h1>celeste-wasm<subt>v${version}</subt></h1>
          </span>
          ${$if(
            use(this.started),
            html` <p>FPS: ${use(this.fps, Math.floor)}</p> `,
          )}

          <div>
            <label for="debug">Debug: </label>
            <input type="checkbox" bind:checked=${use(store.debug)} />
          </div>
        </span>
        <span class="flex gap right vcenter">
          <div class="flex gap-sm vcenter ">
          <button on:click=${() => {
            if (store.theme === "light") {
              store.theme = "dark";
            } else {
              store.theme = "light";
            }
          }}>
            <span class="material-symbols-rounded">${
            use(store.theme, (theme) => (theme === "light" ? "dark_mode" : "light_mode"))
            }</span>
          </button>
            <button
              on:click=${() => {
                if (this.canvas.requestFullscreen()) {
                  this.fullscreen = true;
                }
              }}
            >
              <span class="material-symbols-rounded">fullscreen</span>
            </button>
            <button
              class=${[
                use(this.started, (started) => (started ? "s" : "important")),
              ]}
              on:click=${start}
            >
              <span class="material-symbols-rounded">play_arrow</span>
            </button>
          </div>
          <input type="file" id="upload" />
        </span>
      </div>

      ${(navigator.userAgent.includes("Firefox") && html`<${FuckMozilla} />`) ||
        ""}

      <canvascontainer>
        <div class=${[use(this.started, (f) => f && "hidden")]}>
          <div>
            <span class="material-symbols-rounded">videogame_asset_off</span>
            <br>
            <span>Game not running.</span>
          </div>
        </div>
        <canvas
          id="canvas"
          class=${[use(this.fullscreen, (f) => f && "pinned")]}
          bind:this=${use(this.canvas)}
        ></canvas>
      </canvascontainer>

      <h2>Log</h2>
      <pre bind:this=${use(this.log)}></pre>
    </main>
  `;
}

function FuckMozilla() {
    this.css = `
        width: 100%;

        background-color: var(--accent);
        color: var(--fg);
        padding: 1em;
        padding-top: 0.5em;
        margin-bottom: 1em;
        border-radius: 0.5em;

    `;

    return html`
        <div>
            <h1>THIS DOESN'T (MIGHT NOT) WORK ON FIREFOX!!!</h1>
            <p>i don't know why and i don't feel like fixing it. use chromium please </p>
        </div>
    `
}

const app = h(App).$;

let olog = console.log;

let logs = [];
let ringsize = 200;
export function log(...args) {
    olog(...args);
    logs.push(args.join(" ") + "\n");
    if (logs.length > ringsize) {
        logs.shift();
    }
}
setInterval(() => {
    app.log.innerText = logs.join("\n");
    app.log.scrollTop = app.log.scrollHeight;
}, 5000);

// console.log = log

document.body.appendChild(app.root);
