# read me

1. install arch packages `dotnet-host dotnet-runtime dotnet-sdk dotnet-targeting-pack aspnet-runtime fmodstudio` & emscripten sdk
2. clone FNA latest in the parent dir
3. apply `FNA.patch` to FNA
4. run `sudo dotnet workload restore` in this dir
5. download the celeste fmod project (with dlc) from fmod download page
6. open in fmodstudio and migrate the project
7. [optional] click the Assets tab and select every folder, click "add a custom platform encoding setting" and set the compression quality to your taste
8. press f7 in fmod studio to export the banks
8. place your celeste Content dir in a temp dir
9. copy in the exported fmod v2 banks to `Content/FMOD/Desktop/` in the temp dir
10. run `path/to/emsdk/upstream/emscripten/tools/file_packager.py data.data --preload path/to/Content/@/Content --js-output=data.js --lz4 --no-node --use-preload-cache` and copy the data.js into `wwwroot/`
11. apply data.patch to data.js
12. run `dotnet publish` (or `dotnet run -v d` if you hate fast loading times)
13. copy data.data into `bin/Release/net8.0/publish/wwwroot` and run a http server in that dir if you ran `dotnet publish`

**main improvements that need to be done:**
1. ~~persistent fs~~
2. ~~fix release build, currently crashes at [generic contentreader type limitation](<https://gist.github.com/TheSpydog/e94c8c23c01615a5a3b2cc1a0857415c#qa>)~~
3. ~~enable optimizations~~
4. threading so it no longer freezes
5. don't load all assets into memory, fetch on demand
6. ~~fix b-side cassete blocks~~

## fna game porting guide
1. decompile it
2. follow instructions over at [FNA.WASM.Sample](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM#set-up-wasm-project) to get a basic project up, replacing `MyGame` with the default xna `Game`
3. follow instructions from `FNA.WASM.Sample` for complex asset system
4. once assets are loading copy in the decompiled game and replace the default `Game` object, copy in anything the Main function does from the original game
