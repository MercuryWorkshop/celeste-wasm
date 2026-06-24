import { Logo, NAME } from "./main";
import { Switch } from "./ui/Switch";
import { Button, Icon, Link } from "./ui/Button";
import {
	copyFile,
	copyFileForBadBrowsers,
	copyFolder,
	copyFolderForBadBrowsers,
	countFolder,
	countFolderForBadBrowsers,
	extractTar,
	hasContent,
	FSAPI_UNAVAILABLE,
	PICKERS_UNAVAILABLE,
	rootFolder,
} from "./fs";
import {
	DownloadApp,
	gameState,
	PatchCeleste,
	pickDownloadsFolder,
} from "./game/dotnet";
import { SteamLogin, steamState } from "./steam";
import { LogView } from "./game";

import iconFolderOpen from "@ktibow/iconset-material-symbols/folder-open-outline";
import iconCloudUpload from "@ktibow/iconset-material-symbols/cloud-upload";
//import iconDownload from "@ktibow/iconset-material-symbols/download";
import iconEncrypted from "@ktibow/iconset-material-symbols/encrypted";
import iconArchive from "@ktibow/iconset-material-symbols/archive";
import iconUnarchive from "@ktibow/iconset-material-symbols/unarchive";
import iconFolderZip from "@ktibow/iconset-material-symbols/folder-zip";
import iconManufacturing from "@ktibow/iconset-material-symbols/manufacturing";
import iconSettings from "@ktibow/iconset-material-symbols/settings";
import { Settings } from "./settings";
import { Dialog } from "./ui/Dialog";
import { event } from "./analytics";

const validateDirectory = async (directory: FileSystemDirectoryHandle) => {
	let content;
	try {
		content = await directory.getDirectoryHandle("Content", { create: false });
	} catch {
		return `Failed to find Content directory in selected folder`;
	}

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
			await content.getDirectoryHandle(child, { create: false });
		} catch {
			return `Failed to find subdirectory Content/${child}`;
		}
	}

	try {
		await directory.getFileHandle("Celeste.exe", { create: false });
	} catch {
		try {
			const orig = await directory.getDirectoryHandle("orig", {
				create: false,
			});
			await orig.getFileHandle("Celeste.exe", { create: false });
		} catch {
			return `Failed to find Celeste.exe in selected folder`;
		}
	}

	return "";
};

const validateDirectoryForBadBrowsers = async (
	entry: FileSystemEntry | null
) => {
	if (!entry || !entry.isDirectory) {
		return "what?";
	}
	let directory = entry as FileSystemDirectoryEntry;
	let reader = directory.createReader();
	let entries: FileSystemEntry[] = [];
	return new Promise<string>((resolve) => {
		reader.readEntries((result: FileSystemEntry[]) => {
			entries = result;
			let content = entries.find((e) => e.name === "Content");
			if (!content || !content.isDirectory) {
				resolve("Failed to find Content directory in selected folder");
				return;
			}
			let contentDir = content as FileSystemDirectoryEntry;
			let contentReader = contentDir.createReader();
			contentReader.readEntries((contentEntries: FileSystemEntry[]) => {
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
					if (!contentEntries.some((e) => e.name === child && e.isDirectory)) {
						resolve(`Failed to find subdirectory Content/${child}`);
						return;
					}
				}
			});
			resolve("");
		});
	});
};

const initialHasContent = await hasContent();
let initialIsPatched = false;
try {
	await rootFolder.getFileHandle("CustomCeleste.dll", { create: false });
	initialIsPatched = true;
} catch {}

const Intro: Component<
	{
		"on:next": (
			type: "copy" | "extract" | "patch" | "download" | "done"
		) => void;
	},
	{
		disabled: boolean;
	}
> = function () {
	this.css = `
		.error {
			margin-block: 0.3em;
		}

		p {
			margin-block: 0.3em;
		}
	`;

	const next = (type: "copy" | "extract" | "patch" | "download" | "done") => {
		this.disabled = true;
		this["on:next"](type);
	};
	if (initialHasContent) {
		if (initialIsPatched) {
			queueMicrotask(() => next("done"));
		} else {
			next("patch");
		}
	}

	return (
		<div class="step">
			<p>
				This is a near-complete port of{" "}
				<Link href="https://www.celestegame.com/">Celeste</Link> and{" "}
				<Link href="https://everestapi.github.io/">Everest</Link> to the browser
				using <b>.NET 9's threaded WebAssembly support</b>. It also uses{" "}
				<Link href="https://github.com/r58playz/monomod">
					r58Playz's <b>MonoMod WASM port</b>
				</Link>{" "}
				to dynamically patch the game. It needs around 1GB of memory and will
				probably not work on low-end devices.
			</p>
			<p>
				You will need to own the FNA version of Celeste to use this port (you
				have the FNA version if not on Windows; the FNA version is also
				downloadable on steam).
			</p>
			<p>
				The setup background is from{" "}
				<Link href="https://www.fangamer.com/products/celeste-desk-mat-skies">
					Fangamer
				</Link>
				. The source is available on{" "}
				<Link href="https://github.com/MercuryWorkshop/celeste-wasm">
					GitHub
				</Link>
				.
			</p>
			{PICKERS_UNAVAILABLE === true ? (
				<div class="error">
					Your browser does not support the{" "}
					<Link href="https://developer.mozilla.org/en-US/docs/Web/API/Window/showDirectoryPicker">
						File System Access API
					</Link>
					. You will be unable to use the upload/download features in the filesystem viewer.
					Please switch to a chromium based browser.
				</div>
			) : null}
			{PICKERS_UNAVAILABLE === "security" ? (
				<div class="error">
					Your browser does not allow the{" "}
					<Link href="https://developer.mozilla.org/en-US/docs/Web/API/Window/showDirectoryPicker">
						File System Access API
					</Link>
					for security reasons on this page. You will be unable to use the upload/download features in the filesystem viewer.
				</div>
			) : null}
			{FSAPI_UNAVAILABLE ? (
				<div class="error">
					Your browser does not support the{" "}
					<Link href="https://developer.mozilla.org/en-US/docs/Web/API/DataTransferItem/webkitGetAsEntry">
						File and Directory Entries API
					</Link>
					. You will be unable to copy game assets from your local install of
					Celeste.
				</div>
			) : null}

			<Button
				on:click={() => next("copy")}
				type="primary"
				icon="left"
				disabled={use(this.disabled, (x) => x || FSAPI_UNAVAILABLE)}
			>
				<Icon icon={iconFolderOpen} />
				{FSAPI_UNAVAILABLE
					? "Copying local Celeste assets is unsupported"
					: "Copy local Celeste assets"}
			</Button>
			<Button
				on:click={() => next("extract")}
				type="primary"
				icon="left"
				disabled={use(this.disabled)}
			>
				<Icon icon={iconUnarchive} />
				Extract {NAME} archive
			</Button>
			{/*
			<Button
				on:click={() => next("download")}
				type="primary"
				icon="left"
				disabled={use(this.disabled)}
			>
				<Icon icon={iconDownload} />
				Download assets with Steam Login
			</Button>
			*/}
		</div>
	);
};

const Progress: Component<{ percent: number }, {}> = function () {
	this.css = `
		background: color-mix(in srgb, var(--surface1) 70%, transparent);
		border-radius: 1rem;
		height: 0.6rem;

		.bar {
			background: var(--accent);
			border-radius: 1rem;
			height: 0.6rem;
			transition: width 100ms ease;
		}
	`;

	return (
		<div>
			<div class="bar" style={use`width:${this.percent}%`} />
		</div>
	);
};

const Extract: Component<
	{
		"on:done": () => void;
	},
	{
		extracting: boolean;
		status: string;
		percent: number;
		input: HTMLInputElement;
	}
> = function () {
	this.css = `
		/* hacky */
		.center svg {
			transform: translateY(15%);
		}

		.file-input { display: none }
	`;

	const opfs = async () => {
		const files: FileList = await new Promise((res, rej) => {
			this.input.value = "";
			this.input.oncancel = rej;
			this.input.onchange = () => res(this.input.files!);
			this.input.click();
		});
		const file = files[0];

		let parsedSize = 0;
		const fileSize = file.size;

		const stream = file.stream();
		const reader = stream.getReader();
		const self = this;
		let progressStream = new ReadableStream({
			async pull(controller) {
				const { value, done } = await reader.read();

				if (!value || done) {
					controller.close();
				} else {
					controller.enqueue(value);

					parsedSize += value.byteLength;
					self.percent = (parsedSize / fileSize) * 100;
				}
			},
		});

		this.extracting = true;

		if (file.name.endsWith(".gz"))
			progressStream = progressStream.pipeThrough(
				new DecompressionStream("gzip")
			);
		await extractTar(progressStream, rootFolder, (type, name) =>
			console.log(`untarred ${type} ${name}`)
		);

		this.extracting = false;

		event("assets-provided", { option: "archive" });
		this["on:done"]();
	};

	return (
		<div class="step">
			<p class="center">
				Select a {NAME} exported .tar archive of the root directory. You can
				create this on a browser with {NAME} already set-up by clicking the
				archive button (<Icon icon={iconArchive} />) in the filesystem explorer
				while in the root directory.
			</p>
			{$if(use(this.extracting), <Progress percent={use(this.percent)} />)}
			<input type="file" class="file-input" accept=".tar,.tar.gz,.gz,application/x-tar,application/gzip" bind:this={use(this.input)} />
			<Button
				on:click={opfs}
				type="primary"
				icon="left"
				disabled={use(this.extracting)}
			>
				<Icon icon={iconFolderZip} />
				Select {NAME} archive
			</Button>
			{$if(use(this.status), <div class="error">{use(this.status)}</div>)}
		</div>
	);
};

const Copy: Component<
	{
		"on:done": () => void;
	},
	{
		copying: boolean;
		os: string;
		status: string;
		percent: number;
	}
> = function () {
	const opfs = async () => {
		const directory = await showDirectoryPicker();
		const res = await validateDirectory(directory);
		if (res) {
			this.status = res;
			return;
		}

		const contentFolder = await directory.getDirectoryHandle("Content", {
			create: false,
		});

		const max = await countFolder(contentFolder);
		let cnt = 0;
		this.copying = true;
		const before = performance.now();
		await copyFolder(contentFolder, rootFolder, (x) => {
			cnt++;
			this.percent = (cnt / max) * 100;
			console.debug(`copied ${x}: ${((cnt / max) * 100).toFixed(2)}`);
		});
		const after = performance.now();
		console.debug(`copy took ${(after - before).toFixed(2)}ms`);

		let celesteExe;
		try {
			let orig = await directory.getDirectoryHandle("orig", { create: false });
			celesteExe = await orig.getFileHandle("Celeste.exe", { create: false });
			console.debug("found everest install");
		} catch {
			celesteExe = await directory.getFileHandle("Celeste.exe", {
				create: false,
			});
		}
		await copyFile(celesteExe, rootFolder);

		await new Promise((r) => setTimeout(r, 250));
		await rootFolder.getFileHandle(".ContentExists", { create: true });
		event("assets-provided", { option: "opfs" });
		this["on:done"]();
	};

	const opfsForBadBrowsers = async (transfer: DataTransferItem) => {
		// mdn told me to check for this
		let handle: FileSystemEntry | null;
		if (
			"getAsEntry" in transfer &&
			typeof (transfer as any).getAsEntry === "function"
		) {
			handle = (transfer as any).getAsEntry();
		} else {
			handle = transfer.webkitGetAsEntry();
		}

		const res = await validateDirectoryForBadBrowsers(handle);
		if (res) {
			this.status = res;
			return;
		}

		const directory = handle as FileSystemDirectoryEntry;
		const reader = directory.createReader();
		reader.readEntries(async (entries: FileSystemEntry[]) => {
			const contentFolder = entries.find(
				(e) => e.name === "Content"
			) as FileSystemDirectoryEntry;
			const max = await countFolderForBadBrowsers(contentFolder);
			let cnt = 0;
			this.copying = true;
			const before = performance.now();
			await copyFolderForBadBrowsers(contentFolder, rootFolder, (x) => {
				cnt++;
				this.percent = (cnt / max) * 100;
				console.debug(`copied ${x}: ${((cnt / max) * 100).toFixed(2)}`);
			});
			const after = performance.now();
			console.debug(`copy took ${(after - before).toFixed(2)}ms`);

			let celesteExe;
			try {
				let orig = entries.find(
					(e) => e.name === "orig"
				) as FileSystemDirectoryEntry;
				let reader = orig.createReader();
				let subEntries = await new Promise<FileSystemEntry[]>((r) =>
					reader.readEntries(r)
				);
				celesteExe = subEntries.find(
					(e) => e.name === "Celeste.exe"
				) as FileSystemFileEntry;
				console.debug("found everest install");
			} catch {
				celesteExe = entries.find(
					(e) => e.name === "Celeste.exe"
				) as FileSystemFileEntry;
			}

			await copyFileForBadBrowsers(celesteExe!, rootFolder);
			await new Promise((r) => setTimeout(r, 250));
			await rootFolder.getFileHandle(".ContentExists", { create: true });

			event("assets-provided", { option: "opfs-bad" });
			this["on:done"]();
		});
	};

	let ua = navigator.userAgent;
	this.os = "";
	if (ua.includes("Win")) {
		this.os = "win";
	} else if (ua.includes("Mac")) {
		this.os = "darwin";
	} else if (ua.includes("Linux")) {
		this.os = "linux";
	}

	this.css = `
		code {
			overflow-wrap: break-word;
		}

		p {
			margin-block: 0.3em;
		}

		.droparea {
			position: relative;
			border: 2px dashed var(--surface6);
			color: var(--fg6);
			font-size: 1.05rem;
			font-weight: 500;
			border-radius: 1rem;
			padding: 2rem;
			text-align: center;
			background: color-mix(in srgb, var(--surface3) 45%, transparent);
			transition: border-color 0.2s ease;
		}

		.droparea.true {
			pointer-events: none;
			cursor: no-drop;

			* {
				cursor: no-drop;
			}

			background:  color-mix(in srgb, var(--bg-sub) 45%, transparent);
			color: var(--surface6);
			border-color: var(--surface4);
		}

		.droparea.dragover {
			border-color: var(--accent);
			color: color-mix(in srgb, var(--fg3) 92%, var(--accent));
			.dnd-bg-hover {
				transition: background-image 0.35s ease;
				background-image: radial-gradient(
					circle at center,
					color-mix(in srgb, var(--accent) 25%, transparent),
					transparent
				);
			}
		}

		.dnd-bg-hover {
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			border-radius: 1rem;
			z-index: 9;
			transition: background-image 0.35s ease;
			background-image: radial-gradient(
				circle at center,
				color-mix(in srgb, var(--surface3) 5%, transparent),
				transparent
			);
		}

		.dnd-content {
			z-index: 10;
			position: relative;
		}

		.dnd-icon {
			font-size: 2.65rem;
		}
	`;

	return (
		<div class="step">
			<p>
				Select your <b>FNA</b> Celeste install's directory. It will be copied to
				browser storage and can be removed in the file manager.
			</p>
			{this.os == "win" ? (
				<p>
					The directory for Steam installs of Celeste on your OS is usually
					located in{" "}
					<code>C:\Program Files (x86)\Steam\steamapps\common\Celeste</code>.
				</p>
			) : null}
			{this.os == "darwin" ? (
				<p>
					The directory for Steam installs of Celeste on your OS is usually
					located in:
					<br />{" "}
					<code>
						~/Library/Application
						Support/Steam/steamapps/common/Celeste/Celeste.app/Contents/Resources
					</code>
				</p>
			) : null}
			{this.os == "linux" ? (
				<p>
					The directory for Steam installs of Celeste is usually located in{" "}
					<code>~/.steam/root/steamapps/common/Celeste</code>.
				</p>
			) : null}
			{this.os == "" ? (
				<p>
					The directory for Steam installs of Celeste is usually located in one
					of these locations:
					<ul>
						<li>
							<code>~/.steam/root/steamapps/common/Celeste</code>
						</li>
						<li>
							<code>C:\Program Files (x86)\Steam\steamapps\common\Celeste</code>
						</li>
						<li>
							<code>
								~/Library/Application
								Support/Steam/steamapps/common/Celeste/Celeste.app/Contents/Resources
							</code>
						</li>
					</ul>
				</p>
			) : null}
			<div class="warning">
				If you get an error stating it can't open the folder because it
				"contains system files", try copying it to another location first.
			</div>
			{$if(use(this.copying), <Progress percent={use(this.percent)} />)}
			{PICKERS_UNAVAILABLE ? null : (
				<Button
					on:click={opfs}
					type="primary"
					icon="left"
					disabled={use(this.copying)}
				>
					<Icon icon={iconFolderOpen} />
					Select Celeste directory
				</Button>
			)}
			<div
				class={use`droparea ${this.copying}`}
				on:drop={async (e: DragEvent) => {
					(
						(e.currentTarget || e.target) as HTMLElement | null
					)?.classList.remove("dragover");
					e.preventDefault();
					if (!e.dataTransfer || !e.dataTransfer.items || this.copying) return;
					const transfer = e.dataTransfer.items[0];
					await opfsForBadBrowsers(transfer);
				}}
				on:dragover={(e: DragEvent) => {
					e.preventDefault();
					e.dataTransfer!.dropEffect = "copy";
					if (this.copying) return;
					((e.currentTarget || e.target) as HTMLElement).classList.add(
						"dragover"
					);
				}}
				on:dragleave={(e: DragEvent) => {
					(
						(e.currentTarget || e.target) as HTMLElement | null
					)?.classList.remove("dragover");
				}}
			>
				<div class="dnd-bg-hover"></div>
				<div class="dnd-content">
					<Icon icon={iconCloudUpload} class="dnd-icon" />
					{PICKERS_UNAVAILABLE ? (
						<p>Drag and drop Celeste directory</p>
					) : (
						<p>Or, drag and drop one here</p>
					)}
				</div>
			</div>
			{$if(use(this.status), <div class="error">{use(this.status)}</div>)}
		</div>
	);
};

export const Download: Component<
	{
		"on:done": () => void;
	},
	{
		downloading: boolean;
		downloadDisabled: boolean;
		status: string;
		percent: number;
		input: HTMLInputElement;
	}
> = function () {
	this.css = `
		gap: 2.5rem;
		font-size: 15pt;

		.console {
			font-size: initial;
			height: 10em;
		}
	`;

	useChange([this.downloading, steamState.login], () => {
		this.downloadDisabled = this.downloading || steamState.login != 2;
	});

	const download = async () => {
		this.downloading = true;
		let result = await DownloadApp();
		this.downloading = false;
		if (result) {
			await rootFolder.getFileHandle(".ContentExists", { create: true });
			event("assets-provided", { option: "steam" });
			this["on:done"]();
		} else {
			console.error("FAILED TO DOWNLOAD. TRY RELOADING");
		}
	};

	const downloadscuffed = async () => {
		await pickDownloadsFolder();
		await download();
	};

	return (
		<div>
			{$if(
				use(gameState.ready),
				<div>
					{$if(
						use(steamState.login, (l) => l == 2),
						<div>
							<p>Logged into Steam successfully!</p>
							<Button
								type="primary"
								icon="left"
								disabled={use(this.downloadDisabled)}
								on:click={download}
							>
								<Icon icon={iconEncrypted} />
								Download Assets
							</Button>
							<Button
								type="primary"
								icon="left"
								disabled={use(this.downloadDisabled)}
								on:click={downloadscuffed}
							>
								<Icon icon={iconEncrypted} />
								Download Assets (scuffed) (chrome os only)
							</Button>
						</div>,
						<SteamLogin />
					)}

					{$if(use(this.downloading), <Progress percent={use(this.percent)} />)}
				</div>,
				<div class="loading">
					<p>Initializing Connection to Steam... ?</p>
				</div>
			)}

			{$if(
				use(steamState.login, (l) => l == 2),
				<div class="console">
					<LogView scrolling={false} />
				</div>
			)}
		</div>
	);
};

export const Patch: Component<
	{
		"on:done": () => void;
	},
	{
		patching: boolean;
		everest: boolean;
	}
> = function () {
	this.patching = false;
	this.everest = false;
	this.css = `
		display: flex;
		flex-direction: column;
		gap: 1rem;

		.component-log {
		  scrollbar-width: none;
		}

		.console {
			display: flex;
			font-size: initial;
			height: 10em;
		}
	`;
	const patch = async () => {
		this.patching = true;
		try {
			await PatchCeleste(this.everest);
			this.patching = false;
			event("patched", { everest: this.everest });
			this["on:done"]();
		} catch {
			console.debug("================================================");
			console.error("[!!!] There was an error patching Celeste!");
			console.log("Please try again or reload the page.");
			console.debug("================================================");
			this.patching = false;
		}
	};

	return (
		<div>
			<p>
				We're going to patch Celeste with MonoMod for neccesary WASM fixes. You
				can also optionally install the Everest Mod Loader, but it will take
				longer to install.
			</p>
			<Switch
				title={"Install Everest Mod Loader?"}
				bind:on={use(this.everest)}
				bind:disabled={use(this.patching)}
			/>

			<Button
				type="primary"
				icon="left"
				on:click={patch}
				disabled={use(this.patching)}
			>
				<Icon icon={iconManufacturing} />
				Patch Celeste
			</Button>

			<div class="console">
				<LogView scrolling={true} />
			</div>
		</div>
	);
};

export const Splash: Component<
	{
		"on:next": (animation: boolean) => void;
		start: () => Promise<void>;
	},
	{
		next: "intro" | "copy" | "extract" | "download" | "patch";
		settingsOpen: boolean;
	}
> = function () {
	this.css = `
		position: relative;

		width: 100%;
		height: 100%;

		--animation: cubic-bezier(0.25, 0, 0.25, 1);

		.splash, .blur, .main {
			position: absolute;
			width: 100%;
			height: 100%;
			top: 0;
			left: 0;
		}

		.splash {
			object-fit: cover;
			z-index: 101;
		}

		.blur {
			backdrop-filter: blur(1rem);
			background-color: color-mix(in srgb, var(--bg) 35%, transparent);
			z-index: 102;
			transition: backdrop-filter .65s var(--animation) 0.35s, background-color .65s var(--animation) 0.35s;

			@starting-style {
	      backdrop-filter: blur(2.5rem);
  			background-color: color-mix(in srgb, var(--bg) 80%, transparent);
			}
		}

		.main {
			display: flex;
			align-items: center;
			justify-content: center;
			z-index: 103;
		}

		.container {
			backdrop-filter: blur(0.5vw);
			background-color: color-mix(in srgb, var(--bg) 80%, transparent);
      box-shadow: 0px 0px 20px color-mix(in srgb, var(--surface0) 40%, transparent);
			width: min(40rem, 100%);
			margin: 0 1rem;
			padding: 1.3em;
			border-radius: 1.5rem;

			color: var(--fg);

			display: flex;
			flex-direction: column;
			gap: 0.5rem;

			opacity: 1;
			transform: translateY(0vh) rotate3d(1, 0, 0, 0);
			transition: opacity .65s var(--animation) 0.35s, transform .65s var(--animation) 0.35s;
			transform-origin: 50% 0%;
			perspective: 1250px;

			@starting-style {
			  opacity: 0;
				transform: translateY(5vh) rotate3d(1, 0, 0, 15deg);
			}
		}

		.logo {
			display: flex;
			justify-content: center;
		}

		.step {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
		}

		.main > .container > .component-btn {
      width: 2.5em;
      position: fixed;
      right: 1.3rem;
		}
	`;
	this.next = "intro";
	this.settingsOpen = false;

	return (
		<div>
			<img alt="Splash image" class="splash" src="./splash.webp" />
			<div class="blur" />
			<div class="main">
				<div class="container">
					<div class="logo">
						<Logo />
					</div>
					{use(this.next, (x) => {
						if (x === "intro") {
							return (
								<Intro
									on:next={async (x) => {
										if (x === "done") {
											this["on:next"](false);
											return;
										}
										await this.start();
										this.next = x;
									}}
								/>
							);
						} else if (x === "copy") {
							return <Copy on:done={() => (this.next = "patch")} />;
						} else if (x === "extract") {
							return <Extract on:done={() => this["on:next"](true)} />;
						} else if (x === "download") {
							return <Download on:done={() => (this.next = "patch")} />;
						} else if (x === "patch") {
							return <Patch on:done={() => this["on:next"](true)} />;
						}
					})}
					<Button
						type="primary"
						icon="full"
						disabled={false}
						on:click={() => (this.settingsOpen = true)}
						title="Settings"
					>
						<Icon icon={iconSettings} />
					</Button>
				</div>
			</div>
			<Dialog name="Settings" bind:open={use(this.settingsOpen)}>
				<Settings />
			</Dialog>
		</div>
	);
};
