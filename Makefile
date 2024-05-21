Profile ?= Release

DRM ?= 0
SPLIT ?= 0

WWWROOT = bin/${Profile}/net8.0/wwwroot

VFSFILE=$(WWWROOT)/_framework/data.data

WEBASSETS = $(shell find wwwroot -type f | sed 's/ /\\ /g')

ASSETS = $(shell find Content -type f | sed 's/ /\\ /g')

WASMOUT = $(WWWROOT)/_framework/fna-wasm.wasm

STATICS = FAudio.a FNA3D.a libmojoshader.a SDL2.a


$(WASMOUT): $(wildcard celeste/**/*.*) Program.cs fna-wasm.csproj
	@echo "Building WASM..."
	dotnet build -c $(Profile)
	cp -r wwwroot/* $(WWWROOT)
	rm -rvf $(WWWROOT)/**/*.gz


$(VFSFILE): $(ASSETS)
	@echo "Building VFS bundle..."
	sh helpers/buildvfs.sh "$(WWWROOT)"
	echo -n "const SIZE = " > "$(WWWROOT)/cfg.js"
	stat -c %s "$(VFSFILE)" >> "$(WWWROOT)/cfg.js"
	echo "const DRM = $(DRM);" >> "$(WWWROOT)/cfg.js"
ifeq ($(DRM),1)
	@echo "Encrypting VFS bundle..."
	python3 helpers/xor.py "$(VFSFILE)" $(DRMKEY) > "$(VFSFILE).enc"
	mv "$(VFSFILE)" "$(VFSFILE).old"
	mv "$(VFSFILE).enc" "$(VFSFILE)"
endif
	echo "const SPLIT = $(SPLIT);" >> "$(WWWROOT)/cfg.js"
ifeq ($(SPLIT),1)
	@echo "Splitting VFS bundle..."
	mkdir -p $(WWWROOT)/_framework/data
	split -b20M $(VFSFILE) $(WWWROOT)/_framework/data/
endif
	echo -n "const splits = [" >> "$(WWWROOT)/cfg.js"
	ls -1 $(WWWROOT)/_framework/data/ | sed 's/^/"/' | sed 's/$$/",/' | tr -d '\n' >> "$(WWWROOT)/cfg.js"
	echo "];" >> "$(WWWROOT)/cfg.js"

$(STATICS):
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/FAudio.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/FNA3D.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/libmojoshader.a
	wget https://github.com/RedMike/FNA-WASM-Build/releases/latest/download/SDL2.a

statics: $(STATICS)

clean: 
	rm -rvf bin obj
	rm -f wasm.pak $(STATICS)

wasm.pak: $(WASMOUT) helpers/pack_wasm.sh
	@echo "Building pak file..."
	sh helpers/pack_wasm.sh "$(WWWROOT)"

singlefile: wasm.pak $(VFSFILE)
	@echo "Building single file..."
	cp -r wwwroot/* $(WWWROOT)
	ksh helpers/pack_html.sh "$(WWWROOT)"

build: $(WASMOUT)

serve: $(WASMOUT) $(VFSFILE)
	@echo "Serving..."
	cp -r wwwroot/* $(WWWROOT)
	sh helpers/server.sh "$(WWWROOT)"

compress: $(WASMOUT) $(VFSFILE)
	@echo "Compressing..."
	cp -r wwwroot/* $(WWWROOT)
	tar cavf celeste-wasm.tar.zst -C $(WWWROOT) .

all: $(WASMOUT) $(VFSFILE)
	cp -r wwwroot/* $(WWWROOT)

.PHONY: clean singlefile build serve copyweb compress all
