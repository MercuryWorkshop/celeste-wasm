import { Switch } from "./ui/Switch";
import { Button } from "./ui/Button";
import { TextField } from "./ui/TextField";
import { analyticsEnabled, event } from "./analytics";
import { store } from "./store";
import { gameState, setSteamConnectionDisabled } from "./game/dotnet";
import { formatEverestDisplayVersion } from "./game/everest";
import { EverestVersionPicker } from "./everestPicker";
import { Icon } from "./ui/Button";

import iconSend from "@ktibow/iconset-material-symbols/send";

export const Settings: Component<
	{},
	{
		darkMode: boolean;
		showLogs: boolean;
	}
> = function () {
	this.css = `
		display: flex;
		overflow-x: hidden;
		overflow-y: auto;
		flex-direction: column;
		gap: 0.8rem;

		.setting,
		.component-switch {
			margin-inline: 0.25rem;
		}

		.setting {
			display: flex;
			flex-direction: row;
			align-items: center;
			justify-content: space-between;
			margin-inline: 0.25rem;
		}
  `;

	this.darkMode = store.theme === "dark";

	this.showLogs = store.logs > 0;

	return (
		<div>
			<Switch
				title="Dark Mode"
				bind:on={use(this.darkMode)}
				on:change={(e: Event) => {
					store.theme = (e.target as HTMLInputElement).checked
						? "dark"
						: "light";
				}}
				disabled={false}
			/>
			<Switch
				title="Show Logs"
				bind:on={use(this.showLogs)}
				disabled={false}
				on:change={(e: Event) => {
					store.logs = (e.target as HTMLInputElement).checked ? 1 : -1;
				}}
			/>
			<Switch
				title="Connect to Steam"
				bind:on={use(store.steamConnectionDisabled, (disabled) => !disabled)}
				disabled={false}
				on:change={(e: Event) => {
					void setSteamConnectionDisabled(
						!(e.target as HTMLInputElement).checked
					);
				}}
			/>
			<WispServer />
			<EverestVersion />
			<div>
				<div style="margin-inline: 0.2rem; user-select: none;">
					Accent Color
				</div>
				<AccentPicker />
			</div>
			{analyticsEnabled ? (
				<div>
					<div>
						This instance of Webleste has analytics for figuring out how many
						people are affected by r58playz's random regressions. We send an
						event on page load, when you finish providing assets (with what
						option you chose), when you finish patching (with whether you chose
						to install Everest), when you click the play button, and when you
						turn this off (but not after).
					</div>
					<Switch
						title="Enable Analytics"
						bind:on={use(store.analytics)}
						disabled={false}
						on:change={() =>
							event("analytics-toggle", { enabled: store.analytics })
						}
					/>
				</div>
			) : null}
		</div>
	);
};

export const WispServer: Component<{}> = function () {
	this.css = `
		display: flex;
		flex-direction: row;
		align-items: center;
		margin-inline: 0.25rem;
		gap: 0.5rem;

		input {
			flex: 1;
		}
	`;

	return (
		<div>
			<span>Wisp Proxy Server:</span>
			<TextField
				bind:value={use(store.wispServer)}
				placeholder={"wss://" + import.meta.env.VITE_WISP_URL}
			/>
		</div>
	);
};

export const EverestVersion: Component<{}, {}> = function () {
	this.css = `
		display: flex;
		flex-direction: row;
		align-items: center;
		gap: 0.5rem;
		margin-inline: 0.25rem;
		min-width: 0;

		.vercol {
			display: flex;
			flex-direction: column;
			line-height: 1.2;
			flex: 0 1 auto;
			min-width: 0;
		}

		.vercol > span:first-child {
			white-space: nowrap;
		}

		.installed {
			font-size: 0.75rem;
			opacity: 0.7;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			max-width: 12rem;
		}

		.row > .component-btn {
			flex: 0 0 auto;
			margin-inline-start: auto;
		}
		.everest-send {
			width: auto;
			aspect-ratio: 1;
			padding: 0.5rem;
		}
		.everest-send svg {
			width: 1.25rem;
			height: 1.25rem;
		}
	`;

	return (
		<div class="row">
			<div class="vercol">
				<span>Everest Version:</span>
				{$if(
					use(store.installedEverestVersion, (x) => !!String(x ?? "").trim()),
					<span class="installed">
						{use(
							store.installedEverestVersion,
							(installed) =>
								`Installed: ${formatEverestDisplayVersion(installed)}`
						)}
					</span>
				)}
			</div>
			<EverestVersionPicker disabled={false} />
			<Button
				on:click={() => (gameState.patchFlowOpen = true)}
				type="primary"
				icon="full"
				class="everest-send"
				disabled={false}
				title={"Repatch Everest"}
			>
				<Icon icon={iconSend} />
			</Button>
		</div>
	);
};

const AccentPicker: Component<{}> = function () {
	this.css = `
		display: flex;
		flex-wrap: nowrap;
		max-width: 100%;
		justify-content: space-between;
		align-items: center;
		margin-inline: 0.2rem;
		padding-block: 10px;
		padding-inline: calc(0.25rem + 5.25px);
		margin-top: 0.2rem;

		button {
			border: none;
			appearance: none;
			cursor: pointer;
			background-color: var(--accent);
			height: 2.2rem;
			width: 2.2rem;
			font-size: 1.35rem;
			font-family: var(--font-display);
			color: var(--bg);
			border-radius: 50%;
			outline: 2px solid var(--accent);
			outline-offset: -0.5px;
			transition: outline-width 0.25s ease, outline-offset 0.2s ease, background-color 0.2s ease, border-color 0.2s ease, color 0.2s ease;
		}

		button:not(:has(.selected)):hover {
			outline-width: 5.25px;
		}

		button:has(.selected) {
			transition: outline-width 0.25s ease, outline-offset 0.2s ease, background-color 0.2s ease, border-color 0.2s ease, color 0.2s ease;
			outline-offset: 2.75px;
		}

		.selected {
			user-select: none;
			-webkit-user-select: none;
		}
	`;

	const options = [undefined, "orange", "yellow", "green", "blue", "purple"];
	return (
		<div>
			{options.map((option) => (
				<button
					key={option}
					// @ts-ignore i hate you typescript
					on:click={() => (store.accentColor = option)}
					class={use`${store.theme} ${option}`}
					// @ts-ignore is there really no way to ignore a whole scope
					title={option?.charAt(0).toUpperCase() + option?.slice(1) || "Red"}
				>
					{$if(
						use(store.accentColor, (c) => c == option),
						<div class="selected">✓</div>
					)}
				</button>
			))}
		</div>
	);
};
