import { gameState, play } from "./game";
import { Button, Icon } from "./ui";
import iconPlayArrow from "@ktibow/iconset-material-symbols/play-arrow";

export const Logo: Component<{}, {}> = function() {
	this.css = `
		display: flex;
		align-items: center;
		font-size: 1.5rem;

		font-family: Renogare;

		img {
			width: 3rem;
			height: 3rem;
		}
	`;
	return (
		<div>
			<img src="/app.ico" />
			celeste-wasm
		</div>
	)
}

const TopBar: Component<{}, { allowPlay: boolean, }> = function() {
	this.css = `
		background: var(--bg-sub);
		display: flex;
		align-items: center;
		gap: 1em;
		padding: 1em;
		height: 3rem;

		.expand { flex: 1; }
	`;

	useChange([gameState.ready, gameState.playing], () => {
		this.allowPlay = gameState.ready && !gameState.playing;
	});

	return (
		<div>
			<Logo />
			<div class="expand" />
			<Button on:click={() => {
				play();
			}} icon="left" type="primary" disabled={use(this.allowPlay, x => !x)}>
				<Icon icon={iconPlayArrow} />
				Play
			</Button>
		</div>
	)
}

const GameView: Component<{}, {}> = function() {
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
	`;
	const playing = use(gameState.playing, x => x ? "started" : "stopped");
	return (
		<div>
			<div class={playing}>
				Game not running.
			</div>
			<canvas id="canvas" class={playing} />
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

export const Main: Component<{}, {}> = function() {
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
			<TopBar />
			<div class="main">
				<div class="game">
					<GameView />
				</div>
				<h2>Log</h2>
				<LogView />
			</div>
		</div>
	);
}
