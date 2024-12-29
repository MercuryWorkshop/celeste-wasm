import { gameState, play } from "./game";
import { Button, Dialog, Icon, Link } from "./ui";
import { store } from "./store";
import { OpfsExplorer } from "./fs";

import iconPlayArrow from "@ktibow/iconset-material-symbols/play-arrow";
import iconFullscreen from "@ktibow/iconset-material-symbols/fullscreen";
import iconLightMode from "@ktibow/iconset-material-symbols/light-mode";
import iconDarkMode from "@ktibow/iconset-material-symbols/dark-mode";
import iconFolderOpen from "@ktibow/iconset-material-symbols/folder-open";

export const Logo: Component<{}, {}> = function() {
	this.css = `
		display: flex;
		align-items: center;
		font-size: 1.5rem;

		font-family: Renogare;

		img {
			image-rendering: pixelated;
			-ms-interpolation-mode: nearest-neighbor;
			width: 3rem;
			height: 3rem;
		}


		.superscript {
			align-self: start;
			padding-top: 0.25rem;

			font-size: 1rem;
			color: var(--fg6);
		}
	`;
	return (
		<div>
			<img src="/app.ico" />
			<span>celeste-wasm</span>
			<subt class="superscript">v1.4.0.0</subt>
		</div>
	)
}

const TopBar: Component<{
	canvas: HTMLCanvasElement,
	fsOpen: boolean,
}, { allowPlay: boolean, }> = function() {
	this.css = `
		background: var(--bg-sub);
		display: flex;
		align-items: center;
		gap: 1em;
		padding: 1em;
		height: 3rem;
		border-bottom: 2px solid var(--surface1);

		.expand { flex: 1; }
	`;

	useChange([gameState.ready, gameState.playing], () => {
		this.allowPlay = gameState.ready && !gameState.playing;
	});

	return (
		<div>
			<Logo />
			<div class="expand" />
			<Button on:click={() => this.fsOpen = true} icon="full" type="normal" disabled={false}>
				<Icon icon={iconFolderOpen} />
			</Button>
			<Button on:click={() => {
				if (store.theme === "light") {
					store.theme = "dark";
				} else {
					store.theme = "light";
				}
			}} icon="full" type="normal" disabled={false}>
				<Icon icon={use(store.theme, x => x === "light" ? iconDarkMode : iconLightMode)} />
			</Button>
			<Button on:click={async () => {
				try {
					await this.canvas.requestFullscreen({ navigationUI: "hide" });
				} catch { }
			}} icon="full" type="normal" disabled={use(gameState.playing, x => !x)}>
				<Icon icon={iconFullscreen} />
			</Button>
			<Button on:click={() => {
				play();
			}} icon="left" type="primary" disabled={use(this.allowPlay, x => !x)}>
				<Icon icon={iconPlayArrow} />
				Play
			</Button>
		</div>
	)
}

const BottomBar: Component<{}, {}> = function() {
	this.css = `
		background: var(--bg-sub);
		border-top: 2px solid var(--surface1);
		padding: 0.5rem;
		font-size: 0.8rem;

		display: flex;
		align-items: center;
		justify-content: space-between;
	`;

	return (
		<div>
			<span>Ported by <Link href="https://github.com/r58playz">r58Playz</Link></span>
			<span>All game assets and code belong to <Link href="https://exok.com/">Extremely OK Games, Ltd.</Link> All rights reserved.</span>
			<span>Check out the project on <Link href="https://github.com/r58playz/celeste-wasm-threads">GitHub!</Link></span>
		</div>
	)
}

const GameView: Component<{ canvas: HTMLCanvasElement }, {}> = function() {
	this.css = `
		position: relative;
		aspect-ratio: 16 / 9;
		user-select: none;

		div, canvas {
			position: absolute;
			left: 0;
			top: 0;
			width: 100%;
			height: 100%;

			border: 2px solid var(--surface6);
			border-radius: 1rem;
		}
		div.started, canvas.stopped {
			display: none;
		}

		div {
			background: var(--surface1);
			color: var(--fg6);

			font-family: var(--font-display);
			font-size: 2rem;
			font-weight: 570;
			
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
		}

		canvas:fullscreen {
			border: none;
			border-radius: 0;
			background: black;
		}
	`;
	const playing = use(gameState.playing, x => x ? "started" : "stopped");

	return (
		<div>
			<div class={playing}>
				Game not running.
			</div>
			<canvas id="canvas" class={playing} bind:this={use(this.canvas)} />
		</div>
	)
}

const LogView: Component<{}, {}> = function() {
	this.css = `
		min-height: 16rem;
		max-height: 32rem;
		overflow: scroll;
		padding: 1em;

		border: 2px solid var(--surface6);
		border-radius: 1rem;
		background: var(--bg-sub);

		font-family: var(--font-mono);
	`;

	const create = (color: string, log: string) => {
		const el = document.createElement("div");
		el.innerText = log;
		el.style.color = color;
		return el;
	}

	this.mount = () => {
		setInterval(() => {
			for (const log of gameState.logbuf) {
				this.root.appendChild(create(log.color, log.log));
			}
			this.root.scrollTop = this.root.scrollHeight;
			gameState.logbuf = [];
		}, 1000);
	};

	return (
		<div>
		</div>
	)
}

export const Main: Component<{}, {
	canvas: HTMLCanvasElement,
	fsOpen: boolean
}> = function() {
	this.css = `
		width: 100%;
		height: 100%;
		background: var(--bg);
		color: var(--fg);

		display: flex;
		flex-direction: column;
		overflow: scroll;

		.main {
			flex: 1;
			display: flex;
			flex-direction: column;
			gap: 1rem;
			padding: 1rem 0;

			margin: auto;
			width: min(1300px, calc(100% - 2rem));
		}

		.main h2 {
			margin: 0;
		}
	`;

	return (
		<div>
			<TopBar canvas={use(this.canvas)} bind:fsOpen={use(this.fsOpen)} />
			<div class="main">
				<div class="game">
					<GameView bind:canvas={use(this.canvas)} />
				</div>
				<LogView />
			</div>
			<Dialog name="File System" bind:open={use(this.fsOpen)}>
				<OpfsExplorer />
			</Dialog>
			<BottomBar />
		</div>
	);
}
