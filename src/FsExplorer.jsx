export function FSExplorer() {
	this.path = "/";
	this.listing = [];

	this.displayingFile = false;
	this.filePath = "";
	this.fileData = "";

	this.fileHiddenButNotRemoved = false;

	this.mount = async () => {
		await window.initPromise;
		this.fs = window.FS;
		this.listing = this.fs.readdir(this.path);
	}

	this.css = `
		width: 100%;
		height: calc(100% - 6rem);

		.item {
			margin-block: 0.15rem;
			gap: 0.5rem;
			cursor: pointer;
		}

		.material-symbols-rounded {
			font-size: 1.3rem;
		}

		pre {
			display: inline;
		}

		#uploadcontainer {
      position: sticky;
      bottom: 0;
      right: 0;
      left: 0;
      display: flex;
      justify-content: flex-end;
      // padding: 1rem;
      pointer-events: none;
      width: 100%;

      button {
        pointer-events: all;
      }
		}

		.actions {
		    pointer-events: none;
				opacity: 0;
				transition: opacity 0.15s ease;
		}

		.item:hover .actions {
		    pointer-events: all;
				opacity: 1;
				transition: opacity 0.15s ease;
		}

		.item {
		  width: 100%;
			border-radius: 0.6rem;
			padding-inline: 0.5rem;
			background: var(--bg);
			transition: background 0.15s ease;
		}

		.item:hover {
		  background: var(--surface0);
			transition: background 0.15s ease;
		}

		#path {
			font-size: 1.5rem;
			font-weight: 650;
			font-family: var(--font-display);
			margin-block: 0.5rem;
		}

		#listing {
		  width: 100%;
			height: 100%;
			overflow-y: auto;
		}

		.fileview {
		  animation: fadeinandmove 0.15s ease 0s 1;

			&.hidden {
			 animation: fadeoutandmove 0.15s ease 0s 1;
			}

			textarea {
			  width: 100%;
				min-height: 10rem;
				background: var(--surface0);
				border: none;
				border-radius: 0.5rem;
				resize: vertical;
				padding: 0.5rem;
				font-family: var(--font-mono);
				color: var(--fg);
				font-size: 1rem;
			}
		}
    `

	return (
		<div>
			<div id="path">{use(this.path, path => path == "/" ? "File Browser" : path)}</div>
			<div id="listing">
			{use(this.listing, r => r.map((r) => {
				let mode = this.fs.stat(this.path + r).mode;
				if (this.fs.isDir(mode)) {
					return (
						<div class="item flex vcenter space-between" role="button" on:click={() => {
							if (r == ".") {
								this.mount();
							} else if (r == "..") {
								if (this.path == "/") return;
								this.path = this.path.split("/").toSpliced(-2, 1).join("/");
								this.mount();
							} else {
								this.path += `${r}/`;
								this.mount();
							}
						}}>
						  <div class="title flex vcenter gap-sm">
  							<span class="material-symbols-rounded">folder</span>
  							<pre>{r}/</pre>
							</div>
						</div>
					)
				} else {
					return (
						<div class="item flex vcenter space-between" role="button" on:click={() => {
							this.displayingFile = true;
							this.filePath = this.path + r;
							try {
								this.fileData = (new TextDecoder()).decode(this.fs.readFile(this.path + r));
							} catch { }
						}}>
						  <div class="title flex vcenter gap-sm">
  							<span class="material-symbols-rounded">description</span>
  							<pre>{r}</pre>
           	  </div>
							<div class="actions flex vcenter gap-xs">
  							<button title="Download" class="plain" on:click={(e) => {
  								let data = this.fs.readFile(this.path + r);
  								let el = (<a href={URL.createObjectURL(new Blob([data]))} download={r}></a>)
  								el.click();
  								e.stopPropagation();
  							}}>
  							  <span class="material-symbols-rounded">download</span>
  							</button>
  							<button title="Delete" class="plain" on:click={(e) => {
  								this.fs.unlink(this.path + r);
  								this.mount();
  								this.fs.syncfs(false, () => { });
  								e.stopPropagation();
  							}}>
  							  <span class="material-symbols-rounded" style="color: var(--accent)">delete</span>
  							</button>
							</div>
						</div>
					)
				}
			}))}
			</div>
			{$if(use(this.displayingFile), (
				<div class={["fileview", use(this.fileHiddenButNotRemoved, h => h && "hidden")]}>
					<div class="flex space-between vcenter" style="margin-block: 1rem;">
					<span>File: <pre>{use(this.filePath)}</pre></span>
					<span class="flex vcenter gap-sm">
					<button class="primary" on:click={() => {
						this.fs.writeFile(this.filePath, (new TextEncoder()).encode(this.fileData));
						// this.filePath = "";
						// this.fileData = "";
						// this.displayingFile = false;

						this.fs.syncfs(false, () => { })
					}}>
					<span class="material-symbols-rounded">save</span> <span class="label">Save</span>
					</button>
					<button on:click={() => {
						this.fileHiddenButNotRemoved = true;
						setTimeout(() => {
    				  this.filePath = "";
     					this.fileData = "";
  						this.displayingFile = false;
  						this.fileHiddenButNotRemoved = false;
						}, 150);
					}}>
					<span class="material-symbols-rounded">close</span> <span class="label">Close</span>
					</button>
					</span>
					</div>
					<textarea bind:value={use(this.fileData)}></textarea>
				</div>
			))}
			<div class="flex vcenter" id="uploadcontainer">
			<button class="large primary" title="Upload to this directory" on:click={() => {
				let input = h("input", { type: "file" });

				input.addEventListener("change", () => {
					let file = input.files[0];
					let reader = new FileReader();
					reader.onload = async () => {
						let data = new Uint8Array(reader.result);
						this.fs.writeFile(this.path + file.name, data);
						this.mount();
					};
					reader.readAsArrayBuffer(file);
				});

				document.body.appendChild(input);
				input.click();
				input.remove();

				this.fs.syncfs(false, () => { });
			}}>
				<span class="material-symbols-rounded">upload</span>
			</button>
			</div>
		</div>
	)
}
