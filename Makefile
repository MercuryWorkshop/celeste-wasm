
STATICS_RELEASE=9c3dd72e-5785-49f6-b648-cdcbb036fb7d

statics:
	mkdir statics
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FAudio.a -O statics/FAudio.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FNA3D_Wrapped.a -O statics/FNA3D.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/libmojoshader.a -O statics/libmojoshader.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/SDL2.a -O statics/SDL2.a

clean:
	rm -rv statics obj bin || true

build: statics
#	dotnet publish -v d -c Debug
	dotnet publish -v d -c Release
	# microsoft messed up
	sed -i 's/FS_createPath("\/","usr\/share",!0,!0)/FS_createPath("\/usr","share",!0,!0)/' bin/Release/net9.0/publish/wwwroot/_framework/dotnet.runtime.*.js

serve: build
#	python3 tools/serve.py bin/Debug/net9.0/publish/wwwroot/_framework
	python3 tools/serve.py bin/Release/net9.0/publish/wwwroot/_framework
