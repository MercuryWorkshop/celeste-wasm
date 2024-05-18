# read me

1. install arch packages `dotnet-host dotnet-runtime dotnet-sdk dotnet-targeting-pack aspnet-runtime fmodstudio`
2. clone FNA latest in the parent dir
3. run `sudo dotnet workload restore` in this dir
4. download the celeste fmod project (with dlc) from fmod download page
5. open in fmodstudio, migrate project, export banks
6. place your celeste Content dir in `wwwroot/assets/`
7. copy in the exported fmod v2 banks to `wwwroot/assets/Content/FMOD/Desktop/`
8. run `dotnet run -v d`

## fna game porting guide
1. decompile it
2. follow instructions over at [FNA.WASM.Sample](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM#set-up-wasm-project) to get a basic project up, replacing `MyGame` with the default xna `Game`
3. follow instructions from `FNA.WASM.Sample` for complex asset system
4. once assets are loading copy in the decompiled game and replace the default `Game` object, copy in anything the Main function does from the original game
