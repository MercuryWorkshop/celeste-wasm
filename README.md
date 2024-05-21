
<img src="wwwroot/assets/app.ico" width=100 align="left">

<h1>celeste-wasm</h1>

<br>

A mostly-complete port of Celeste (2018) to WebAssembly using Blazor and [FNA.WASM](https://github.com/FNA-XNA/FNA)

![image](https://github.com/MercuryWorkshop/celeste-wasm/assets/58010778/ba547bea-1763-48ad-b6b5-bbf8682d8d15)

# Limitations
- Loading the game consumes too much memory for mobile and lower end devices
- AOT compilation makes runtime reflection and detouring infeasible, so [Everest](https://github.com/EverestAPI/Everest) was not able to be included
- You may encounter issues on firefox.

# I want to play this
First, you need to own the game. There is a standalone html file [here](https://github.com/MercuryWorkshop/celeste-wasm/releases/download/latest/celeste.html) that can be played completely offline.

# I want to build this
if you can't reproduce this (it's really finnicky) feel free to ask us, the instructions can definitely be imrpoved

1. install arch packages `dotnet-host dotnet-runtime dotnet-sdk dotnet-targeting-pack aspnet-runtime fmodstudio xxd wget` & emscripten sdk
2. clone FNA latest in the parent dir (../)
3. apply `FNA.patch` to FNA
4. run `sudo dotnet workload restore` in this dir
5. download the celeste fmod project (with dlc) from [fmod download page](https://www.fmod.com/docs/2.03/studio/appendix-a-celeste.html)
6. open in fmodstudio and migrate the project
7. [optional] click the Assets tab and select every folder, click "add a custom platform encoding setting" and set the compression quality to your taste
8. press f7 in fmod studio to export the banks
8. Copy the `Content` folder from your celeste install and put it in the root of this project
9. copy in the exported fmod v2 banks to `Content/FMOD/Desktop/`
11. go to the releases tab, download celeste.patched.zip.xor, and then run `python3 helpers/xor.py celeste.patched.zip.xor /path/to/your/celeste/install/Celeste.Content.dll > celeste.patched.zip` and then extract it into `celeste/Celeste`
12. run `make statics`
13. run `make serve`

## i want to build this as an html file
1. cd into `jslibs/` and run `p/npm install`
2. run `npx esbuild --minify --bundle ./pack.js --outfile=../wwwroot/zstd.js`
3. run `make singlefile`


**main improvements that need to be done:**
1. ~~persistent fs~~
2. ~~fix release build, currently crashes at [generic contentreader type limitation](<https://gist.github.com/TheSpydog/e94c8c23c01615a5a3b2cc1a0857415c#qa>)~~
3. ~~enable optimizations~~
4. threading so it no longer freezes
5. don't load all assets into memory, fetch on demand
6. ~~fix b-side cassete blocks~~

## (bonus) simple fna game porting guide
1. decompile it
2. follow instructions over at [FNA.WASM.Sample](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM#set-up-wasm-project) to get a basic project up, replacing `MyGame` with the default xna `Game`
3. follow instructions from `FNA.WASM.Sample` for complex asset system (or take a look in `helpers/buildvfs.sh` and `wwwroot/main.js`)
4. once assets are loading copy in the decompiled game and replace the default `Game` object, copy in anything the Main function does from the original game
