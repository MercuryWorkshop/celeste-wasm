<img src="public/app.ico" width=100 align="left">

<h1>celeste-wasm (threads port)</h1>

<br>

A mostly-complete port of Celeste (2018) to WebAssembly using dotnet 9's threaded WASM support and [FNA WASM libraries](https://github.com/r58playz/FNA-WASM-Build).

This "fork" will be merged into [the original](https://github.com/mercuryWorkshop/celeste-wasm) soon.

## Limitations
- Loading the game consumes 600M or so of memory, which is still around 3x lower than the original port, but it is still too much for low end devices.
- You may encounter issues on firefox.

## I want to build this
1. install arch packages `dotnet-host-bin dotnet-runtime-bin dotnet-sdk-bin dotnet-targeting-pack-bin aspnet-runtime-bin diffutils patch wget`
    - note that the `-bin` prefix is only because arch repos haven't updated to dotnet 9, use the unprefixed packages if they are dotnet 9 
    - also install the dotnet 8 runtime, right now that's `dotnet-runtime`
2. run `dotnet tool install --global ilspycmd --version 9.0.0.7889`
3. clone [FNA](https://github.com/FNA-XNA/FNA) version 24.01 in the parent dir (`../`)
4. clone [this MonoMod fork](https://github.com/r58playz/MonoMod) in the parent dir
4. apply (`git apply ...`) `FNA.patch` to FNA
5. run `sudo dotnet workload restore` in this dir
6. run `bash tools/decompile.sh path/to/Celeste.exe`
7. run `bash tools/applypatches.sh Vanilla`
8. run `make serve` for a dev server and `make publish` for a release build

to enable the download/decrypt feature:
1. create a tar archive (optionally gzip compressed) of the Content directory
2. run `python3 tools/xor.py path/to/archive.tar.gz path/to/key > archive.xor.tar.gz`
3. optionally split the archive into chunks - these chunks must have a suffix of `.CHUNKNUM` where `CHUNKNUM` is a number starting from 0
    - if you do not split the archive, give it a suffix of `.0`
4. pass these env vars to `make serve` or `make publish`:
    - `VITE_DECRYPT_ENABLED=1`
    - `VITE_DECRYPT_KEY`: path to the key from within the Content folder
    - `VITE_DECRYPT_PATH`: path to the archive relative to the http root
    - `VITE_DECRYPT_SIZE`: total size of the archive (after compression if you did that)
    - `VITE_DECRYPT_COUNT`: total number of chunks - set this to 1 if you did not split the archive
    - these vars can also be placed in a `.env.local` - see [Vite docs](https://vite.dev/guide/env-and-mode)

**main improvements that need to be done:**
- fix freezes due to fmod
- remove the janky `WRAP_FNA` stuff and replace it with a SDL that doesn't use EGL emulation
- fix freeze when removing controller while in a level

## I want to port this to a newer version of celeste (once it exists)
1. fix any issues with the hooks
2. make a pr!

## I want to figure out how this works
- The native dotnet WASM support is used to compile a decompiled Celeste to WASM
    - `celeste/Program.cs` sets up the Celeste object and exports a function that polls its main loop
    - `celeste/Hooks` uses MonoMod to fix some issues and add features
    - `celeste/Steamworks.cs` polyfills the Steam achievements API for Steam builds of Celeste
    - dotnet even supports JITing WASM but it uses more resources and is overall slower so right now AOT is used
- FMOD's WASM builds don't support threads so a wrapper that proxies it to the main thread is built and used instead
    - See `tools/fmod-patch` for more information
- SDL2 doesn't use the native Emscripten WebGL apis and instead uses Emscripten's EGL emulation, so calling GL apis always fails
    - To fix this, another wrapper is used that proxies all FNA3D calls to the main thread
