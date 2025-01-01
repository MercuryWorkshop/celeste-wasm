import { Button, Icon } from "./ui";

import iconFolder from "@ktibow/iconset-material-symbols/folder";
import iconDraft from "@ktibow/iconset-material-symbols/draft";
import iconDelete from "@ktibow/iconset-material-symbols/delete";
import iconClose from "@ktibow/iconset-material-symbols/close";
import iconSave from "@ktibow/iconset-material-symbols/save";
import iconUploadFile from "@ktibow/iconset-material-symbols/upload-file";
import iconUploadFolder from "@ktibow/iconset-material-symbols/drive-folder-upload";

export const PICKERS_UNAVAILABLE = !window.showDirectoryPicker || !window.showOpenFilePicker;

export const rootFolder = await navigator.storage.getDirectory();

export async function copyFile(file: FileSystemFileHandle, to: FileSystemDirectoryHandle) {
	const data = await file.getFile().then(r => r.stream());
	const handle = await to.getFileHandle(file.name, { create: true });
	const writable = await handle.createWritable()
	await data.pipeTo(writable);
}

export async function countFolder(folder: FileSystemDirectoryHandle): Promise<number> {
	let count = 0;
	async function countOne(folder: FileSystemDirectoryHandle) {
		for await (const [_, entry] of folder) {
			if (entry.kind === "file") {
				count++;
			} else {
				await countOne(entry);
			}
		}
	}
	await countOne(folder);
	return count;
}

export async function copyFolder(folder: FileSystemDirectoryHandle, to: FileSystemDirectoryHandle, callback?: (name: string) => void) {
	async function upload(from: FileSystemDirectoryHandle, to: FileSystemDirectoryHandle) {
		for await (const [name, entry] of from) {
			if (entry.kind === "file") {
				await copyFile(entry, to);
				if (callback) callback(name);
			} else {
				const newTo = await to.getDirectoryHandle(name, { create: true });
				await upload(entry, newTo);
			}
		}
	}
	const newFolder = await to.getDirectoryHandle(folder.name, { create: true });
	await upload(folder, newFolder);
}

export async function hasContent(): Promise<boolean> {
	try {
		await rootFolder.getDirectoryHandle("Content", { create: false })
		return true;
	} catch {
		return false;
	}
}

async function recursiveGetDirectory(dir: FileSystemDirectoryHandle, path: string[]): Promise<FileSystemDirectoryHandle> {
	if (path.length === 0) return dir;
	return recursiveGetDirectory(await dir.getDirectoryHandle(path[0]), path.slice(1));
}

export const OpfsExplorer: Component<{
	open: boolean,
}, {
	path: FileSystemDirectoryHandle,
	components: string[],
	entries: { name: string, entry: FileSystemHandle }[],

	editing: FileSystemFileHandle | null,
	uploading: boolean,
}> = function() {
	this.path = rootFolder;
	this.components = [];
	this.entries = [];

	this.uploading = false;

	this.css = `
		display: flex;
		flex-direction: column;
		gap: 1em;

		.path {
			display: flex;
			align-items: center;
			gap: 0.5rem;
		}
		.path h3 {
			font-family: var(--font-mono);
			margin: 0;
		}

		.entries {
			display: flex;
			flex-direction: column;
			gap: 0.5em;
		}

		.entry {
			display: flex;
			align-items: center;
			gap: 0.5em;

			font-family: var(--font-mono);
		}

		.entry > svg {
			width: 1.5rem;
			height: 1.5rem;
		}

		.editor {
			display: flex;
			flex-direction: column;
			gap: 0.5em;
		}
		.editor .controls {
			display: flex;
			gap: 0.5em;
			align-items: center;
		}
		.editor .controls .name {
			font-family: var(--font-mono);
		}
		.editor textarea {
			min-height: 16rem;
			background: var(--bg-sub);
			color: var(--fg);
			border: 2px solid var(--surface6);
			border-radius: 0.5rem;
		}

		.expand { flex: 1 }
		.hidden { visibility: hidden }
	`;

	useChange([this.open], () => this.path = this.path);

	useChange([this.path], async () => {
		this.components = await rootFolder.resolve(this.path) || [];

		this.entries = [];
		if (this.components.length > 0) {
			this.entries = [...this.entries, {
				name: "..",
				entry: await recursiveGetDirectory(rootFolder, this.components.slice(0, this.components.length - 1)),
			}]
		}
		for await (const [name, entry] of this.path) {
			this.entries = [...this.entries, {
				name,
				entry
			}];
		}
	});

	const uploadFile = async () => {
		const files = await showOpenFilePicker();
		this.uploading = true;
		for (const file of files) {
			await copyFile(file, this.path);
		}
		this.path = this.path;
		this.uploading = false;
	};
	const uploadFolder = async () => {
		const folder = await showDirectoryPicker();
		this.uploading = true;
		await copyFolder(folder, this.path);
		this.path = this.path;
		this.uploading = false;
	};

	const uploadDisabled = use(this.uploading, x => x || PICKERS_UNAVAILABLE);

	return (
		<div>
			<div class="path">
				<h3>{use(this.components, x => "/" + x.join("/"))}</h3>
				<div class="expand" />
				<Button type="normal" icon="full" disabled={uploadDisabled} on:click={uploadFile}>
					<Icon icon={iconUploadFile} />
				</Button>
				<Button type="normal" icon="full" disabled={uploadDisabled} on:click={uploadFolder}>
					<Icon icon={iconUploadFolder} />
				</Button>
			</div>
			{$if(use(this.uploading), <span>Uploading files...</span>)}
			<div class="entries">
				{use(this.entries, x => x.map(x => {
					const icon = x.entry.kind === "directory" ? iconFolder : iconDraft;
					const remove = async (e: Event) => {
						e.stopImmediatePropagation();
						if (this.editing?.name === x.name) {
							this.editing = null;
						}
						await this.path.removeEntry(x.name, { recursive: true });
						this.path = this.path;
					};
					const action = () => {
						if (x.entry.kind === "directory") {
							this.editing = null;
							this.path = x.entry as FileSystemDirectoryHandle;
						} else {
							this.editing = x.entry as FileSystemFileHandle;
						}
					}

					return (
						<Button on:click={action} icon="left" type="listitem" disabled={false} class="entry">
							<Icon icon={icon} />
							<span>{x.name}</span>
							<div class="expand" />
							<Button class={x.name === ".." ? "hidden" : ""} on:click={remove} icon="full" type="listaction" disabled={false}>
								<Icon icon={iconDelete} />
							</Button>
						</Button>
					)
				}))}
			</div>
			{use(this.editing, file => {
				if (file) {
					const area = <textarea /> as HTMLTextAreaElement;
					area.value = "Loading file...";
					file.getFile().then(r => r.text()).then(r => area.value = r);

					const save = async () => {
						const writable = await file.createWritable()
						await writable.write(area.value);
						await writable.close();
						this.editing = null;
					}

					return (
						<div class="editor">
							<div class="controls">
								<div class="name">{file.name}</div>
								<div class="expand" />
								<Button on:click={save} icon="left" type="primary" disabled={false}>
									<Icon icon={iconSave} />
									Save
								</Button>
								<Button on:click={() => this.editing = null} icon="full" type="normal" disabled={false}>
									<Icon icon={iconClose} />
								</Button>
							</div>
							{area}
						</div>
					)
				}
			})}
		</div>
	)
}
