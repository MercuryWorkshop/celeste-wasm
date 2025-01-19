STATICS_RELEASE=e0c7b9bf-b3e4-4fe0-8531-24d41586d18d

statics:
	mkdir statics
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FAudio.a -O statics/FAudio.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FNA3D_Wrapped.a -O statics/FNA3D.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/libmojoshader.a -O statics/libmojoshader.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/SDL2.a -O statics/SDL2.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/liba.o -O statics/liba.o
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/dotnet.zip -O statics/dotnet.zip

clean:
	rm -rv statics obj bin public/_framework nuget || true

build: statics
	pnpm i
	rm -rv bin/Release/net9.0/publish/wwwroot/_framework public/_framework || true
#
	NUGET_PACKAGES="$(realpath .)/nuget" dotnet restore
	unzip -o statics/dotnet.zip -d nuget/microsoft.netcore.app.runtime.mono.multithread.browser-wasm/9.0.1/
	NUGET_PACKAGES="$(realpath .)/nuget" dotnet publish -c Release -v diag
#
	# microsoft messed up
	sed -i 's/FS_createPath("\/","usr\/share",!0,!0)/FS_createPath("\/usr","share",!0,!0)/' bin/Release/net9.0/publish/wwwroot/_framework/dotnet.runtime.*.js
	cp -rv bin/Release/net9.0/publish/wwwroot/_framework public/

serve: build
	pnpm dev

publish: build
	pnpm build
