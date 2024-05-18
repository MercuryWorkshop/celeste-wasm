// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

// we load the asset manifest early so that we can use it to set the dotnet config
let assetManifest = await globalThis.fetch("asset_manifest.csv");
let assetManifestText = "";
if (!assetManifest.ok) {
    console.error("Unable to load asset manifest");
    console.error(assetManifest);
}
else {
    assetManifestText = await assetManifest.text();
}
let assetList = assetManifestText.split('\n')
    .filter(i => i)
    .map(i => i.trim().replace('\\', '/'));
console.log(`Found ${assetList.length} assets in manifest`);

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withModuleConfig({
        onConfigLoaded: (config) => {
            if (!config.resources.vfs) {
                config.resources.vfs = {}
            }

            for (let asset of assetList) {
                asset = asset.trim().replace(/^\/assets\//, '');;
                console.log(`Found ${asset}, adding to VFS`);
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
await new Promise((r)=>dotnet.instance.Module.FS.syncfs(true, r));
console.log("synced; exposing dotnet FS");
window.FS = dotnet.instance.Module.FS;

setModuleImports('main.js', {
    setMainLoop: (cb) => dotnet.instance.Module.setMainLoop(cb),
    syncFs: (cb) => dotnet.instance.Module.FS.syncfs(false, cb)
});

// set canvas
var canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;
await dotnet.run();
