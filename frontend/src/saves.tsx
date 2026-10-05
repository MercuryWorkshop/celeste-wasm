import { Button, Icon } from "./ui/Button";
import {
	PICKERS_UNAVAILABLE,
	TAR_TYPES,
	createTar,
	extractTar,
	pipeToWritable,
	rootFolder,
} from "./fs";

import iconUploadFile from "@ktibow/iconset-material-symbols/upload-file";
import iconDownload from "@ktibow/iconset-material-symbols/download";

async function getSavesDir(
	create: boolean
): Promise<FileSystemDirectoryHandle> {
	const celeste = await rootFolder.getDirectoryHandle("Celeste", { create });
	return await celeste.getDirectoryHandle("Saves", { create });
}

export const SavesButtons: Component<{}, { working: boolean; status: string }> =
	function () {
		this.working = false;
		this.status = "";
		this.css = `
		display: flex;
		flex-direction: column;
		gap: 0.5rem;

		.row {
			display: flex;
			gap: 0.5rem;
		}

		.row > * {
			flex: 1;
		}

		.status {
			color: var(--fg6);
			font-size: 0.85rem;
		}

		.hint {
			font-size: 0.85rem;
			opacity: 0.8;
		}
	`;

		const exportSave = async () => {
			if (this.working) return;
			this.working = true;
			this.status = "";
			try {
				let saves: FileSystemDirectoryHandle;
				try {
					saves = await getSavesDir(false);
				} catch {
					this.status = "No saves found yet. Play the game first.";
					return;
				}
				let count = 0;
				const tar = createTar(saves, (type) => {
					if (type === "file") count++;
				});
				const file = await showSaveFilePicker({
					excludeAcceptAllOption: true,
					suggestedName: "webleste-saves.tar",
					types: TAR_TYPES,
				});
				const stream = file.name.endsWith(".gz")
					? tar.pipeThrough(new CompressionStream("gzip"))
					: tar;
				const writable = await file.createWritable();
				await pipeToWritable(stream, writable);
				this.status =
					count === 1
						? `Exported 1 file to ${file.name}.`
						: `Exported ${count} files to ${file.name}.`;
			} catch (err) {
				if (err instanceof DOMException && err.name === "AbortError") return;
				console.error("Save export failed", err);
				this.status =
					err instanceof Error
						? `Export failed: ${err.message}`
						: "Export failed.";
			} finally {
				this.working = false;
			}
		};

		const importSave = async () => {
			if (this.working) return;
			this.working = true;
			this.status = "";
			try {
				const [picker] = await showOpenFilePicker({ multiple: false });
				if (!/\.tar(\.gz)?$/i.test(picker.name)) {
					this.status = "Pick a .tar or .tar.gz save backup.";
					return;
				}
				let stream = await picker.getFile().then((f) => f.stream());
				if (picker.name.endsWith(".gz")) {
					stream = stream.pipeThrough(new DecompressionStream("gzip"));
				}
				const saves = await getSavesDir(true);
				let count = 0;
				await extractTar(stream, saves, (type) => {
					if (type === "file") count++;
				});
				this.status =
					count === 1
						? "Imported 1 file. Same-named saves were overwritten."
						: `Imported ${count} files. Same-named saves were overwritten.`;
			} catch (err) {
				if (err instanceof DOMException && err.name === "AbortError") return;
				console.error("Save import failed", err);
				this.status =
					err instanceof Error
						? `Import failed: ${err.message}`
						: "Import failed.";
			} finally {
				this.working = false;
			}
		};

		const pickerDisabled = use(this.working, (x) => x || !!PICKERS_UNAVAILABLE);

		return (
			<div class="SavesButtons">
				{PICKERS_UNAVAILABLE ? (
					<div class="hint">
						Your browser does not support file pickers, so saves cannot be
						imported or exported here. Use a Chromium-based browser.
					</div>
				) : null}
				<div class="row">
					<Button
						on:click={exportSave}
						type="primary"
						icon="left"
						disabled={pickerDisabled}
						title={"Export saves to a .tar file"}
					>
						<Icon icon={iconDownload} />
						Export saves
					</Button>
					<Button
						on:click={importSave}
						type="primary"
						icon="left"
						disabled={pickerDisabled}
						title={"Import saves from a .tar file"}
					>
						<Icon icon={iconUploadFile} />
						Import saves
					</Button>
				</div>
				{$if(use(this.status), <div class="status">{use(this.status)}</div>)}
			</div>
		);
	};
