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
    display: flex;
    flex-direction: column;
    align-items: center;
    margin: 0;
    background-color: var(--bg);
    color: var(--fg);

    overflow-x: hidden;

    .game {
        margin-inline: 2em;
        display: flex;
        align-items: center;
        justify-content: center;

        canvascontainer {
            display: grid;
            min-width: min(960px, 100%);
            min-height: min(540px, 100%);
            max-height: 85vh;
            max-width: 100%;
            aspect-ratio: 16 / 9;
            border: 0.7px solid var(--surface5);
            border-radius: 0.6rem;
            overflow: hidden;
            background-color: var(--surface1);

            &:has(.hidden) {
              background-color: black;
            }

            & > * {
                grid-area: 1 / 1;
            }

            & > div {
                &.hidden {
                    display: none;
                }

                .material-symbols-rounded {
                    font-size: 3em;
                }

                h3 {
                    margin: 0.2rem;
                    font-weight: 570;
                }

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
            }
        }
        canvas {
            width: 100%;
            display: block;
        }
    }
    .pinned {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: auto;
    }

    button,
    input::-webkit-file-upload-button,
    .fakebutton {
        user-select: none;
        background-color: var(--surface1);
        padding: 0.5em 1em;
        color: var(--fg);
        border: none;
        border-radius: 0.6rem;
        cursor: pointer;
        transition: background-color 0.2s;
        font-size: 0.95rem;
        font-family: var(--font-body);
        font-weight: 500;
        &:hover {
            background-color: var(--surface3);
        }
        &:active {
            background-color: var(--surface5);
        }

        &:has(.material-symbols-rounded):not(:has(.label)) {
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

    input[type="file"] {
        display: none; /* this SUCKS for accessibility but since when did we care about that again */
    }

    .logs {
      width: min(960px, 100%);
      pre {
          overflow-y: scroll;
          font-size: 0.9em;
          max-height: 32em;
          border: 0.7px solid var(--surface5);
          border-radius: 0.7em;
          padding: 1em;
          background-color: var(--surface0);
          font-family: var(--font-mono);
          min-height: 16em;
      }
    }

    #logo {
      image-rendering: pixelated;
      -ms-interpolation-mode: nearest-neighbor;
      width: 3.75rem;
      height: 3.75rem;
      margin: 0;
      padding: 0;
    }

    .top-bar {
      margin-bottom: 1.5em;
      padding-inline: 1.7em;
      background-color: var(--surface0);

      h1 {
        font-size: 2rem;
        display: flex;
      }

      h1 subt {
        font-size: 0.5em;
        margin-left: 0.25em;
        color: var(--fg6);
      }
    }

    h1,
    h2,
    h3,
    h4,
    h5,
    h6 {
      font-family: var(--font-display);
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
    }, 1000);

    const start = async () => {
        this.started = true;
        console.info("Starting...");

        const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
            .withDiagnosticTracing(false)
            .withApplicationArgumentsFromQuery()
            .create();


        await new Promise(r => loadData(dotnet.instance.Module, r));
        console.info("Loaded assets into VFS");

        dotnet.instance.Module.FS.mkdir("/libsdl", 0o755);
        dotnet.instance.Module.FS.mount(
            dotnet.instance.Module.FS.filesystems.IDBFS,
            {},
            "/libsdl",
        );
        await new Promise((r) => dotnet.instance.Module.FS.syncfs(true, r));
        console.log("synced; exposing dotnet FS");
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
      <div class="flex vcenter gap space-between top-bar">
        <span class="flex vcenter gap left">
          <span class="flex vcenter">
            <img id="logo" src="/assets/app.ico" />
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
        <span class="flex gap-md right vcenter">
          <button on:click=${() => {
            if (store.theme === "light") {
                store.theme = "dark";
            } else {
                store.theme = "light";
            }
        }}>
            <span class="material-symbols-rounded">${use(store.theme, (theme) => (theme === "light" ? "dark_mode" : "light_mode"))
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
          <label for="upload" class="flex vcenter gap-sm fakebutton"><span class="material-symbols-rounded">upload</span><span class="label">Upload Data</span></label>
          <input type="file" id="upload" />
        </span>
      </div>

      ${(navigator.userAgent.includes("Firefox") && html`<${FuckMozilla} />`) ||
        ""}

      <div class="game">
        <canvascontainer>
          <div class=${[use(this.started, (f) => f && "hidden")]}>
            <div>
              <span class="material-symbols-rounded">videogame_asset_off</span>
              <br>
              <h3>Game not running.</h3>
            </div>
          </div>
          <canvas
            id="canvas"
            class=${[use(this.fullscreen, (f) => f && "pinned")]}
            bind:this=${use(this.canvas)}
          ></canvas>
        </canvascontainer>
      </div>

      <div class="logs">
      <h2>Log</h2>
      <pre bind:this=${use(this.log)}></pre>
      </div>
    </main>
  `;
}

function FuckMozilla() {
    this.css = `
        width: min(960px, 100%);

        background-color: var(--accent);
        color: var(--fg);
        padding: 1em;
        padding-top: 0.5em;
        margin-bottom: 1em;
        border-radius: 0.6rem;


        h1 {
          display: flex;
          align-items: center;
          gap: 0.25em;
          span {
            font-size: 2.25rem;
          }
        }
    `;

    return html`
        <div>
            <h1>
            <span class="material-symbols-rounded">warning</span>
            THIS MIGHT NOT WORK WELL ON FIREFOX
            </h1>
            <p>The chromium WASM implemenation is generally better, and it was what was tested on the most. It will probably still work (might not!) but you should be using chromium</p>

            <button on:click=${() => this.root.remove()}>Dismiss</button>
        </div>
    `
}

const app = h(App).$;

let olog = console.log;

let logs = [];
let ringsize = 200;
export function ilog(color, ...args) {
    olog(...args);
    logs.push([color, `[${new Date().toISOString()}] ` + args.join(" ") + "\n"]);
}

console.log = (...args) => {
    ilog("var(--fg)", ...args)
}
console.warn = (...args) => {
    ilog("var(--warning)", ...args)
}
console.error = (...args) => {
    ilog("var(--error)", ...args)
}
console.info = (...args) => {
    ilog("var(--info)", ...args)
}
console.debug = (...args) => {
    ilog("var(--fg6)", ...args)
}

document.body.appendChild(app.root);

ilog("var(--success)", "Loaded frontend!");

setInterval(() => {
    for (let log of logs) {
        app.log.append(h("span", { style: { color: log[0] } }, log[1]));

        if (app.log.children.length > ringsize) {
            app.log.children[0].remove();
        }
    }
    logs = []

    app.log.scrollTop = app.log.scrollHeight
}, 1500);
