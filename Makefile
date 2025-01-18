STATICS_RELEASE=7a74fd6f-2c6f-4283-ba61-873c64649b24
STATICS=statics/FAudio.a statics/FNA3D_Wrapped.a statics/libmojoshader.a statics/SDL2.a statics/liba.o

staticsdir:
	mkdir statics

$(STATICS):
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/$(@F) -O $@

statics: $(STATICS)
	cp FNA3D_Wrapped.a FNA3D.a

clean:
	rm -rv statics obj bin public/_framework || true

build: statics
	pnpm i
	rm -rv bin/Release/net9.0/publish/wwwroot/_framework public/_framework || true
	dotnet publish -v d -c Release
	# microsoft messed up
	sed -i 's/FS_createPath("\/","usr\/share",!0,!0)/FS_createPath("\/usr","share",!0,!0)/' bin/Release/net9.0/publish/wwwroot/_framework/dotnet.runtime.*.js
	cp -rv bin/Release/net9.0/publish/wwwroot/_framework public/

serve: build
	pnpm dev

publish: build
	pnpm build
