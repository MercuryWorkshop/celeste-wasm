import { Logs, log } from "./Logs.jsx";
import { FSExplorer } from "./FsExplorer.jsx";
import { fps, version, init, start } from "./game.js";
import { store } from "./main.jsx";
import { SaveManager } from './SaveManager.jsx'

export function App() {
	this.css = `
		width: 100vw;
		height: 100vh;
		display: flex;
		flex-direction: column;
		align-items: center;
		margin: 0;
		background-color: var(--bg);
		color: var(--fg);

		overflow-x: hidden;

		.game {
			width: 100%;
			display: flex;
			align-items: center;
			justify-content: center;

			canvascontainer {
				display: grid;
				max-height: 85vh;
				max-width: 100%;
				aspect-ratio: 16 / 9;
				border: 0.7px solid var(--surface5);
				border-radius: 0.6rem;
				overflow: hidden;
				background-color: black;

				&:not(:has(canvas[width])) {
					min-width: min(100%, 960px);
					min-height: min(100%, 540px);
				}

				& > * {
					grid-area: 1 / 1;
				}

				& > div {
				  transition: 0.3s ease;
					&.hidden {
						opacity: 0;
						pointer-events: none;
						transition: 0.3s ease;
					}

					.material-symbols-rounded {
						font-size: 3em;
					}

					h3 {
						margin: 0.2rem;
						font-weight: 570;
					}

					background-color: var(--surface1);
					user-select: none;
					text-align: center;
					color: var(--fg6);
					font-size: 1.5em;
					font-weight: 550;

					z-index: 5;
					width: 100%;
					height: 100%;
					display: flex;
					justify-content: center;
					align-items: center;
				}
			}
			canvas {
				width: 100%;
				display: block;
			}
		}

		.pinned {
			position: fixed;
			top: 0;
			left: 0;
			width: 100vw;
			height: auto;
		}

		.logs {
			width: min(960px, 100%);
		}

		.top-bar {
			padding-bottom: 1em;
			border: none;
			background-color: var(--surface0);
			flex-direction: column;
			align-items: center;
			gap: 0.5em;
		}

		.expand { flex: 1 }

		.content {
			padding: 0 1em;
			display: flex;
			flex-direction: column;
			align-items: center;
			flex: 1;
		}

		dialog:-internal-dialog-in-top-layer::backdrop {
			background: color-mix(in srgb, black 35%, color-mix(in srgb, var(--bg), transparent 70%))!important;
		}

		dialog {
			background: var(--bg);
			color: var(--fg);
			border: 0.1em solid var(--surface3);
			border-radius: 0.6em;
			opacity: 1;
			scale: 1;
			max-height: 75vh;
			width: unset;
			aspect-ratio: 1 / 1;

			transition: opacity 0.25s, transform 0.25s;
			transition-timing-function: ease;

			&:not([open]) {
				display: block;
				pointer-events: none;
				transform: scale(0.8);
				opacity: 0;
				scale: 0.9;
			}

			button {
				float: right;
			}

			& > div {
				width: 100%;
			}
		}

		footer {
			padding: 0.5em;
			border-top: 0.1em solid var(--surface1);
			background-color: var(--bg-sub);
			font-size: 0.8em;
			position: relative;
			bottom: 0;
			flex-direction: column;
			align-items: center;
			text-align: center;
		}

		@media screen and (min-width: 950px) {
			footer {
				flex-direction: row !important;
			}
		}

		@media screen and (min-width: 700px) {
			.top-bar {
				flex-direction: row !important;
				gap: 1.5em !important;
				padding: 0 1.5em !important;
			}
		}
	`;

	this.loaded = false;
	this.started = false;
	this.allowPlay = false;

	let updatePlay = () => { this.allowPlay = this.loaded || !this.started };
	handle(use(this.loaded), updatePlay);
	handle(use(this.started), updatePlay);

	window.initPromise = (async () => {
		await init();
		this.loaded = true;
		log("var(--success)", "Loaded frontend!");
	})();

	this.fullscreen = false;

	document.addEventListener("fullscreenchange", () => {
		this.fullscreen = document.fullscreen;
	});

	setInterval(() => {
		this.fps = fps;
	}, 1000);

	const startgame = () => {
		this.started = true;

		start(this.canvas);
		this.allowPlay = false;
	};

	return (
		<main class={["gap-md", use(store.theme)]}>
			<div class="flex vcenter space-between top-bar">
				<span class="flex vcenter gap left">
					<Logo />
					{$if(
						use(this.started),
						(<p>FPS: {use(this.fps, Math.floor)}</p>),
					)}
				</span>
				<span class="expand" />
				<span class="flex right vcenter" style="gap: 0.85em;">
					<button on:click={() => {
						this.savesmenu.showModal();
					}} title="Manage save data">
						<span class="material-symbols-rounded">save</span>
					</button>

					<button on:click={() => {
						this.fs.showModal();
					}} title="File Manager">
						<span class="material-symbols-rounded">folder_open</span>
					</button>

					<button on:click={() => {
						if (store.theme === "light") {
							store.theme = "dark";
						} else {
							store.theme = "light";
						}
					}} title="Toggle Theme">
						<span class="material-symbols-rounded">{use(store.theme, (theme) => (theme === "light" ? "dark_mode" : "light_mode"))
						}</span>
					</button>

					<button
						on:click={() => {
							if (this.canvas.requestFullscreen()) {
								this.fullscreen = true;
							}
						}}
						title="Enter Fullscreen"
					>
						<span class="material-symbols-rounded">fullscreen</span>
					</button>

					<button
						class={[
							use(this.allowPlay, (allowed) => (!allowed ? "disabled" : "primary")),
						]}
						on:click={startgame}
						title="Start Game"
					>
						<span class="material-symbols-rounded">play_arrow</span>
					</button>
				</span>
			</div>
			<div class="content">
				{(navigator.userAgent.includes("Firefox") && (<FuckMozilla />)) || ""}

				<div class="game">
					<canvascontainer>
						<div class={[use(this.started, (f) => f && "hidden")]}>
							<div>
								<span class="material-symbols-rounded">videogame_asset_off</span>
								<br />
								<h3>Game not running.</h3>
							</div>
						</div>

						<canvas
							id="canvas"
							bind:this={use(this.canvas)}
							on:contextmenu={(e) => e.preventDefault()}
						></canvas>
					</canvascontainer>
				</div>

				<dialog bind:this={use(this.fs)} id="fs">
					<button on:click={() => this.fs.close()} class="plain">
						<span class="material-symbols-rounded">close</span>
					</button>

					<FSExplorer />
				</dialog>

				<dialog bind:this={use(this.savesmenu)}>
					<button on:click={() => this.savesmenu.close()} class="plain">
						<span class="material-symbols-rounded">close</span>
					</button>

					<SaveManager />
				</dialog>

				<div class="logs">
					<h2>Log</h2>

					<Logs />
				</div>
			</div>
			<footer class="flex space-between gap-sm">
				<span>
					Ported by <a href="https://mercurywork.shop" target="_blank">Mercury Workshop</a>
				</span>

				<span>
					All game assets and code belong to <a href="https://exok.com/" target="_blank">Extremely OK Games, Ltd.</a> All rights reserved.
				</span>

				<span>
					Check out the project on <a href="https://github.com/MercuryWorkshop/celeste-wasm">GitHub!</a>
				</span>
			</footer>
		</main>
	);
}

function FuckMozilla() {
	this.css = `
		width: min(960px, 100%);

		background-color: var(--accent);
		color: var(--fg);
		padding: 1em;
		padding-top: 0.5em;
		margin-bottom: 1em;
		border-radius: 0.6rem;


		h1 {
			display: flex;
			align-items: center;
			gap: 0.25em;
			span {
				font-size: 2.25rem;
			}
		}
	`;

	return (
		<div>
			<h1>
				<span class="material-symbols-rounded">warning</span>
				THIS MIGHT NOT WORK WELL ON FIREFOX!
			</h1>

			<p>
				Chromium's WASM implementation is generally better, and it was what was tested on the most.
				It will probably still work (might not!) on Firefox but you should really be using a Chromium-based browser for this.
			</p>

			<button on:click={() => this.root.remove()}>Dismiss</button>
		</div>
	)
}

export function Logo() {
	this.css = `
		.logo {
			image-rendering: pixelated;
			-ms-interpolation-mode: nearest-neighbor;
			width: 3.75rem;
			height: 3.75rem;
			margin: 0;
			padding: 0;
		}

		h1 {
			font-size: 2rem;
			display: flex;
		}

		h1 subt {
			font-size: 0.5em;
			margin-left: 0.25em;
			color: var(--fg6);
		}
	`

	return (
		<span class="flex vcenter">
			<img class="logo" src={document.querySelector("link[rel=icon]").href} />
			<h1>celeste-wasm<subt>v{version}</subt></h1>
		</span>
	)
}
