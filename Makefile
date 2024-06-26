Profile ?= Release

DRM ?= 0
SPLIT ?= 0

WWWROOT = bin/${Profile}/net8.0/wwwroot

VFSFILE=$(WWWROOT)/_framework/data.data
VFSTARGET=$(WWWROOT)/_framework/data.dummy

WEBASSETS = $(shell find public -type f | sed 's/ /\\ /g')

JSSRC = $(shell find src -type f | sed 's/ /\\ /g')

ASSETS = $(shell find Content -type f | sed 's/ /\\ /g')

WASMOUT = $(WWWROOT)/_framework/fna-wasm.wasm

STATICS = FAudio.a FNA3D.a libmojoshader.a SDL2.a


$(WASMOUT): $(wildcard celeste/**/*.*) Program.cs fna-wasm.csproj
	@echo "Building WASM..."
	dotnet build -c $(Profile)
	cp -r public/* $(WWWROOT)
	rm -rvf $(WWWROOT)/**/*.gz


$(VFSTARGET) $(VFSFILE): $(ASSETS)
	@echo "Building VFS bundle..."
	sh helpers/buildvfs.sh "$(WWWROOT)"
	echo -n "const SIZE = " > "$(WWWROOT)/cfg.js"
	stat -c %s "$(VFSFILE)" >> "$(WWWROOT)/cfg.js"
	echo "const DRM = $(DRM);" >> "$(WWWROOT)/cfg.js"
ifeq ($(DRM),1)
	@echo "Encrypting VFS bundle..."
	python3 helpers/xor.py "$(VFSFILE)" $(DRMKEY) > "$(VFSFILE).enc"
	mv "$(VFSFILE).enc" "$(VFSFILE)"
endif
	echo "const SPLIT = $(SPLIT);" >> "$(WWWROOT)/cfg.js"
ifneq ($(SPLIT),0)
	@echo "Splitting VFS bundle..."
	mkdir -p $(WWWROOT)/_framework/data
	split -b$(SPLIT)M --additional-suffix .data $(VFSFILE) $(WWWROOT)/_framework/data/data
	rm $(WWWROOT)/_framework/data.data
endif
	echo "const CHUNKSIZE = $$(( $(SPLIT) * 1024 * 1024 ));" >> $(WWWROOT)/cfg.js
	echo -n "const splits = [" >> "$(WWWROOT)/cfg.js"
	ls -1 $(WWWROOT)/_framework/data/ | sed 's/^/"/' | sed 's/$$/",/' | tr -d '\n' >> "$(WWWROOT)/cfg.js"
	echo "];" >> "$(WWWROOT)/cfg.js"
	:> "$(VFSTARGET)"

$(STATICS):
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/FAudio.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/FNA3D.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/libmojoshader.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/SDL2.a

src/node_modules: src/package.json
	cd src && pnpm install

$(WWWROOT)/bundle.js: src/node_modules $(JSSRC)
	SINGLEFILE= sh helpers/bundle.sh "$(WWWROOT)"

$(WWWROOT)/bundlesingle.js: src/node_modules $(JSSRC)
	SINGLEFILE=1 sh helpers/bundle.sh "$(WWWROOT)" 

$(WWWROOT)/singlefileloader.js: src/node_modules src/singlefileloader.js
	cd src && npx esbuild --minify --bundle ../src/singlefileloader.js --outfile="../$(WWWROOT)/singlefileloader.js"

statics: $(STATICS)

clean: 
	rm -rvf bin obj
	rm -f wasm.pak $(STATICS)

wasm.pak: $(WASMOUT) helpers/pack_wasm.sh
	@echo "Building pak file..."
	sh helpers/pack_wasm.sh "$(WWWROOT)"

singlefile: wasm.pak $(VFSFILE) $(WWWROOT)/bundlesingle.js $(WWWROOT)/singlefileloader.js
	@echo "Building single file..."
	cp -r public/* $(WWWROOT)
	ksh helpers/pack_html.sh "$(WWWROOT)"

build: $(WASMOUT)

serve: $(WASMOUT) $(VFSTARGET) $(WWWROOT)/bundle.js
	@echo "Serving..."
	cp -r public/* $(WWWROOT)
	sh helpers/server.sh "$(WWWROOT)"

compress: $(WASMOUT) $(VFSTARGET) $(WWWROOT)/bundle.js
	@echo "Compressing..."
	cp -r public/* $(WWWROOT)
	tar cavf celeste-wasm.tar.zst -C $(WWWROOT) .

all: $(WASMOUT) $(VFSTARGET) $(WWWROOT)/bundle.js singlefile
	cp -r public/* $(WWWROOT)

.PHONY: clean singlefile build serve copyweb compress all
