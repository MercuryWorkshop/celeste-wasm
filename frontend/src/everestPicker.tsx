import { store } from "./store";
import {
	fetchEverestVersions,
	normalizeEverestIdentity,
	type EverestBuild,
} from "./game/dotnet";
import { everestIdentityMatches } from "./game/everest";

export const EverestVersionPicker: Component<
	{ disabled: boolean },
	{
		builds: EverestBuild[];
		loading: boolean;
	}
> = function () {
	this.css = `
		display: contents;

		select {
			flex: 1 1 10rem;
			min-width: 6rem;
			max-width: 15rem;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			background: var(--bg-sub);
			color: var(--fg);
			border: 2px solid var(--surface4);
			border-radius: 0.5rem;
			padding: 0.4rem 0.5rem;
			font: inherit;
		}
	`;

	this.builds = [];
	this.loading = false;

	const optionValue = (build: EverestBuild): string =>
		normalizeEverestIdentity(build) || "stable";

	let loadingBuilds = false;
	const ensureBuilds = async () => {
		if (this.builds.length > 0 || loadingBuilds) return;
		loadingBuilds = true;
		this.loading = true;
		try {
			const all = await fetchEverestVersions();
			this.builds = all.filter(
				(b) => (b.branch ?? "").trim().toLowerCase() === "stable"
			);
		} catch (err) {
			console.warn("Everest version list failed", err);
			this.builds = [];
		}
		if (
			store.everestVersion !== "stable" &&
			!this.builds.some(
				(b) =>
					optionValue(b) === store.everestVersion ||
					everestIdentityMatches(store.everestVersion, optionValue(b))
			)
		) {
			store.everestVersion = "stable";
		}
		this.loading = false;
		loadingBuilds = false;
	};

	return (
		<div>
			<select
				bind:value={use(store.everestVersion)}
				on:focus={() => void ensureBuilds()}
				on:click={() => void ensureBuilds()}
				disabled={use(this.disabled)}
			>
				<option value="stable">stable (latest)</option>
				{use(this.builds, (builds) =>
					builds.map((b) => {
						const value = optionValue(b);
						const version = String(b.version ?? "").trim();
						const date = String(b.date ?? "")
							.trim()
							.slice(0, 10);
						const label = [version || value, date && `(${date})`]
							.filter(Boolean)
							.join(" ");
						return <option value={value}>{label}</option>;
					})
				)}
				{$if(
					use(this.loading),
					<option disabled={true}>Loading versions...</option>
				)}
			</select>
		</div>
	);
};
