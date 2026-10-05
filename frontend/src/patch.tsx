import { Switch } from "./ui/Switch";
import { Button, Icon } from "./ui/Button";
import {
	gameState,
	preInit,
	PatchCeleste,
	removeUploadedCopy,
	hasInstalledEverest,
} from "./game/dotnet";
import { LogView } from "./game";
import { event } from "./analytics";
import { EverestVersionPicker } from "./everestPicker";

import iconManufacturing from "@ktibow/iconset-material-symbols/manufacturing";
import iconClose from "@ktibow/iconset-material-symbols/close";

export const Patch: Component<
	{
		"on:done": () => void;
	},
	{
		patching: boolean;
		everest: boolean;
		pickerDisabled: boolean;
		progress: number;
		plabel: string;
		status: string;
		confirmingRemove: boolean;
	}
> = function () {
	this.patching = false;
	this.everest = false;
	// Default the toggle to the installed state: a repatch/update opened
	// while Everest is installed should keep it, not silently drop it.
	void hasInstalledEverest().then((has) => {
		if (has) this.everest = true;
	});
	this.pickerDisabled = true;
	this.progress = 0;
	this.plabel = "";
	this.status = "";
	this.confirmingRemove = false;
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

		.verrow {
			display: flex;
			flex-direction: row;
			align-items: center;
			gap: 0.5rem;
			min-width: 0;
		}

		.verrow > span {
			flex: 0 0 auto;
			white-space: nowrap;
		}

		.pwrap {
			display: flex;
			flex-direction: column;
			gap: 0.35rem;
		}

		.plabel {
			font-size: 0.9rem;
			color: var(--fg6);
		}

		.pbar {
			background: color-mix(in srgb, var(--surface1) 70%, transparent);
			border-radius: 1rem;
			height: 0.6rem;
			overflow: hidden;
		}

		.pbar .bar {
			background: var(--accent);
			border-radius: 1rem;
			height: 0.6rem;
			transition: width 150ms ease-out;
		}

		.pbar .bar.ind {
			width: 40%;
			animation: pslide 1.2s ease-in-out infinite alternate;
		}
		@keyframes pslide {
			from { margin-inline-start: -40%; }
			to { margin-inline-start: 100%; }
		}
		@media (prefers-reduced-motion: reduce) {
			.pbar .bar.ind { animation: none; }
		}
	`;

	useChange([this.patching, this.everest], () => {
		this.pickerDisabled = this.patching || !this.everest;
	});

	const phaseLabel = (phase: string, value: number): string => {
		switch (phase) {
			case "starting":
				return "Starting up (first run takes a while)...";
			case "everest-download":
				return value >= 0
					? `Downloading Everest (${Math.round(value * 200)}%)...`
					: "Downloading Everest...";
			case "everest-extract":
				return "Extracting Everest...";
			case "patch":
				return "Patching Celeste (this usually takes a few minutes)...";
			case "done":
				return "Patch complete.";
			default:
				return "";
		}
	};

	const patch = async () => {
		this.patching = true;
		this.confirmingRemove = false;
		this.status = "";
		this.progress = -1;
		this.plabel = phaseLabel("starting", -1);
		try {
			await preInit();
			await PatchCeleste(this.everest, (phase, value) => {
				this.progress = value;
				this.plabel = phaseLabel(phase, value);
			});
			event("patched", { everest: this.everest });
			this["on:done"]();
		} catch (err) {
			console.debug("================================================");
			console.error("[!!!] There was an error patching Celeste!", err);

			console.log("Please reload the page and try again.");
			this.status = err instanceof Error ? err.message : String(err);
			console.debug("================================================");
		} finally {
			this.patching = false;
		}
	};

	const removeCopy = async () => {
		if (!this.confirmingRemove) {
			this.confirmingRemove = true;
			return;
		}
		this.confirmingRemove = false;
		this.patching = true;
		try {
			await removeUploadedCopy();
			event("assets-removed", {});
			location.reload();
		} catch (err) {
			this.status = err instanceof Error ? err.message : String(err);
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
				on:change={() => (this.status = "")}
			/>
			<div class="verrow">
				<span>Everest version:</span>
				<EverestVersionPicker disabled={use(this.pickerDisabled)} />
			</div>

			<Button
				type="primary"
				icon="left"
				on:click={patch}
				disabled={use(this.patching)}
			>
				<Icon icon={iconManufacturing} />
				Patch Celeste
			</Button>
			{$if(
				use(this.patching),
				<div class="pwrap">
					<div class="plabel">{use(this.plabel)}</div>
					<div class="pbar">
						{use(this.progress, (p) =>
							p < 0 ? (
								<div class="bar ind" />
							) : (
								<div
									class="bar"
									style={`width: ${Math.round(Math.min(Math.max(p, 0), 1) * 100)}%`}
								/>
							)
						)}
					</div>
				</div>
			)}
			<Button
				type="normal"
				icon="none"
				on:click={removeCopy}
				disabled={use(this.patching)}
			>
				{use(this.confirmingRemove, (x) =>
					x
						? "Click again to remove the uploaded Celeste files"
						: "Remove uploaded Celeste files"
				)}
			</Button>

			<div class="console">
				<LogView scrolling={true} />
			</div>
			{$if(
				use(this.status),
				<div>
					<div class="error">{use(this.status)}</div>
					<Button
						type="normal"
						icon="none"
						disabled={false}
						on:click={() => location.reload()}
					>
						Reload page and try again
					</Button>
				</div>
			)}
		</div>
	);
};

export const PatchFlow: Component<{}, {}> = function () {
	this.css = `
		position: fixed;
		inset: 0;
		z-index: 200;
		background: var(--bg-sub);
		overflow-y: auto;
		padding: 1rem;

		.sheet {
			max-width: 40rem;
			margin-inline: auto;
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
		}

		.head {
			display: flex;
			align-items: center;
			gap: 0.5rem;
		}

		.head h2 {
			margin: 0;
		}

		.head .expand {
			flex: 1;
		}
	`;

	const close = () => {
		gameState.patchFlowOpen = false;
	};

	return (
		<div>
			<div class="sheet">
				<div class="head">
					<h2>Patch Celeste</h2>
					<div class="expand" />
					<Button
						on:click={close}
						type="normal"
						icon="full"
						disabled={false}
						title={"Close"}
					>
						<Icon icon={iconClose} />
					</Button>
				</div>
				<Patch on:done={close} />
			</div>
		</div>
	);
};
