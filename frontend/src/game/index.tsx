import { gameState, loglisteners, preInit } from "./dotnet";
import { Loader } from "./loading";

export const LogView: Component<{ scrolling: boolean }> = function () {
	this.css = `
		min-height: 0;
		flex: 1;
		font-family: var(--font-mono);

		.log {
			word-break: break-all;
		}

		::-webkit-scrollbar {
			width: 10px;
		}
		::-webkit-scrollbar-track {
			background: var(--surface3);
		}
		::-webkit-scrollbar-thumb {
			background: var(--surface6);
		}
	`;

	const create = (color: string, log: string) => {
		const el = document.createElement("div");
		el.classList.add("log");
		el.innerText = log;
		el.style.color = color;
		return el;
	};

	this.mount = () => {
		const logroot = this.root as HTMLElement;
		const frag = document.createDocumentFragment();

		loglisteners.push((x) => frag.append(create(x.color, x.log)));
		setInterval(() => {
			if (frag.children.length > 0) {
				logroot.appendChild(frag);
				// A single load emits hundreds of log lines (MonoMod relink spam,
				// WebGL warnings, jiterpreter stats); appending them all keeps DOM
				// work on the same main thread as the game. Drop old nodes.
				while (logroot.children.length > 500) {
					logroot.firstChild?.remove();
				}
				logroot.scrollTop = logroot.scrollHeight;
			}
		}, 250);
	};

	return (
		<div
			class="component-log"
			style={this.scrolling ? "overflow: auto" : "overflow: hidden"}
		/>
	);
};

export const GameView: Component<
	{ canvas: HTMLCanvasElement },
	{},
	{ start: () => Promise<void> }
> = function () {
	this.css = `
		aspect-ratio: 16 / 9;
		user-select: none;
		position: relative;

		transition: background 150ms, color 150ms;

		.gameoverlay, canvas {
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			transition: background 150ms, color 150ms, border-color 150ms;
			border-bottom: 1.75px solid var(--surface2);
		}

		div.started, canvas.stopped {
			visibility: hidden;
			z-index: 0;
		}

		canvas {
			z-index: 1;
		}

		.gameoverlay.notrunning {
			background: var(--surface0);
			color: var(--surface5);
			transition: background 150ms, color 150ms, border-color 150ms;
			font-family: var(--font-display);
			font-size: 2rem;
			font-weight: 570;

			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;

			z-index: 1;
		}

		.gameoverlay.loader {
			opacity: 0;
			visibility: hidden;
			display: none;
			transition: opacity 0.5s ease;
			pointer-events: none;
			z-index: 3;
		}

		.gameoverlay.loader.active {
			opacity: 1;
			visibility: visible;
			display: flex;
			transition: opacity 0.5s ease;
			pointer-events: auto;
		}

		.gameoverlay.notrunning.stopped {
			pointer-events: none;
			visibility: hidden;
		}

		canvas:fullscreen {
			border: none;
			border-radius: 0;
			background: black;
			width: min(100vw, calc(100vh * 16 / 9)) !important;
			height: min(100vh, calc(100vw * 9 / 16)) !important;
			left: max(0px, calc((100vw - min(100vw, calc(100vh * 16 / 9))) / 2)) !important;
			top: max(0px, calc((100vh - min(100vh, calc(100vw * 9 / 16))) / 2)) !important;
			object-fit: fill;
		}

		#fps {
			position: absolute;
			top: 0;
			right: 0;
			z-index: 2;
		}
	`;
	const notRunning = use(gameState.playing, (x) =>
		x ? "gameoverlay notrunning started" : "gameoverlay notrunning stopped"
	);
	const loader = use(gameState.initting, (x) =>
		x ? "gameoverlay loader active" : "gameoverlay loader"
	);
	const canvas = use(gameState.playing, (x) =>
		x ? "canvas started" : "canvas stopped"
	);

	this.start = async () => {
		preInit().catch((err) =>
			console.error("Runtime initialization failed", err)
		);
	};

	return (
		<div>
			<div class={loader}>
				<Loader />
			</div>
			<div class={notRunning}>Game not running.</div>
			<canvas
				id="canvas"
				class={canvas}
				bind:this={use(this.canvas)}
				on:contextmenu={(e: Event) => e.preventDefault()}
			/>
		</div>
	);
};

export { gameState, play, loadedLibcurlPromise } from "./dotnet";
