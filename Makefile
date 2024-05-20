Profile ?= Release

WWWROOT = bin/${Profile}/net8.0/wwwroot

VFSFILE=$(WWWROOT)/_framework/data.data

WEBASSETS = $(shell find wwwroot -type f | sed 's/ /\\ /g')

ASSETS = $(shell find Content -type f | sed 's/ /\\ /g')

WASMOUT = $(WWWROOT)/_framework/fna-wasm.wasm

STATICS = FAudio.a FNA3D.a libmojoshader.a SDL2.a


$(WASMOUT): $(STATICS) $(wildcard celeste/**/*.*) Program.cs fna-wasm.csproj
	@echo "Building WASM..."
	dotnet build -c $(Profile)
	cp -r wwwroot/* $(WWWROOT)


$(VFSFILE): $(ASSETS)
	@echo "Building VFS bundle..."
	sh helpers/buildvfs.sh "$(WWWROOT)"

$(STATICS):
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/latest/download/FAudio.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/latest/download/FNA3D.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/latest/download/libmojoshader.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/latest/download/SDL2.a

clean: 
	rm -rvf bin obj
	rm -f wasm.pak


wasm.pak: $(WASMOUT)
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

.PHONY: clean singlefile build serve copyweb all
