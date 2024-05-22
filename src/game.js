export const version = "1.4.0.0";
let setModuleImports, getAssemblyExports, getConfig;
export async function init() {
  ({ setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withModuleConfig({
      onConfigLoaded: (config) => {
        config.disableIntegrityCheck = true;
      },
    })
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create());

  dotnet.instance.Module.FS.mkdir("/libsdl", 0o755);
  dotnet.instance.Module.FS.mount(
    dotnet.instance.Module.FS.filesystems.IDBFS,
    {},
    "/libsdl",
  );
  await new Promise((r) => dotnet.instance.Module.FS.syncfs(true, r));
  console.log("synced; exposing dotnet FS");
  window.FS = dotnet.instance.Module.FS;
}

let ts = performance.now();
export let fps;
const MainLoop = (cb) => {
  dotnet.instance.Module.setMainLoop(() => {
    let now = performance.now();
    let dt = now - ts;
    ts = now;
    fps = 1000 / dt;

    cb();
  });
};

export async function start(canvas) {
  console.info("Starting...");

  await new Promise(r => loadData(dotnet.instance.Module, r));
  console.info("Loaded assets into VFS");
  localStorage["vfs_populated"] = true
  if (window.assetblob) {
    URL.revokeObjectURL(window.assetblob);
  }

  setModuleImports("main.js", {
    setMainLoop: MainLoop,
    syncFs: (cb) => dotnet.instance.Module.FS.syncfs(false, cb),
  });

  dotnet.instance.Module.canvas = canvas;

  await dotnet.run();

  let Exports = await getAssemblyExports("fna-wasm");

  Exports.Program.StartGame();
}
