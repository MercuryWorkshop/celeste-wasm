export function FSExplorer() {
	this.path = "/";
	this.listing = [];

	this.displayingFile = false;
	this.filePath = "";
	this.fileData = "";

	this.mount = async () => {
		await window.initPromise;
		this.fs = window.FS;
		this.listing = this.fs.readdir(this.path);
	}

	this.css = `
		width: 100%;

		.item {
			display: flex;
			align-items: center;
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

		button {
			float: right;
			bottom: 0;
		}

		#path {
			font-size: 1.5rem;
			font-weight: 650;
			font-family: var(--font-display);
			margin-block: 0.5rem;
		}
    `

	return (
		<div>
			<div id="path">{use(this.path, path => path == "/" ? "File Browser" : path)}</div>
			{use(this.listing, r => r.map((r) => {
				let mode = this.fs.stat(this.path + r).mode;
				if (this.fs.isDir(mode)) {
					return (
						<div class="item" role="button" on:click={() => {
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
							<span class="material-symbols-rounded">folder</span>
							<pre>{r}/</pre>
						</div>
					)
				} else {
					return (
						<div class="item" role="button" on:click={() => {
							this.displayingFile = true;
							this.filePath = this.path + r;
							try {
								this.fileData = (new TextDecoder()).decode(this.fs.readFile(this.path + r));
							} catch { }
						}}>
							<span class="material-symbols-rounded">description</span>
							<pre>{r}</pre>
							<button on:click={(e) => {
								let data = this.fs.readFile(this.path + r);
								let el = (<a href={URL.createObjectURL(new Blob([data]))} download={r}></a>)
								el.click();
								e.stopPropagation();
							}}>download</button>
							<button on:click={(e) => {
								this.fs.unlink(this.path + r);
								this.mount();
								this.fs.syncfs(false, () => { });
								e.stopPropagation();
							}}>delete</button>
						</div>
					)
				}
			}))}
			{$if(use(this.displayingFile), (
				<div>
					<div>file: <pre>{use(this.filePath)}</pre></div>
					<button on:click={() => {
						this.fs.writeFile(this.filePath, (new TextEncoder()).encode(this.fileData));
						this.filePath = "";
						this.fileData = "";
						this.displayingFile = false;

						this.fs.syncfs(false, () => { })
					}}>saveandclose</button>
					<textarea bind:value={use(this.fileData)}></textarea>
				</div>
			))}
			<button class="large" on:click={() => {
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
	)
}
