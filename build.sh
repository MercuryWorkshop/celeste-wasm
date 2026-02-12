#!/bin/bash
set -e

# Setup environment
export DOTNET_ROOT=/home/the921691/.dotnet
export PATH=$DOTNET_ROOT/bin:$PATH
export PNPM=$(which pnpm || npx pnpm --version > /dev/null 2>&1 && echo "npx pnpm" || echo "pnpm")

cd "$(dirname "$0")"

echo "=== Environment Setup ==="
echo "DOTNET_ROOT: $DOTNET_ROOT"
echo "dotnet version: $(dotnet --version)"
echo "pnpm command: $PNPM"
echo ""

echo "=== Installing frontend dependencies ==="
cd frontend
$PNPM install --frozen-lockfile || $PNPM install
cd ..

echo ""
echo "=== Cleaning old build artifacts ==="
rm -rf frontend/public/_framework loader/bin/Release/net9.0/publish/wwwroot/_framework || true

echo ""
echo "=== Running dotnet restore ==="
NUGET_PACKAGES="$(realpath .)/nuget" dotnet restore loader/StardewLoader.csproj --nodereuse:false -v n

echo ""
echo "=== Replacing runtime ==="
bash replaceruntime.sh

echo ""
echo "=== Publishing StardewLoader WASM (THIS WILL TAKE 2-3 HOURS) ==="
NUGET_PACKAGES="$(realpath .)/nuget" dotnet publish loader/StardewLoader.csproj -c Release --nodereuse:false -v n

echo ""
echo "=== Copying framework to frontend ==="
cp -r loader/bin/Release/net9.0/publish/wwwroot/_framework frontend/public/

echo ""
echo "=== Applying emscripten compatibility patches ==="
if ls frontend/public/_framework/dotnet.native.*.js > /dev/null 2>&1; then
  sed -i 's/var offscreenCanvases \?= \?{};/var offscreenCanvases={};if(globalThis.window\&\&!window.TRANSFERRED_CANVAS){transferredCanvasNames=[".canvas"];window.TRANSFERRED_CANVAS=true;}/' frontend/public/_framework/dotnet.native.*.js
  sed -i 's/this.appendULeb(32768)/this.appendULeb(65535)/' frontend/public/_framework/dotnet.runtime.*.js || true
  sed -i 's/return runEmAsmFunction(code, sigPtr, argbuf);/return runMainThreadEmAsm(code, sigPtr, argbuf, 1);/' frontend/public/_framework/dotnet.native.*.js || true
  echo "Patches applied successfully"
else
  echo "Warning: Could not find patched files"
fi

echo ""
echo "=== Build complete! ==="
echo "You can now run: cd frontend && $PNPM dev"
