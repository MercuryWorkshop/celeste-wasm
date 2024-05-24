import { downloadsave, uploadsave, unzipsave } from "./game.js";
import { app } from "./main.jsx";

export function SaveManager() {
	this.css = `
		flex-direction: column;
		justify-content: center;
		align-items: center;
		gap: 0.75rem;
		width: 100%;
		height: calc(100% - 3rem);
		#dragarea {
			display: flex;

			flex-direction: column;
			align-items: center;
			justify-content: center;
			width: 95%;
			height: auto;
			aspect-ratio: 1.25 / 1;
			border: 2px dashed var(--fg6);
			border-radius: 0.6rem;
			color: var(--fg6);
			transition: 0.3s ease;

			&.dragover {
			  color: var(--fg3);
				border-color: var(--accent);
				background-color: color-mix(in srgb, var(--accent) 5%, color-mix(in srgb, var(--surface2) 40%, transparent));
				transition: 0.3s ease;
				box-shadow: 0 0 28px 0 color-mix(in srgb, var(--accent) 20%, transparent);
			}

			h1 {
				margin: 0;
			}
		}

		& > button {
			width: 95%;
		}

		#head {
      font-size: 1.5rem;
			font-weight: 650;
			font-family: var(--font-display);
			margin-block: 0.5rem;
			align-self: flex-start;
      margin-left: 2.5%;
		}
	`

	return (
		<div class="flex">
		  <span id="head">Save Manager</span>
			<div class="gap" id="dragarea" bind:this={use(this.zone)}
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

					this.zone.classList.remove("dragover");


					await unzipsave(file);
					app.savesmenu.close();
				}}
			>
				<div class="material-symbols-rounded" style="font-size: 5rem;">cloud_upload</div>
				<h1>Drag and drop savefile</h1>

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
