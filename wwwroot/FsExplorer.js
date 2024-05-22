export function FsExplorer() {
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
        width: min(960px, 100%);
    `

    return html`
        <div>
            <pre>${use(this.path)}</pre>
            ${use(this.listing, r => r.map((r) => {
                let mode = this.fs.stat(this.path + r).mode;
                if (this.fs.isDir(mode)) {
                    return html`
                        <div on:click=${() => {
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
                        }}><pre>${r}/</pre></div>
                    `
                } else {
                    return html`
                        <div on:click=${() => {
                            this.displayingFile = true;
                            this.filePath = this.path + r;
                            try {
                                this.fileData = (new TextDecoder()).decode(this.fs.readFile(this.path + r));
                            } catch {}
                        }}>
                            <pre>${r}</pre>
                            <button on:click=${(e) => {
                                let data = this.fs.readFile(this.path + r);
                                let el = html`<a href=${URL.createObjectURL(new Blob([data]))} download=${r}></a>`
                                el.click();
                                e.stopPropagation();
                            }}>download</button>
                        </div>
                    `
                }
            }))}
            ${$if(use(this.displayingFile), html`
                <div>
                    <div>file: <pre>${use(this.filePath)}</pre></div>
                    <button on:click=${() => {
                        this.fs.writeFile(this.filePath, (new TextEncoder()).encode(this.fileData));
                        this.filePath = "";
                        this.fileData = "";
                        this.displayingFile = false;
                    }}>saveandclose</button>
                    <textarea bind:value=${use(this.fileData)}></textarea>
                <div>
            `)}
        </div>
    `
}

