
<img src="public/assets/app.ico" width=100 align="left">

<h1>celeste-wasm</h1>

<br>

A mostly-complete port of Celeste (2018) to WebAssembly using dotnet's `wasmbrowser` template, [FNA WASM libraries](https://github.com/RedMike/FNA-WASM-Build) and [FNA.WASM.Sample guide](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM).

![image](https://github.com/MercuryWorkshop/celeste-wasm/assets/58010778/ba547bea-1763-48ad-b6b5-bbf8682d8d15)

# Limitations
- Loading the game consumes too much memory for mobile and lower end devices (standalone html uses even more and is slower to start)
- AOT compilation makes runtime reflection and detouring infeasible, so [Everest](https://github.com/EverestAPI/Everest) was not able to be included
- You may encounter issues on firefox.

# I want to play this
First, you need to own the game. There is a standalone html file [here](https://github.com/MercuryWorkshop/celeste-wasm/releases/download/latest/celeste.html) that can be played completely offline. There is also a public build [here](https://celeste.r58playz.dev).

# I want to build this
if you can't reproduce this (it's really finnicky) feel free to ask us, the instructions can definitely be imrpoved

1. install arch packages `dotnet-host dotnet-runtime dotnet-sdk dotnet-targeting-pack aspnet-runtime fmodstudio xxd wget` & emscripten sdk
2. clone FNA latest in the parent dir (`../`)
3. apply `FNA.patch` to FNA
4. run `sudo dotnet workload restore` in this dir
5. download the celeste fmod project (with dlc) from [fmod download page](https://www.fmod.com/download). [fmod official docs for this](https://www.fmod.com/docs/2.03/studio/appendix-a-celeste.html)
6. open in fmodstudio and migrate the project
7. [optional] click the Assets tab and select every folder, click "add a custom platform encoding setting" and set the compression quality to your taste
8. press f7 in fmod studio to export the banks
8. Copy the `Content` folder from your celeste install and put it in the root of this project
9. copy in the exported fmod v2 banks to `Content/FMOD/Desktop/`
11. go to the releases tab, download celeste.patched.zip.xor, and then run `python3 helpers/xor.py celeste.patched.zip.xor /path/to/your/celeste/install/Content/Dialog/english.txt > celeste.patched.zip` and then extract it into `celeste/Celeste`
12. run `make statics`
13. run `make serve`

## i want to build this as an html file
1. run `make singlefile`

## i want to port this to a newer version of celeste (once it exists)
we couldn't get the output of ilspy to be stable enough for a patch system to work. look in Patches/Code/Celeste for all the changes we made and you will have to rebase them yourself with your own decompiler output. we used ilspy 7.2-rc.


**main improvements that need to be done:**
1. threading so it no longer freezes (waiting for .net 9)
2. make a true patching system so that we can easily update to newer versions of celeste
3. fix gamepads (currently crashes at emscripten type signature error)
4. port over everest (if possible)

## (bonus) simple fna game porting guide
1. decompile it
2. follow instructions over at [`FNA.WASM.Sample`](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM#set-up-wasm-project) to get a basic project up, replacing `MyGame` with the default xna `Game`
3. follow instructions from `FNA.WASM.Sample` for complex asset system (or take a look in `helpers/buildvfs.sh` and `wwwroot/main.js`)
4. once assets are loading copy in the decompiled game and replace the default `Game` object, copy in anything the Main function does from the original game
