<img src="public/app.ico" width=100 align="left">

<h1>celeste-wasm (threads port)</h1>

<br>

A mostly-complete port of Celeste (2018) to WebAssembly using dotnet's WASM support and [FNA WASM libraries](https://github.com/r58playz/FNA-WASM-Build).

This "fork" will be merged into [the original](https://github.com/mercuryWorkshop/celeste-wasm) soon.

## Limitations
- Loading the game consumes 800M or so of memory, which is still an improvement over the original, but it is still too much for low end devices.
- MonoMod has no support for WASM, so [Everest](https://github.com/EverestAPI/Everest) was not able to be included
- You may encounter issues on firefox.

## I want to build this
if you can't reproduce this (it's really finnicky) feel free to ask us, the instructions can definitely be improved

1. install arch packages `dotnet-host-bin dotnet-runtime-bin dotnet-sdk-bin dotnet-targeting-pack-bin aspnet-runtime-bin diffutils patch wget` & emscripten sdk
    - note that the `-bin` prefix is only because arch repos haven't updated to dotnet 9, use the unprefixed packages if they are dotnet 9 
2. run `dotnet tool install --global ilspycmd --version 8.2.0.7535`
3. clone [FNA](https://github.com/FNA-XNA/FNA) in the parent dir (`../`)
4. apply (`git apply ...`) `FNA.patch` to FNA
5. run `sudo dotnet workload restore` in this dir
6. run `bash tools/decompile.sh path/to/Celeste.exe`
7. run `bash tools/applypatches.sh`
8. run `make serve`
9. manually copy over your Content folder to OPFS with `window.copyContent`

## I want to modify the decompiled Celeste
1. follow "I want to build this"
2. make your changes to the Celeste code in `celeste/{Celeste,Celeste.Editor,Celeste.Pico8,Monocle}`
3. run `bash tools/genpatches.sh`
4. commit the generated patches

## I want to port this to a newer version of celeste (once it exists)
1. run `bash tools/decompile.sh path/to/Celeste.exe`
2. run `bash tools/applypatches.sh`
3. fix any broken patches/make it run
3. run `bash tools/genpatches.sh`
4. make a pr!

**main improvements that need to be done:**
- move `Init` to another thread to remove the last of the freezing
- remove the janky `WRAP_FNA` stuff and replace it with a SDL that doesn't use EGL emulation
- check (and fix) controller support
- port over everest (if possible)
- add back a OPFS explorer
