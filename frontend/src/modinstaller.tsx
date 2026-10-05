import { loadedLibcurlPromise } from "./game/index";
import { TextField } from "./ui/TextField";
import { Button, Icon } from "./ui/Button";
import { rootFolder } from "./fs";
import { epoxyFetch } from "./epoxy";
import { marked } from "marked";
import DOMPurify from "dompurify";
import iconSearch from "@ktibow/iconset-material-symbols/search";
import iconDownload from "@ktibow/iconset-material-symbols/download";
import iconCheck from "@ktibow/iconset-material-symbols/check";

DOMPurify.addHook("afterSanitizeAttributes", (node) => {
	if (node.nodeName === "A") {
		const href = node.getAttribute("href") ?? "";
		if (/^https?:\/\//i.test(href)) {
			node.setAttribute("rel", "noopener");
		} else {
			node.removeAttribute("href");
		}
	}
});

function renderDescription(text: string): string {
	const html = marked.parse(text ?? "", { breaks: true }) as string;
	return DOMPurify.sanitize(html, { ADD_ATTR: ["target"] });
}

type Mod = {
	Screenshots: string[];
	PageURL: string;
	Name: string;
	Text: string;
	Files: {
		URL: string;
		Name: string;
	}[];
};

export const modsDir = () =>
	rootFolder
		.getDirectoryHandle("Celeste", { create: false })
		.then((celeste) => celeste.getDirectoryHandle("Mods", { create: false }));
const ensureModsDir = async (): Promise<FileSystemDirectoryHandle> => {
	try {
		return await modsDir();
	} catch {
		const celeste = await rootFolder.getDirectoryHandle("Celeste", {
			create: true,
		});
		return celeste.getDirectoryHandle("Mods", { create: true });
	}
};

export async function downloadWithProgress(
	resp: Response,
	writable: FileSystemWritableFileStream,
	onProgress?: (value: number) => void
) {
	const total = Number(resp.headers.get("content-length")) || 0;
	let loaded = 0;
	const reader = resp.body!.getReader();
	onProgress?.(0);
	try {
		for (;;) {
			const { done, value } = await reader.read();
			if (done) break;
			await writable.write(value);
			loaded += value.byteLength;
			onProgress?.(total > 0 ? Math.min(loaded / total, 1) : -1);
		}
		await writable.close();
	} catch (err) {
		try {
			await writable.abort();
		} catch {}
		throw err;
	} finally {
		reader.releaseLock();
	}
}

export async function downloadModFile(
	url: string,
	filename: string,
	onProgress?: (value: number) => void
): Promise<void> {
	// The wisp-backed fetch can transiently return an empty body; retry a
	// few times before surfacing the failure.
	const mods = await ensureModsDir();
	let lastErr: unknown = null;
	for (let attempt = 1; attempt <= 3; attempt++) {
		try {
			await downloadModFileOnce(mods, url, filename, onProgress);
			return;
		} catch (err) {
			lastErr = err;
			try {
				await mods.removeEntry(`${filename}.partial`);
			} catch {}
			if (attempt < 3) {
				console.debug(`Mod download attempt ${attempt} failed, retrying...`, err);
				await new Promise((r) => setTimeout(r, 1000 * attempt));
			}
		}
	}
	throw lastErr instanceof Error ? lastErr : new Error("Mod download failed");
}

async function downloadModFileOnce(
	mods: FileSystemDirectoryHandle,
	url: string,
	filename: string,
	onProgress?: (value: number) => void
): Promise<void> {

		let resp = await epoxyFetch(url);
		if (!resp.ok || !resp.body) {
			throw new Error(`Mod download failed: HTTP ${resp.status}`);
		}
		const staged = await mods.getFileHandle(`${filename}.partial`, {
			create: true,
		});
		try {
			await downloadWithProgress(resp, await staged.createWritable(), onProgress);
		} catch (err) {
			try {
				await mods.removeEntry(`${filename}.partial`);
			} catch {}
			throw err instanceof Error
				? err
				: new Error("Mod download failed before it completed");
		}
		const stagedFile = await staged.getFile();
		if (stagedFile.size === 0) {
			await mods.removeEntry(`${filename}.partial`);
			throw new Error("Mod download produced an empty file");
		}
		try {
			await mods.removeEntry(filename);
		} catch {}
		const finalFile = await mods.getFileHandle(filename, { create: true });
		const finalWritable = await finalFile.createWritable();
		try {
			await stagedFile.stream().pipeTo(finalWritable);
		} catch (err) {
			try {
				await mods.removeEntry(filename);
			} catch {}
			try {
				await mods.removeEntry(`${filename}.partial`);
			} catch {}
			throw err instanceof Error
				? err
				: new Error("Mod download failed before it completed");
		}
		await mods.removeEntry(`${filename}.partial`);

		console.log("Downloaded mod");
}

export const ModInstaller: Component<
	{
		open: boolean;
	},
	{
		entries: Mod[];
		query: string;
		downloadProgress: Record<string, number>;
		installed: { names: string[]; files: string[] };
		expanded: Record<string, boolean>;
		lightbox: string | null;
	}
> = function () {
	// https://maddie480.ovh/celeste/gamebanana-categories
	// https://maddie480.ovh/celeste/gamebanana-subcategories?itemtype=...&categoryId=...

	this.query = "";
	this.entries = [];
	this.downloadProgress = {};
	this.installed = { names: [], files: [] };
	this.expanded = {};
	this.lightbox = null;
	this.css = `
		height: 100%;
		position: relative;

		.mods {
			overflow: auto;

			img {
				width: auto;
				height: 5rem;
				border: 0px solid transparent!important;
			}
		}

		.mod {
			display: flex;
			flex-direction: row;
			align-items: start;
			position: relative;
			margin-bottom: 1rem;
			border-radius: 14px;
			overflow: hidden;
			box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
			transition: transform 0.2s ease, box-shadow 0.2s ease;
			min-height: 12rem;
			width: calc(100% - 10px);
			margin-left: 5px;
		}

		.mod:hover {
			transform: scale(1.01);
			box-shadow: 0 6px 12px rgba(0, 0, 0, 0.25);
		}

		.bg {
			z-index: 9000;
			position: absolute;
			top: 0;
			left: 0;
			width: 100%!important;
			height: 100%!important;
			border: 0;
			object-fit: cover;
		}

		.gradient-overlay {
			content: "";
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			background: linear-gradient(to bottom, rgba(0,0,0,0.6), rgba(0,0,0,0.9));
			z-index: 9001;
		}

		.mod,
		.detail {
			z-index: 9002;
		}

		.detail {
			padding: 1.25rem;
			width: 100%;

			h2 {
				margin-top: 0;
				color: #fff;
				text-shadow: 0 1px 3px rgba(0,0,0,0.5);
				font-size: 1.4rem;
				
				padding-right: 3.5rem;
			}
		}
		.descblock {
			padding-right: 3.5rem;
		}

		.moddesc {
			overflow: hidden;
			display: -webkit-box;
			-webkit-line-clamp: 5;
			-webkit-box-orient: vertical;
			color: rgba(255, 255, 255, 0.9);
			text-shadow: 0 1px 2px rgba(0,0,0,0.3);
			font-size: 0.9rem;
			line-height: 1.4;
			margin-bottom: 0.5rem;
			overflow-wrap: break-word;
		}
		.moddesc.expanded {
			display: block;
		}
		.moddesc p {
			margin: 0 0 0.5em;
		}
		.moddesc h1,
		.moddesc h2,
		.moddesc h3 {
			color: #fff;
			margin: 0.5em 0 0.25em;
			font-size: 1.05rem;
		}
		.moddesc a {
			color: var(--accent);
		}
		.moddesc ul,
		.moddesc ol {
			margin: 0 0 0.5em;
			padding-inline-start: 1.25rem;
		}
		.moddesc hr {
			border: none;
			border-top: 1px solid rgba(255, 255, 255, 0.25);
			margin: 0.5em 0;
		}
		.moddesc img {
			max-width: 100%;
		}

		.desctoggle {
			background: none;
			border: none;
			padding: 0;
			margin: 0 0 0.5rem;
			color: var(--accent);
			font-size: 0.85rem;
			font-weight: 600;
			cursor: pointer;
		}

		.screenshots img {
			cursor: zoom-in;
		}
		.mod-lightbox {
			position: fixed;
			inset: 0;
			width: 100vw;
			max-width: 100vw;
			height: 100dvh;
			max-height: 100dvh;
			margin: 0;
			padding: 1rem;
			border: none;
			background: rgba(0, 0, 0, 0.95);
			cursor: zoom-out;
		}
		.mod-lightbox::backdrop {
			background: rgba(0, 0, 0, 0.95);
		}
		.mod-lightbox .lbwrap {
			display: flex;
			align-items: center;
			justify-content: center;
			width: 100%;
			height: 100%;
		}
		.mod-lightbox img {
			max-width: calc(100vw - 2rem);
			max-height: calc(100dvh - 2rem);
			object-fit: contain;
			border-radius: 8px;
		}

		.screenshots {
			display: flex;
			flex-direction: row;
			overflow-x: auto;
			white-space: nowrap;
			gap: 0.5rem;
			padding: 0.5rem 0;
			width: 100%;
			scrollbar-width: thin;
			-webkit-overflow-scrolling: touch;
			margin-top: 0.5rem;

			img {
				flex: 0 0 auto;
				height: 5rem;
				border-radius: 4px;
				box-shadow: 0 2px 4px rgba(0,0,0,0.3);
				transition: transform 0.2s ease;
			}

			img:hover {
				transform: scale(1.05);
			}
		}

		.moddownload {
			position: absolute;
			right: 1rem;
			top: 1rem;
			margin: 0;
			z-index: 9003;
			border-radius: 50%;
			width: 42px;
			height: 42px;
			display: flex;
			align-items: center;
			justify-content: center;
			box-shadow: 0 2px 6px rgba(0,0,0,0.3);
			transition: transform 0.2s ease;
		}

		.moddownload:hover {
			transform: scale(1.1);
		}
		.moddownload .dlring {
			position: absolute;
			inset: 0;
			border-radius: 50%;
			background: conic-gradient(#fff calc(var(--p, 0) * 1%), rgba(255, 255, 255, 0.25) 0);
			-webkit-mask: radial-gradient(closest-side, transparent 79%, #000 80%);
			mask: radial-gradient(closest-side, transparent 79%, #000 80%);
		}
		.moddownload .dlpct {
			font-size: 0.65rem;
			font-weight: 700;
			color: #fff;
		}
		.moddownload .dlspin {
			width: 20px;
			height: 20px;
			border-radius: 50%;
			border: 2px solid rgba(255, 255, 255, 0.3);
			border-top-color: #fff;
			animation: dlspin 0.8s linear infinite;
		}
		@keyframes dlspin {
			to { transform: rotate(360deg); }
		}
		@media (prefers-reduced-motion: reduce) {
			.moddownload .dlspin { animation: none; }
		}

		#modsearch {
			display: flex;
			gap: 0.5rem;
			padding: 0.25rem 0.5rem 1rem;
			position: sticky;
			top: 0;
			z-index: 10000;
			background: radial-gradient(ellipse at bottom, color-mix(in srgb, var(--bg-sub) 85%, transparent), var(--bg-sub));
			backdrop-filter: blur(5px);
		}

		.modsearchbar {
			flex-grow: 1;
		}

		.empty-message {
			text-align: center;
			padding: 2rem;
			color: rgba(255,255,255,0.7);
			font-style: italic;
		}
	`;

	const loadFrom = async (url: string) => {
		await loadedLibcurlPromise;
		let res = await epoxyFetch(url);
		this.entries = [];
		if (!res.ok) {
			console.error(`Mod catalog request failed: HTTP ${res.status}`);
			return;
		}

		let entries: Mod[] = await res.json();
		if (!Array.isArray(entries)) {
			console.error("Mod catalog returned an invalid response");
			return;
		}
		this.entries = entries
			.filter((entry) => entry && typeof entry.Name === "string")
			.map((e) => $state(e));
	};

	useChange([this.open, this.entries], async () => {
		if (this.open) {
			await loadedLibcurlPromise;
			for (const e of this.entries) {
				for (let i = 0; i < e.Screenshots.length; i++) {
					const url = e.Screenshots[i];
					if (url.startsWith("blob:")) continue;
					e.Screenshots[i] = "";
					epoxyFetch(url)
						.then((b) => b.blob())
						.then((blob) => {
							const blobUrl = URL.createObjectURL(blob);
							e.Screenshots[i] = blobUrl;
							e.Screenshots = e.Screenshots;
						});
				}
			}
		}
	});

	const search = async () => {
		console.log(this.query);
		await loadFrom(
			"https://maddie480.ovh/celeste/gamebanana-search?q=" + this.query
		);
		this.query = "";
	};

	const download = async (mod: Mod) => {
		const file = mod.Files?.[0];
		const key = file?.Name ?? "";
		try {
			if (!file?.URL || !file.Name) {
				alert("This mod entry does not provide a downloadable file");
				return;
			}
			if (!file.URL.startsWith("https://")) {
				alert("Refusing to download this mod from a non-HTTPS URL");
				return;
			}
			const mods = await ensureModsDir();
			try {
				await mods.getFileHandle(file.Name, { create: false });
				alert("Mod already installed");
				return;
			} catch {}

			await downloadModFile(file.URL, file.Name, (value) => {
				this.downloadProgress = {
					...this.downloadProgress,
					[file.Name]: value,
				};
			});
		} catch (err) {
			console.error("Mod download failed", err);
			alert(err instanceof Error ? err.message : "Mod download failed");
		} finally {
			if (key) {
				const rest = { ...this.downloadProgress };
				delete rest[key];
				this.downloadProgress = rest;
			}
			await refreshInstalled();
		}
	};

	const refreshInstalled = async () => {
		const names = new Set<string>();
		const files = new Set<string>();
		try {
			const { getInstalledMods } = await import("./game/dotnet");
			for (const m of await getInstalledMods()) {
				if (m.name) names.add(m.name.toLowerCase());
				if (m.file) files.add(m.file.toLowerCase());
			}
		} catch {}
		try {
			const mods = await modsDir();
			for await (const [name] of mods) files.add(name.toLowerCase());
		} catch {}
		this.installed = { names: [...names], files: [...files] };

		this.downloadProgress = { ...this.downloadProgress };
	};

	this.mount = async () => {
		loadFrom("https://maddie480.ovh/celeste/gamebanana-featured");
		await refreshInstalled();
	};

	useChange([this.open], () => {
		if (this.open) void refreshInstalled();
	});

	useChange([this.lightbox], () => {
		if (!this.root) return;
		const dlg = this.root.querySelector(
			".mod-lightbox"
		) as HTMLDialogElement | null;
		if (!dlg) return;
		if (this.lightbox) {
			const img = dlg.querySelector("img");
			if (img) img.src = this.lightbox;
			if (!dlg.open) dlg.showModal();
		} else if (dlg.open) {
			dlg.close();
		}
	});

	const isInstalled = (mod: Mod): boolean => {
		if (mod.Name && this.installed.names.includes(mod.Name.toLowerCase()))
			return true;
		const file = mod.Files?.[0]?.Name;
		return !!file && this.installed.files.includes(file.toLowerCase());
	};

	return (
		<div>
			<div id="modsearch">
				<TextField
					placeholder={"Search mods..."}
					on:keydown={(e: any) => {
						this.query = e.target.value;
						e.key === "Enter" && search();
					}}
					bind:value={use(this.query)}
					class={"modsearchbar"}
				/>
				<Button
					on:click={search}
					class={"searchbtn"}
					type={"primary"}
					icon={"full"}
					disabled={false}
				>
					<Icon icon={iconSearch} />
				</Button>
			</div>
			<div class="mods">
				{$if(
					use(this.entries, (entries) => entries.length === 0),
					<div class="empty-message">
						No mods found! Try searching for something else
					</div>
				)}
				{use(this.entries, (e) =>
					e.map((e) => (
						<div class="mod">
							<img
								class="bg"
								src={use(e.Screenshots, (s) =>
									s[0].startsWith("blob:") ? s[0] : ""
								)}
							/>
							<div class="gradient-overlay"></div>
							<div class="detail">
								<h2>{e.Name}</h2>
								{(() => {
									const desc = (<div />) as HTMLDivElement;
									desc.innerHTML = renderDescription(e.Text);
									return (
										<div class="descblock">
											<div
												class={use(this.expanded, (x) =>
													x[e.Name] ? "moddesc expanded" : "moddesc"
												)}
											>
												{desc}
											</div>
											<button
												class="desctoggle"
												on:click={() => {
													this.expanded = {
														...this.expanded,
														[e.Name]: !this.expanded[e.Name],
													};
												}}
											>
												{use(this.expanded, (x) =>
													x[e.Name] ? "Show less" : "Show more"
												)}
											</button>
										</div>
									);
								})()}
								<div class="screenshots">
									{use(e.Screenshots, (e) =>
										e
											.slice(1)
											.filter((x) => x.startsWith("blob:"))
											.map((s) => (
												<img
													src={s}
													title={"View larger"}
													on:click={() => (this.lightbox = s)}
												/>
											))
									)}
								</div>
							</div>
							<Button
								on:click={() => {
									const key = e.Files?.[0]?.Name;
									if (
										key &&
										this.downloadProgress[key] === undefined &&
										!isInstalled(e)
									) {
										download(e);
									}
								}}
								icon="full"
								type="primary"
								class="moddownload"
								disabled={false}
								title={use(this.installed, () =>
									isInstalled(e) ? "Installed" : "Download Mod"
								)}
							>
								{use(this.downloadProgress, (progress) => {
									if (isInstalled(e)) return <Icon icon={iconCheck} />;
									const key = e.Files?.[0]?.Name;
									const value = key ? progress[key] : undefined;
									if (value === undefined) return <Icon icon={iconDownload} />;
									if (value < 0) return <span class="dlspin" />;
									const pct = Math.round(100 * Math.min(value, 1));
									return (
										<span style="position:relative;display:flex;align-items:center;justify-content:center;width:100%;height:100%;">
											<span class="dlring" style={`--p: ${pct}`} />
											<span class="dlpct">{pct}</span>
										</span>
									);
								})}
							</Button>
						</div>
					))
				)}
			</div>
			<dialog
				class="mod-lightbox"
				on:click={(e: any) => {
					if (e.target === e.currentTarget) this.lightbox = null;
				}}
				on:cancel={() => (this.lightbox = null)}
			>
				<div class="lbwrap">
					<img />
				</div>
			</dialog>
		</div>
	);
};
