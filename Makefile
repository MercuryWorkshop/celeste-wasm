STATICS_RELEASE=5ca6e290-3dbe-49dd-b7f8-647e3af0a709
DOTNETFLAGS=--nodereuse:false -v n
FRAMEWORK_STAGING=loader/bin/_framework.staging

statics:
	mkdir statics
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FAudio.a -O statics/FAudio.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FNA3D.a -O statics/FNA3D.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/libmojoshader.a -O statics/libmojoshader.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/SDL3.a -O statics/SDL3.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/liba.o -O statics/liba.o
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/hot_reload_detour.o -O statics/hot_reload_detour.o
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/libcrypto.a -O statics/libcrypto.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/dotnet.zip -O statics/dotnet.zip
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/emsdk.zip -O statics/emsdk.zip

SteamKit2.WASM:
	git clone https://github.com/MercuryWorkshop/SteamKit2.WASM --recursive

FNA:
	git clone https://github.com/FNA-XNA/FNA --recursive -b 25.11

NLua:
	git clone https://github.com/EverestAPI/NLua --recursive
	cd NLua && git apply ../nlua.patch

MonoMod:
	git clone https://github.com/r58Playz/MonoMod --recursive
	cd MonoMod && git reset --hard 8e904f7979c9423982c1f786ba57fdb22a5556d2

dotnetclean:
	rm -rvf {loader,patcher,corefier,Steamworks}/{bin,obj} frontend/public/_framework || true
clean: dotnetclean
	rm -rvf statics MonoMod NLua FNA SteamKit2.WASM || true

deps: statics FNA MonoMod NLua SteamKit2.WASM

build: deps
	pnpm i
	rm -r $(FRAMEWORK_STAGING) loader/bin/Release/net10.0/publish/wwwroot/_framework statics/{dotnet,emsdk} || true
	unzip -q -o statics/dotnet.zip -d statics/dotnet
	unzip -q -o statics/emsdk.zip -d statics/
	dotnet publish loader -c Release $(DOTNETFLAGS)
	cp -r loader/bin/Release/net10.0/publish/wwwroot/_framework $(FRAMEWORK_STAGING)
	# emscripten sucks
	sed -i 's/var offscreenCanvases \?= \?{};/var offscreenCanvases={};if(globalThis.window\&\&!window.TRANSFERRED_CANVAS){transferredCanvasNames=[".canvas"];window.TRANSFERRED_CANVAS=true;}/' $(FRAMEWORK_STAGING)/dotnet.native.*.js
	# dotnet messed up
	sed -i 's/this.appendULeb(32768)/this.appendULeb(65535)/' $(FRAMEWORK_STAGING)/dotnet.runtime.*.js
	# fmod messed up
	sed -i 's/return runEmAsmFunction(code, sigPtr, argbuf);/return runMainThreadEmAsm(code, sigPtr, argbuf, 1);/' $(FRAMEWORK_STAGING)/dotnet.native.*.js
	cd $(FRAMEWORK_STAGING) && split -b20M -d -a1 dotnet.native.*.wasm dotnet.native.*.wasm && rm dotnet.native.*.wasm
	# swap in only once complete, so vite dev reloads into a working framework
	rm -r frontend/public/_framework || true
	mv $(FRAMEWORK_STAGING) frontend/public/_framework

serve: build
	pnpm dev

publish: build
	pnpm build


.PHONY: clean build serve publish
