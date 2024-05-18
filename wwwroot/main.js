// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

const version = "4.0.0.0";

function App() {
    this.css = `
    width: 100vw;
    height: 100vh;
    padding: 1em;
    margin: 0;
    position: relative;
    background-color: #171717;
    color: white;

    overflow-x: hidden;



    canvascontainer {
        display: block;
        width: 100%;
        border: 2px solid white;
        border-radius: 0.5em;
        overflow: hidden;
    }
    canvas {
        width: 100%;
        display: block;
        background-color: grey;
    }
    .pinned {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: auto;
    }

    button {
        background-color: #1c1c1c;
        padding: 0.5em 1em;
        color: white;
        border: none;
        border-radius: 0.5em;
    }
    button.important {
        background-color: #f02424;
    }

    pre {
        overflow-y: scroll;
        font-size: 0.8em;
        max-height: 20em;
        border: 0.7px solid #666;
        border-radius: 0.7em;
        padding: 1em;
        background-color: #1c1c1c;
    }
`;

    this.started = false;


    this.debug = false;
    this.fullscreen = false;


    document.addEventListener('fullscreenchange', () => {
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
        })
    }

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
        }
        else {
            assetManifestText = await assetManifest.text();
        }
        let assetList = assetManifestText.split('\n')
            .filter(i => i)
            .map(i => i.trim().replace('\\', '/'));
        log(`Found ${assetList.length} assets in manifest`);

        const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
            .withModuleConfig({
                onConfigLoaded: (config) => {
                    if (!config.resources.vfs) {
                        config.resources.vfs = {}
                    }

                    for (let asset of assetList) {
                        asset = asset.trim().replace(/^\/assets\//, '');;
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
        dotnet.instance.Module.FS.mount(dotnet.instance.Module.FS.filesystems.IDBFS, {}, "/libsdl");
        await new Promise((r) => dotnet.instance.Module.FS.syncfs(true, r));
        log("synced; exposing dotnet FS");
        window.FS = dotnet.instance.Module.FS;

        setModuleImports('main.js', {
            setMainLoop: MainLoop,
            syncFs: (cb) => dotnet.instance.Module.FS.syncfs(false, cb)
        });


        dotnet.instance.Module.canvas = this.canvas;


        await dotnet.run();


        let Exports = await getAssemblyExports("fna-wasm");




        Exports.Program.SetConfig(this.debug);

        Exports.Program.StartGame();

    }

    return html`
        <div>


            <div class="flex vcenter gap">
                <h1>celeste-wasm</h1>

                <p>Version: ${version}</p>
                <p>FPS: ${use(this.fps, Math.floor)}</p>


                <div>
                    <label for="debug">Debug: </label>
                    <input type="checkbox" bind:checked=${use(this.debug)} />
                </div>



                <button on:click=${() => {
            if (this.canvas.requestFullscreen()) {
                this.fullscreen = true
            }

        }}>Fullscreen</button>
                <button class="important" on:click=${start}>Start Game</button>
            </div>

            <input type="file">


            ${navigator.userAgent.includes("Firefox") && html`<${FuckMozilla} />` || ""}



            <canvascontainer>
                <canvas id="canvas" class=${[use(this.fullscreen, f => f && "pinned")]} bind:this=${use(this.canvas)}></canvas>
            </canvascontainer>



            <h2>Log</h2>
            <pre bind:this=${use(this.log)}>

            </pre>
        </div>
    `
}

function FuckMozilla() {
    this.css = `
        width: 100%;

        background-color: red;
        color: yellow;
        padding: 1em;
        padding-top: 0.5em;
        margin-bottom: 1em;

    `;


    return html`
        <div>
            <h1>THIS DOESN'T WORK WELL ON FIREFOX!!!</h1>
            <p>it might still work. but you should really just use chromium</p>

            <button on:click=${() => this.root.remove()}>fuck you i love my shitty browser</button>
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
