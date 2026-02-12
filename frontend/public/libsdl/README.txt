Place your Stardew game assemblies and native libraries here so the WASM loader can find them.

Required files (example):
- CustomStardewValley.dll  (or StardewValley.dll / StardewValley.exe)
- Any dependent .dll files (e.g., FNA, game mods)
- Native libraries if needed (SDL3 native shims), but note native libs must be WASM-compatible.

How to add files:
- Copy the files into frontend/public/libsdl/ so they are served at /libsdl/ in the browser.

Example:
  cp /path/to/CustomStardewValley.dll frontend/public/libsdl/
  cp -r /path/to/Content frontend/public/libsdl/Content/

After adding files, refresh the page at http://localhost:5173 and check the browser console for new runtime messages.
