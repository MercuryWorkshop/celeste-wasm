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


    canvas {
        width: 100%;
        display: block;
        border: 2px solid white;
        border-radius: 1em;
        background-color: grey;
    }

    button {
        background-color: #f02424;
        padding: 0.5em 1em;
        color: white;
        border: none;
    }

    pre {
        overflow-y: scroll;
        font-size: 0.8em;
        max-height: 20em;
        border: 2px solid white;
        padding: 1em;
        background-color: #1c1c1c;
    }
`;

    this.started = false;





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
    }

    return html`
        <div>


            <div class="flex vcenter gap">
                <h1>celeste-wasm</h1>

                <p>Version: ${version}</p>
                <p>FPS: ${use(this.fps, Math.floor)}</p>

                <div>
                    <button on:click=${start}>Start Game</button>
                </div>
            </div>


            <canvas id="canvas" bind:this=${use(this.canvas)}></canvas>



            <h2>Log</h2>
            <pre bind:this=${use(this.log)}>

            </pre>
        </div>
    `
}


const app = h(App).$;


let olog = console.log;

let logs = [];
let ringsize = 2000;
export function log(...args) {
    olog(...args);
    logs.push(args.join(" ") + "\n");
    if (logs.length > ringsize) {
        logs.shift();
    }
}
setInterval(() => {
    app.log.innerText = logs.join("\n");
}, 1000);

// console.log = log

document.body.appendChild(app.root);


