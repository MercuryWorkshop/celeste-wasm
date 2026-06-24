import { Button, Icon } from "./ui/Button";

import tar, { Headers as TarHeaders, Pack } from "tar-stream";
import {
	fromWeb as streamFromWeb,
	toWeb as streamToWeb,
	/* @ts-expect-error */
} from "streamx-webstream";
import XXH64Worker from "./xxh64.worker?worker";

import iconFolder from "@ktibow/iconset-material-symbols/folder";
import iconDraft from "@ktibow/iconset-material-symbols/draft";
import iconDownload from "@ktibow/iconset-material-symbols/download";
import iconDelete from "@ktibow/iconset-material-symbols/delete";
import iconClose from "@ktibow/iconset-material-symbols/close";
import iconSave from "@ktibow/iconset-material-symbols/save";
import iconUploadFile from "@ktibow/iconset-material-symbols/upload-file";
import iconUploadFolder from "@ktibow/iconset-material-symbols/drive-folder-upload";
import iconArchive from "@ktibow/iconset-material-symbols/archive";
import iconUnarchive from "@ktibow/iconset-material-symbols/unarchive";
import iconArrowBack from "@ktibow/iconset-material-symbols/arrow-back";

export const FSAPI_UNAVAILABLE =
	(!window.showDirectoryPicker || !window.showOpenFilePicker) &&
	!(DataTransferItem.prototype as any).getAsEntry &&
	!DataTransferItem.prototype.webkitGetAsEntry;

// Cross origin sub frames aren't allowed to show a file picker, even though the
// picker functions exist on `window`. Calling them throws:
//   SecurityError: Failed to execute 'showOpenFilePicker' on 'Window':
//   Cross origin sub frames aren't allowed to show a file picker.
// Detect it up front by probing access to the top frame's location, which throws
// for cross origin frames but succeeds for the top frame and same origin frames.
const IN_CROSS_ORIGIN_FRAME = (() => {
	try {
		void window.top!.location.href;
		return false;
	} catch {
		return true;
	}
})();

export const PICKERS_UNAVAILABLE: boolean | "security" =
	!window.showDirectoryPicker || !window.showOpenFilePicker
		? true
		: IN_CROSS_ORIGIN_FRAME
			? "security"
			: false;

export const rootFolder = await navigator.storage.getDirectory();

export const TAR_TYPES = [
	{
		description: "TAR archive (.tar)",
		accept: { "application/x-tar": ".tar" },
	} as FilePickerAcceptType,
	{
		description: "GZip compressed TAR archive (.tar.gz)",
		accept: { "application/x-gzip": ".tar.gz", "application/gzip": ".tar.gz" },
	} as FilePickerAcceptType,
];

export async function calculateXXH64(path: string): Promise<string> {
	let split = path.split("/");
	let dir = await recursiveGetDirectory(rootFolder, split.slice(0, -1));
	let file = await dir.getFileHandle(split.at(-1)!);
	let buf = await file.getFile().then((r) => r.arrayBuffer());

	let worker = new XXH64Worker({ name: `xxh64-${path}` });
	let promise = new Promise<string>((res, rej) => {
		worker.onmessage = ({ data }) => res(data.digest);
		worker.onerror = (x) => rej(new Error(`failed to hash: ${x}`));
	});
	worker.postMessage({ buf }, [buf]);
	let res = await promise;
	worker.terminate();

	return res;
}
export async function calculateCelesteHash(): Promise<string> {
	return await calculateXXH64("CustomCeleste.dll");
}

export async function replaceHashes(hash: string) {
	const cacheDir = await recursiveGetDirectory(rootFolder, [
		"Celeste",
		"Mods",
		"Cache",
	]);
	for await (const [name, file] of cacheDir) {
		if (!name.endsWith(".sum") || file.kind !== "file") continue;

		const contents = await file.getFile().then((r) => r.text());
		const split = contents.split("\n");

		const old = split[0];
		split[0] = hash;

		const writable = await file.createWritable();
		await writable.write(split.join("\n"));
		await writable.close();
		console.debug(`replaced "${name}": "${old}" -> "${split[0]}"`);
	}
}
(self as any).calculateCelesteHash = calculateCelesteHash;
(self as any).replaceHashes = replaceHashes;

async function skipOobe() {
	await rootFolder.getFileHandle(".ContentExists", { create: true });
	await rootFolder.getFileHandle("CustomCeleste.dll", { create: true });
	const content = await rootFolder.getDirectoryHandle("Content", {
		create: true,
	});
	for (const folder of [
		"Dialog",
		"Effects",
		"FMOD",
		"Graphics",
		"Maps",
		"Monocle",
		"Overworld",
		"Tutorials",
	]) {
		await content.getDirectoryHandle(folder, { create: true });
	}
}
(self as any).skipOobe = skipOobe;

export async function copyFile(
	file: FileSystemFileHandle,
	to: FileSystemDirectoryHandle
) {
	const data = await file.getFile().then((r) => r.stream());
	const handle = await to.getFileHandle(file.name, { create: true });
	const writable = await handle.createWritable();
	await data.pipeTo(writable);
}

export async function copyFileForBadBrowsers(
	file: FileSystemFileEntry,
	to: FileSystemDirectoryHandle
) {
	const data = await new Promise<File>((resolve, reject) => {
		file.file(resolve, reject);
	});
	const handle = await to.getFileHandle(file.name, { create: true });
	const writable = await handle.createWritable();
	await data.stream().pipeTo(writable);
}

export async function countFolder(
	folder: FileSystemDirectoryHandle
): Promise<number> {
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

export async function countFolderForBadBrowsers(
	folder: FileSystemDirectoryEntry
): Promise<number> {
	let count = 0;
	async function countOne(folder: FileSystemDirectoryEntry) {
		const reader = folder.createReader();
		const entries = await new Promise<FileSystemEntry[]>((resolve, reject) => {
			reader.readEntries(resolve, reject);
		});

		for (const entry of entries) {
			if (entry.isFile) {
				count++;
			} else {
				await countOne(entry as FileSystemDirectoryEntry);
			}
		}
	}
	await countOne(folder);
	return count;
}

export async function copyFolder(
	folder: FileSystemDirectoryHandle,
	to: FileSystemDirectoryHandle,
	callback?: (name: string) => void
) {
	async function upload(
		from: FileSystemDirectoryHandle,
		to: FileSystemDirectoryHandle
	) {
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

export async function copyFolderForBadBrowsers(
	folder: FileSystemDirectoryEntry,
	to: FileSystemDirectoryHandle,
	callback?: (name: string) => void
) {
	async function upload(
		from: FileSystemDirectoryEntry,
		to: FileSystemDirectoryHandle
	) {
		const reader = from.createReader();
		const entries = await new Promise<FileSystemEntry[]>((resolve, reject) => {
			reader.readEntries(resolve, reject);
		});

		for (const entry of entries) {
			if (entry.isFile) {
				const file = entry as FileSystemFileEntry;
				const fileHandle = await to.getFileHandle(file.name, { create: true });
				const writable = await fileHandle.createWritable();
				file.file((f) => f.stream().pipeTo(writable));
				if (callback) callback(file.name);
			} else {
				const dir = entry as FileSystemDirectoryEntry;
				const newTo = await to.getDirectoryHandle(dir.name, { create: true });
				await upload(dir, newTo);
			}
		}
	}
	const newFolder = await to.getDirectoryHandle(folder.name, { create: true });
	await upload(folder, newFolder);
}

export async function hasContent(): Promise<boolean> {
	try {
		const directory = await rootFolder.getDirectoryHandle("Content", {
			create: false,
		});
		for (const child of [
			"Dialog",
			"Effects",
			"FMOD",
			"Graphics",
			"Maps",
			"Monocle",
			"Overworld",
			"Tutorials",
		]) {
			try {
				await directory.getDirectoryHandle(child, { create: false });
			} catch {
				return false;
			}
		}
		await rootFolder.getFileHandle(".ContentExists", { create: false });
		return true;
	} catch {
		return false;
	}
}

async function createEntry(
	pack: Pack,
	header: TarHeaders,
	file?: ReadableStream<Uint8Array>
) {
	let resolve: () => void = null!;
	let reject: (err: any) => void = null!;
	const promise = new Promise<void>((res, rej) => {
		resolve = res;
		reject = rej;
	});

	const entry = pack.entry(header, (err) => {
		if (err) reject(err);
		else resolve();
	});

	if (file) {
		const reader = file.getReader();
		while (true) {
			const { value, done } = await reader.read();
			if (done || !value) break;

			entry.write(value);
		}

		entry.end();
	} else {
		entry.end();
	}

	await promise;
}

export function createTar(
	folder: FileSystemDirectoryHandle,
	callback?: (type: "directory" | "file", name: string) => void
): ReadableStream {
	const archive = tar.pack();

	async function pack(pathPrefix: string, folder: FileSystemDirectoryHandle) {
		for await (const [name, entry] of folder) {
			if (callback) callback(entry.kind, name);

			if (entry.kind == "file") {
				const file = await entry.getFile();
				const stream = file.stream();

				await createEntry(
					archive,
					{
						name: pathPrefix + name,
						type: entry.kind,
						size: file.size,
					},
					stream
				);
			} else {
				await createEntry(archive, {
					name: pathPrefix + name,
					type: entry.kind,
				});

				await pack(pathPrefix + name + "/", entry);
			}
		}
	}
	pack("", folder).then(() => archive.finalize());

	return streamToWeb(archive);
}

export async function extractTar(
	stream: ReadableStream<Uint8Array>,
	folder: FileSystemDirectoryHandle,
	callback?: (type: "directory" | "file", name: string) => void
) {
	const tarInput = streamFromWeb(stream);
	const archive = tar.extract();

	archive.on("entry", async (header, stream, next) => {
		const body: ReadableStream<Uint8Array> = streamToWeb(stream);

		async function consume() {
			const reader = body.getReader();

			while (true) {
				const { done, value } = await reader.read();
				if (done || !value) break;
			}
		}

		const path = header.name.split("/");
		if (path[path.length - 1] === "") path.pop();
		if (path[0] === folder.name) path.shift();
		if (path.length === 0) {
			await consume();
			next();
			return;
		}

		let handle = folder;
		for (const name of path.splice(0, path.length - 1)) {
			handle = await handle.getDirectoryHandle(name, { create: true });
		}

		if (header.type === "directory") {
			await handle.getDirectoryHandle(path[0], { create: true });
			await consume();

			if (callback) callback("directory", path[0]);
		} else if (header.type === "file") {
			const file = await handle.getFileHandle(path[0], { create: true });
			const writable = await file.createWritable();
			await body.pipeTo(writable);

			if (callback) callback("file", path[0]);
		} else {
			await consume();
		}

		next();
	});

	const promise = new Promise<void>((res, rej) => {
		archive.on("finish", () => res());
		archive.on("error", (err) => rej(err));
	});

	tarInput.pipe(archive);
	await promise;
}

export async function recursiveGetDirectory(
	dir: FileSystemDirectoryHandle,
	path: string[]
): Promise<FileSystemDirectoryHandle> {
	if (path.length === 0) return dir;
	return recursiveGetDirectory(
		await dir.getDirectoryHandle(path[0]),
		path.slice(1)
	);
}

export const OpfsExplorer: Component<
	{
		open: boolean;
	},
	{
		path: FileSystemDirectoryHandle;
		components: string[];
		entries: { name: string; entry: FileSystemHandle }[];

		editing: FileSystemFileHandle | null;
		uploading: boolean;
		downloading: boolean;
	}
> = function () {
	this.path = rootFolder;
	this.components = [];
	this.entries = [];

	this.uploading = false;
	this.downloading = false;

	this.css = `
		display: flex;
		flex-direction: column;
		gap: 1em;
		min-height: min(44rem, 90vh);

		.path {
			display: flex;
			align-items: center;
			gap: 0.5rem;
			margin: 0 0.5rem;
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
			gap: 0.5rem;

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
			border: 2px solid var(--surface4);
			border-radius: 0.5rem;
		}

		.expand { flex: 1 }
		.hidden { visibility: hidden }

		.archive {
			display: flex;
			flex-direction: row;
			gap: 0.5em;

		}

		.archive > * {
		  flex-grow: 1;
		}
	`;

	useChange([this.open], () => (this.path = this.path));

	useChange([this.path], async () => {
		this.components = (await rootFolder.resolve(this.path)) || [];

		let entries = [];
		if (this.components.length > 0) {
			entries.push({
				name: "..",
				entry: await recursiveGetDirectory(
					rootFolder,
					this.components.slice(0, this.components.length - 1)
				),
			});
		}
		for await (const [name, entry] of this.path) {
			entries.push({
				name,
				entry,
			});
		}
		entries.sort((a, b) => {
			const kind = a.entry.kind.localeCompare(b.entry.kind);
			return kind === 0 ? a.name.localeCompare(b.name) : kind;
		});
		this.entries = entries;
	});

	const uploadFile = async () => {
		const files = await showOpenFilePicker({ multiple: true });
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
	const downloadArchive = async () => {
		const dirName = this.components.at(-1) || "celeste-wasm";
		const file = await showSaveFilePicker({
			excludeAcceptAllOption: true,
			suggestedName: dirName + ".tar",
			types: TAR_TYPES,
		});

		this.downloading = true;

		let tar = createTar(this.path, (type, name) =>
			console.log(`tarring ${type} ${name}`)
		);
		if (file.name.endsWith(".gz"))
			tar = tar.pipeThrough(new CompressionStream("gzip"));

		const fileStream = await file.createWritable();
		await tar.pipeTo(fileStream);

		this.downloading = false;
	};
	const uploadArchive = async () => {
		const files = await showOpenFilePicker({ multiple: true });
		this.uploading = true;
		for (const file of files) {
			let tar = await file.getFile().then((r) => r.stream());
			if (file.name.endsWith(".gz"))
				tar = tar.pipeThrough(new DecompressionStream("gzip"));
			await extractTar(tar, this.path, (type, name) =>
				console.log(`untarring ${type} ${name}`)
			);
		}
		this.uploading = false;
	};

	const uploadDisabled = use(this.uploading, (x) => x || !!PICKERS_UNAVAILABLE);
	const downloadDisabled = use(
		this.downloading,
		(x) => x || !!PICKERS_UNAVAILABLE
	);

	return (
		<div>
			<div class="path">
				{$if(
					use(this.components, (x) => x.length > 0),
					<Button
						type="normal"
						icon="full"
						disabled={false}
						on:click={async () => {
							this.path = this.entries[0].entry as FileSystemDirectoryHandle;
						}}
						title={"Up A Level"}
					>
						<Icon icon={iconArrowBack} />
					</Button>
				)}
				<h3>
					{use(this.components, (x) =>
						x.length == 0 ? "Root Directory" : "/" + x.join("/")
					)}
				</h3>
				<div class="expand" />
				<Button
					type="normal"
					icon="full"
					disabled={uploadDisabled}
					on:click={uploadFile}
					title={"Upload File"}
				>
					<Icon icon={iconUploadFile} />
				</Button>
				<Button
					type="normal"
					icon="full"
					disabled={uploadDisabled}
					on:click={uploadFolder}
					title={"Upload Folder"}
				>
					<Icon icon={iconUploadFolder} />
				</Button>
			</div>
			{$if(use(this.uploading), <span>Uploading files...</span>)}
			{$if(use(this.downloading), <span>Downloading files...</span>)}
			<div class="entries">
				{use(this.entries, (x) =>
					x
						.filter((x) => x.name != "..")
						.map((x) => {
							const icon =
								x.entry.kind === "directory" ? iconFolder : iconDraft;
							const remove = async (e: Event) => {
								e.stopImmediatePropagation();
								if (this.editing?.name === x.name) {
									this.editing = null;
								}
								await this.path.removeEntry(x.name, { recursive: true });
								this.path = this.path;
							};
							const download = async (e: Event) => {
								e.stopImmediatePropagation();
								if (x.entry.kind === "file") {
									const entry = x.entry as FileSystemFileHandle;
									const blob = await entry.getFile();

									const url = URL.createObjectURL(blob);
									const a = document.createElement("a");
									a.href = url;
									a.download = x.name;
									a.click();

									await new Promise((r) => setTimeout(r, 100));
									URL.revokeObjectURL(url);
								}
							};
							const action = () => {
								if (x.entry.kind === "directory") {
									this.editing = null;
									this.path = x.entry as FileSystemDirectoryHandle;
								} else {
									this.editing = x.entry as FileSystemFileHandle;
								}
							};

							return (
								<Button
									on:click={action}
									icon="none"
									type="listitem"
									disabled={false}
									class="entry"
								>
									<Icon icon={x.name == ".." ? iconUploadFolder : icon} />
									<span>{x.name === ".." ? "Parent Directory" : x.name}</span>
									<div class="expand" />
									<Button
										class={x.entry.kind !== "file" ? "hidden" : ""}
										on:click={download}
										icon="full"
										type="listaction"
										disabled={false}
										title={"Download File"}
									>
										<Icon icon={iconDownload} />
									</Button>
									<Button
										class={x.name === ".." ? "hidden" : ""}
										on:click={remove}
										icon="full"
										type="listaction"
										disabled={false}
										title={"Delete File"}
									>
										<Icon icon={iconDelete} />
									</Button>
								</Button>
							);
						})
				)}
			</div>
			{use(this.editing, (file) => {
				if (file) {
					const area = (<textarea />) as HTMLTextAreaElement;
					area.value = "Loading file...";
					file
						.getFile()
						.then((r) => r.text())
						.then((r) => (area.value = r));

					const save = async () => {
						const writable = await file.createWritable();
						await writable.write(area.value);
						await writable.close();
						this.editing = null;
					};

					return (
						<div class="editor">
							<div class="controls">
								<div class="name">{file.name}</div>
								<div class="expand" />
								<Button
									on:click={save}
									icon="left"
									type="primary"
									disabled={false}
								>
									<Icon icon={iconSave} />
									Save
								</Button>
								<Button
									on:click={() => (this.editing = null)}
									icon="full"
									type="normal"
									disabled={false}
								>
									<Icon icon={iconClose} />
								</Button>
							</div>
							{area}
						</div>
					);
				}
			})}
			<div style={{ flexGrow: 1 }} />
			<div class="archive">
				<Button
					type="normal"
					icon="full"
					disabled={uploadDisabled}
					on:click={uploadArchive}
				>
					<Icon icon={iconUnarchive} /> Upload Folder Archive
				</Button>
				<Button
					type="normal"
					icon="full"
					disabled={downloadDisabled}
					on:click={downloadArchive}
				>
					<Icon icon={iconArchive} /> Download Folder Archive
				</Button>
			</div>
		</div>
	);
};
