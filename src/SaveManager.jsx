import { downloadsave, uploadsave, unzipsave } from "./game.js";
import { app } from "./main.jsx";

export function SaveManager() {
	this.css = `
		flex-direction: column;
		justify-content: center;
		align-items: center;
		gap: 0.75rem;
		#dropzone {
			display: flex;

			flex-direction: column;
			align-items: center;
			justify-content: center;
			width: calc(min(100%, 40rem) - 6rem);
			height: auto;
			aspect-ratio: 1 / 1;
			border: 2px dashed var(--fg6);
			border-radius: 0.6rem;
			color: var(--fg6);
			transition: 0.3s;

			&.dragover {
				border-color: var(--accent);
				background-color: color-mix(in srgb, var(--accent) 5%, color-mix(in srgb, var(--surface2) 40%, transparent));
				transition: 0.3s;
			}

			h1 {
				margin: 0;
			}
		}

		#dl {
			width: calc(min(100%, 40rem) - 6rem);
		}
	`

	return (
		<div class="flex">
			<div class="gap" bind:this={use(this.zone)}
				on:dragover={(ev) => {
					ev.preventDefault();
				}}
				on:dragenter={() => {
					this.zone.classList.add("dragover");
				}}
				on:dragleave={() => {
					this.zone.classList.remove("dragover");
				}}
				on:drop={async (ev) => {
					ev.preventDefault();
					let files = ev.dataTransfer.items ? ev.dataTransfer.items : ev.dataTransfer.files;
					let file;

					if (files > 1) {
						alert("Please only drop one file at a time.");
						return;
					} else {
						file = files[0].getAsFile();
					}

					document.getElementById("dropzone").classList.remove("dragover");

					unzipsave(file);
				}}
			>
				<div class="material-symbols-rounded" style="font-size: 5rem;">cloud_upload</div>
				<h1>Drag and drop file</h1>

				<div>
					<button id="upload" on:click={async () => {
						await uploadsave();
						app.savesmenu.close();
					}}>
						<span class="material-symbols-rounded">upload</span>
						<span class="label">Upload from computer</span>
					</button>
				</div>
			</div>

			<button id="dl" on:click={async () => {
				await downloadsave();
				app.savesmenu.close();
			}}>
				<span class="material-symbols-rounded">cloud_download</span>
				<span class="label">Download current save</span>
			</button>
		</div>
	)
}
