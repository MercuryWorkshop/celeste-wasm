import { gameState, play, GameView, LogView } from "./game/index";
import { Dialog } from "./ui/Dialog";
import { Button, Icon } from "./ui/Button";
import { store } from "./store";

import { OpfsExplorer } from "./fs";
import { Achievements } from "./achievements";
import { ModInstaller } from "./modinstaller";
import { UpdateDialog } from "./updates";
import { SteamCloud } from "./steam";
import { Settings } from "./settings";
import { PatchFlow } from "./patch";

import iconPlayArrow from "@ktibow/iconset-material-symbols/play-arrow";
import iconFullscreen from "@ktibow/iconset-material-symbols/fullscreen";
import iconFolderOpen from "@ktibow/iconset-material-symbols/folder-open";
import iconTrophy from "@ktibow/iconset-material-symbols/trophy";
import iconDownload from "@ktibow/iconset-material-symbols/download";
import iconSettings from "@ktibow/iconset-material-symbols/settings";
import iconArrowCircleDown from "@ktibow/iconset-material-symbols/arrow-circle-down";

// Dreamland's $store logs an info line on every autosave; the settings
// store mutates often, so this is pure console noise. Filter just
// dreamland's own prefix.
const _origInfo = console.info.bind(console);
console.info = (...args: unknown[]) => {
	if (typeof args[0] === "string" && args[0].startsWith("[dreamland.js]")) return;
	_origInfo(...args);
};

export const NAME = "webleste";

export const Logo: Component<{}, {}> = function () {
	this.css = `
		display: flex;
		align-items: center;
		font-size: 1.5rem;

		font-family: var(--font-display);
		color: var(--fg);

		&:hover {
		  color: var(--accent);
		}

		img {
			image-rendering: pixelated;
			-ms-interpolation-mode: nearest-neighbor;
			width: 3rem;
			height: 3rem;
		}

		.extras {
			align-self: start;
			padding: 0.25rem 0;
			font-size: 1rem;
			color: var(--fg6);

			display: flex;
			flex-direction: column;
			justify-content: space-between;
		}
	`;
	return (
		<a href="https://github.com/MercuryWorkshop/celeste-wasm" target="_blank">
			<img alt="Celeste icon" src="./app.webp" />
			<span>{NAME}</span>
			<div class="extras">
				<span class="ver">v1.4.0.0</span>
			</div>
		</a>
	);
};

const TopBar: Component<
	{
		canvas: HTMLCanvasElement;
		fsOpen: boolean;
		showLog: number;
		steamOpen: boolean;
		achievementsOpen: boolean;
		modInstallerOpen: boolean;
		updatesOpen: boolean;
		settingsOpen: boolean;
	},
	{
		allowPlay: boolean;
		fps: HTMLElement;
	}
> = function () {
	this.css = `
		background: var(--bg);
		padding: 1em;
		border-bottom: 1.75px solid var(--surface2);
		transition: background 150ms, color 150ms, border-color 150ms;
		transition-timing-function: ease;
		display: flex;
		align-items: stretch;
		width: 100%;
		gap: 0.5rem;

		flex: 0 0;

		.group {
			display: flex;
			align-items: center;
			gap: 1rem;
		}

		.updatecount {
			font-weight: 700;
			color: var(--accent);
		}

		.expand { flex: 1; }

		@media (max-width: 750px) {
			& {
				flex-direction: column;
			}
			.group {
				justify-content: space-evenly;
			}
		}
	`;

	useChange([gameState.playing], () => {
		this.allowPlay = !gameState.playing;
	});
	this.allowPlay = !gameState.playing;

	return (
		<div>
			<div class="group">
				<Logo />
			</div>
			<div class="expand" />
			<div class="group">
				<Button
					on:click={() => (this.updatesOpen = true)}
					icon="left"
					type="normal"
					disabled={false}
					title="Mod manager"
				>
					<Icon icon={iconArrowCircleDown} />
					{$if(
						use(gameState.updatesAvailable, (n) => n > 0),
						<span class="updatecount">
							{use(gameState.updatesAvailable, (n) => String(n))}
						</span>
					)}
				</Button>
				<Button
					on:click={() => (this.modInstallerOpen = true)}
					icon="left"
					type="normal"
					disabled={use(gameState.hasEverest, (t) => !t)}
					title={"Download Mods"}
				>
					<Icon icon={iconDownload} />
					<span>Mods</span>
				</Button>
				{/*
				<Button
					on:click={() => (this.steamOpen = true)}
					icon="left"
					type="normal"
					disabled={false}
					title={"Log in to Steam"}
				>
					<svg
						width="1em"
						height="1em"
						viewBox="0 0 256 259"
						version="1.1"
						xmlns="http://www.w3.org/2000/svg"
						xmlns:xlink="http://www.w3.org/1999/xlink"
						preserveAspectRatio="xMidYMid"
					>
						<g>
							<path
								d="M127.778579,0 C60.4203546,0 5.24030561,52.412282 0,119.013983 L68.7236558,147.68805 C74.5451924,143.665561 81.5845466,141.322185 89.1497766,141.322185 C89.8324924,141.322185 90.5059824,141.340637 91.1702465,141.377541 L121.735621,96.668877 L121.735621,96.0415165 C121.735621,69.1388208 143.425688,47.2457835 170.088511,47.2457835 C196.751333,47.2457835 218.441401,69.1388208 218.441401,96.0415165 C218.441401,122.944212 196.751333,144.846475 170.088511,144.846475 C169.719475,144.846475 169.359666,144.83725 168.99063,144.828024 L125.398299,176.205276 C125.425977,176.786507 125.444428,177.367738 125.444428,177.939743 C125.444428,198.144443 109.160732,214.575753 89.1497766,214.575753 C71.5836817,214.575753 56.8868387,201.917832 53.5655182,185.163615 L4.40997549,164.654462 C19.6326942,218.967277 69.0834655,258.786219 127.778579,258.786219 C198.596511,258.786219 256,200.847629 256,129.393109 C256,57.9293643 198.596511,0 127.778579,0 Z M80.3519677,196.332478 L64.6033732,189.763644 C67.389592,195.63131 72.2239585,200.539484 78.6359521,203.233444 C92.4932392,209.064206 108.472481,202.430791 114.247888,188.435116 C117.043333,181.663313 117.061785,174.190342 114.294018,167.400086 C111.526251,160.609831 106.295171,155.31417 99.5879487,152.491048 C92.9176301,149.695603 85.7767911,149.797088 79.5031858,152.186594 L95.777656,158.976849 C105.999942,163.276114 110.834309,175.122157 106.571948,185.436702 C102.318812,195.751247 90.574254,200.631743 80.3519677,196.332478 Z M202.30901,96.0424391 C202.30901,78.1165345 187.85204,63.5211763 170.092201,63.5211763 C152.323137,63.5211763 137.866167,78.1165345 137.866167,96.0424391 C137.866167,113.968344 152.323137,128.554476 170.092201,128.554476 C187.85204,128.554476 202.30901,113.968344 202.30901,96.0424391 Z M145.938821,95.9870838 C145.938821,82.4988323 156.779242,71.5661525 170.138331,71.5661525 C183.506646,71.5661525 194.347066,82.4988323 194.347066,95.9870838 C194.347066,109.475335 183.506646,120.408015 170.138331,120.408015 C156.779242,120.408015 145.938821,109.475335 145.938821,95.9870838 Z"
								fill="var(--fg)"
							></path>
						</g>
					</svg>
					<span>Sync</span>
				</Button>
				*/}
				<Button
					on:click={() => (this.achievementsOpen = true)}
					icon="full"
					type="normal"
					disabled={false}
					title={"Achievements"}
				>
					<Icon icon={iconTrophy} />
				</Button>
				<Button
					on:click={() => (this.fsOpen = true)}
					icon="full"
					type="normal"
					disabled={false}
					title={"File Browser"}
				>
					<Icon icon={iconFolderOpen} />
				</Button>
				<Button
					icon="full"
					type="normal"
					disabled={false}
					title="Settings"
					on:click={() => {
						this.settingsOpen = true;
					}}
				>
					<Icon icon={iconSettings} />
				</Button>
				<Button
					on:click={async () => {
						try {
							await this.canvas.requestFullscreen({ navigationUI: "hide" });
						} catch {}
					}}
					icon="full"
					type="normal"
					disabled={use(gameState.playing, (x) => !x)}
					title={"Fullscreen"}
				>
					<Icon icon={iconFullscreen} />
				</Button>
				<Button
					on:click={() => {
						play().catch((err) => console.error("Play failed", err));
					}}
					icon="left"
					type="primary"
					disabled={use(this.allowPlay, (x) => !x)}
					title={"Start Game"}
				>
					<Icon icon={iconPlayArrow} />
					<span>Play</span>
				</Button>
			</div>
		</div>
	);
};

export const Main: Component<
	{},
	{
		canvas: HTMLCanvasElement;
		fsOpen: boolean;
		achievementsOpen: boolean;
		modInstallerOpen: boolean;
		updatesOpen: boolean;
		steamOpen: boolean;
		settingsOpen: boolean;
		logcontainer: HTMLDivElement;

		dialogs?: HTMLDivElement;

		logsize: number;
	},
	{
		start: () => Promise<void>;
	}
> = function () {
	this.css = `
		width: 100%;
		height: 100%;
		background: var(--bg-sub);
		color: var(--fg);

		display: flex;
		flex-direction: column;
		align-items: center;

		transition: background 150ms, color 150ms;

		.game {
			aspect-ratio: 16 / 9;
			flex: 0 1 min(calc(9 * 100vw / 16), 100vh);
		}

		.logs {
			display: flex;
			flex-direction: column;

			width: 100%;
			padding: 0 0.5em 0.5em 0.5em;

			background: var(--bg-sub);
		}

		.resizer {
			background: var(--surface1);
			cursor: ns-resize;
			width: 100%;
			flex: 0 0 0.25em;
		}

		.main h2 {
			margin: 0;
		}

		.expand { flex: 1; }
	`;

	this.fsOpen = false;
	this.achievementsOpen = false;

	useChange([gameState.patchFlowOpen], () => {
		if (gameState.patchFlowOpen) this.settingsOpen = false;
	});

	this.mount = () => {
		useChange([store.logs], (x) => {
			this.logcontainer.style.height = `${x}px`;
			this.logsize = x;
		});
	};

	const game = <GameView bind:canvas={use(this.canvas)} />;

	let started = false;
	this.start = async () => {
		if (started) return;
		started = true;
		this.dialogs = (
			<div>
				<Dialog name="Steam Cloud" bind:open={use(this.steamOpen)}>
					<SteamCloud open={use(this.steamOpen)} />
				</Dialog>
				<Dialog name="File System" bind:open={use(this.fsOpen)}>
					<OpfsExplorer open={use(this.fsOpen)} />
				</Dialog>
				<Dialog name="Achievements" bind:open={use(this.achievementsOpen)}>
					<Achievements open={use(this.achievementsOpen)} />
				</Dialog>
				<Dialog name="Mod Installer" bind:open={use(this.modInstallerOpen)}>
					<ModInstaller open={use(this.modInstallerOpen)} />
				</Dialog>
				<Dialog name="Mod manager" bind:open={use(this.updatesOpen)}>
					<UpdateDialog bind:open={use(this.updatesOpen)} />
				</Dialog>
				<Dialog name="Settings" bind:open={use(this.settingsOpen)}>
					<Settings />
				</Dialog>
			</div>
		) as HTMLDivElement;
		await (game.$ as ComponentType<typeof GameView>).start();
	};

	this.logsize = 0;

	return (
		<div style={use`--logsize: ${use(this.logsize, (x) => x || 0)}px;`}>
			<TopBar
				canvas={use(this.canvas)}
				bind:fsOpen={use(this.fsOpen)}
				bind:achievementsOpen={use(this.achievementsOpen)}
				bind:steamOpen={use(this.steamOpen)}
				bind:modInstallerOpen={use(this.modInstallerOpen)}
				bind:updatesOpen={use(this.updatesOpen)}
				bind:settingsOpen={use(this.settingsOpen)}
				bind:showLog={use(store.logs)}
			/>
			<div class="game">{game}</div>
			<div class="expand" />
			{$if(
				use(store.logs, (x) => x > 0) /* @ts-expect-error */,
				<>
					<div
						class="resizer"
						on:mousedown={(e: MouseEvent) => {
							const startY = e.clientY;
							const startHeight = this.logcontainer.clientHeight;
							let height: number;
							const onMouseMove = (e: MouseEvent) => {
								height = startHeight + startY - e.clientY;
								this.logcontainer.style.height = `${height}px`;
								this.logsize = height;
							};
							const onMouseUp = () => {
								document.removeEventListener("mousemove", onMouseMove);
								document.removeEventListener("mouseup", onMouseUp);
								store.logs = height;
								this.logsize = height;
							};
							document.addEventListener("mousemove", onMouseMove);
							document.addEventListener("mouseup", onMouseUp);
						}}
					></div>
					<div class="logs" bind:this={use(this.logcontainer)}>
						<LogView scrolling={true} />
					</div>
				</>
			)}
			{use(this.dialogs)}
			{$if(use(gameState.patchFlowOpen), <PatchFlow />)}
		</div>
	);
};
