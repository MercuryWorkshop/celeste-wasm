import { Logs, log } from "./Logs.js";
import { FsExplorer } from "./FsExplorer.js";
import { fps, version, init, start } from "./game.js";
import { store } from "./main.js";

export function App() {
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

    .logs {
      width: min(960px, 100%);
    }

    .top-bar {
      margin-bottom: 1.5em;
      padding-inline: 1.7em;
      border: none;
      background-color: var(--surface0);
    }

    dialog:-internal-dialog-in-top-layer::backdrop {
      background: color-mix(in srgb, black 35%, color-mix(in srgb, var(--bg), transparent 70%))!important;
    }

    #fs {
      background: var(--bg);
      color: var(--fg);
      border: 0.1em solid var(--surface5);
      border-radius: 0.5em;
      opacity: 0;
      scale: 0.8;
      width: 60vw;

      transition: opacity 0.2s ease, scale 0.2s ease;

      &[open] {
        opacity: 1;
        scale: 1;
        transition: opacity 0.2s ease, scale 0.2s ease;
      }

      button {
        float: right;
      }
    }

  footer {
    display: flex;
    justify-content: space-between;
    gap: 0.5em;
    padding: 0.5em;
    border-top: 0.1em solid var(--surface5);
    background-color: var(--surface0);
    padding-top: 2em;
  }
`;

    this.loaded = false;
    this.started = false;
    this.allowPlay = false;

    let updatePlay = () => { this.allowPlay = this.loaded || this.started };
    handle(use(this.loaded), updatePlay);
    handle(use(this.started), updatePlay);

    window.initPromise = (async () => {
        await init();
        this.loaded = true;
        log("var(--success)", "Loaded frontend!");
    })();

    this.fullscreen = false;

    document.addEventListener("fullscreenchange", () => {
        this.fullscreen = document.fullscreen;
    });

    setInterval(() => {
        this.fps = fps;
    }, 1000);

    const startgame = () => {
        this.started = true;

        start(this.canvas);
    };

    return html`
    <main class=${[use(store.theme)]}>
      <div class="flex vcenter gap space-between top-bar">
        <span class="flex vcenter gap left">
          <${Logo} />
          ${$if(
        use(this.started),
        html` <p>FPS: ${use(this.fps, Math.floor)}</p> `,
    )}
        </span>
        <span class="flex gap-md right vcenter">

          <button on:click=${() => {
            document.getElementById("fs").showModal();
        }}>
              <span class="material-symbols-rounded">folder_open</span>
          </button>

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
            use(this.allowPlay, (allowed) => (!allowed ? "disabled" : "important")),
        ]}
              on:click=${startgame}
            >
              <span class="material-symbols-rounded">play_arrow</span>
            </button>
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
            bind:this=${use(this.canvas)}
            on:contextmenu=${(e) => e.preventDefault()}
          ></canvas>
        </canvascontainer>
      </div>

      <dialog id="fs">
      <button on:click=${() => {
            document.getElementById("fs").close();
        }} class="plain">
      <span class="material-symbols-rounded">close</span>
      </button>
        <${FsExplorer} />
      </dialog>

      <div class="logs">
      <h2>Log</h2>
      <${Logs} />
    </div>
        <footer>
            <kbd>
                port by <a href="https://mercurywork.shop">mercury workshop</a>
            </kbd>

            <kbd>
                all game assets and code belong to EXOK
            </kbd>

            <kbd>
                check out the project on <a href="https://github.com/MercuryWorkshop/celeste-wasm">github</a>
            </kbd>
        </footer>
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

export function Logo() {
    this.css = `
    .logo {
      image-rendering: pixelated;
      -ms-interpolation-mode: nearest-neighbor;
      width: 3.75rem;
      height: 3.75rem;
      margin: 0;
      padding: 0;
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
  `
    return html`
    <span class="flex vcenter">
      <img class="logo" src=${document.querySelector("link[rel=icon]").href} />
      <h1>celeste-wasm<subt>v${version}</subt></h1>
    </span>
  `
}
