import { Button, Icon } from "./ui/Button";
import { Switch } from "./ui/Switch";
import {
	everestIdentityMatches,
	fetchEverestVersions,
	gameState,
	installedMods,
	refreshInstalledMods,
	deleteModZip,
	normalizeEverestIdentity,
	selectEverestBuild,
	UpdateEntry,
	InstalledMod,
	MissingDep,
} from "./game/dotnet";
import { downloadModFile, modsDir } from "./modinstaller";
import { epoxyFetch } from "./epoxy";
import { event } from "./analytics";

import iconDelete from "@ktibow/iconset-material-symbols/delete-forever-outline";

const parseModVersion = (text: string | undefined): number[] | null => {
	if (!text) return null;
	const match = String(text).match(/(\d+(?:\.\d+)+)/);
	if (!match) return null;
	return match[1].split(".").map(Number);
};

const compareVersions = (a: number[], b: number[]): number => {
	for (let i = 0; i < Math.max(a.length, b.length); i++) {
		const delta = (a[i] ?? 0) - (b[i] ?? 0);
		if (delta) return delta;
	}
	return 0;
};

type ModDependency = {
	name: string;
	version: string;
	optional: boolean;
};

const BUILTIN_DEPENDENCIES = new Set(
	["Celeste", "EverestCore", "Everest"].map((n) => n.toLowerCase())
);

const findMissingDeps = (
	installed: {
		name: string;
		version: string;
		file?: string;
		dependencies?: ModDependency[];
	}[]
): MissingDep[] => {
	const missing = new Map<string, MissingDep>();
	const byName = new Map(installed.map((m) => [m.name.toLowerCase(), m]));
	for (const mod of installed) {
		for (const dep of mod.dependencies ?? []) {
			const depName = (dep.name ?? "").trim();
			if (!depName) continue;
			if (BUILTIN_DEPENDENCIES.has(depName.toLowerCase())) continue;
			if (dep.optional) continue;
			const required = (dep.version ?? "").trim();
			const has = byName.get(depName.toLowerCase());
			const existing = missing.get(depName.toLowerCase());
			if (!has) {
				if (!existing) {
					missing.set(depName.toLowerCase(), {
						mod: mod.name,
						name: depName,
						required,
						installed: null,
						file: null,
					});
				} else if (required) {
					const current = parseModVersion(existing.required);
					const need = parseModVersion(required);
					if (!current || (need && compareVersions(current, need) < 0)) {
						existing.required = required;
					}
					existing.mod = `${existing.mod}, ${mod.name}`;
				}
				continue;
			}
			if (!required) continue;
			const current = parseModVersion(has.version);
			const need = parseModVersion(required);
			const outdated = !current || (need && compareVersions(current, need) < 0);
			if (!outdated) continue;
			if (existing) {
				if (need && (!current || compareVersions(current, need) < 0)) {
					existing.required = required;
				}
				existing.mod = `${existing.mod}, ${mod.name}`;
				continue;
			}
			missing.set(depName.toLowerCase(), {
				mod: mod.name,
				name: depName,
				required,
				installed: has.version,
				file: has.file ?? null,
			});
		}
	}
	return [...missing.values()];
};

const gamebananaSearchNames = (name: string): string[] => {
	const trimmed = (name ?? "").trim();
	if (!trimmed) return [];
	const variants = new Set<string>([trimmed]);
	const spaced = trimmed

		.replace(/([a-z0-9])([A-Z])/g, "$1 $2")
		.replace(/([A-Za-z])(\d)/g, "$1 $2");
	if (spaced !== trimmed) variants.add(spaced);
	return [...variants];
};

let checking = false;

const waitForRuntime = async () => {
	const start = Date.now();
	while (!gameState.ready) {
		if (Date.now() - start > 60000) {
			throw new Error("the game runtime is still starting");
		}
		await new Promise((r) => setTimeout(r, 500));
	}
};

const shortIdentity = (value: unknown): string =>
	String(value ?? "")
		.trim()
		.split("@")[0];

const GAMEBANANA_SEARCH_URL = "https://maddie480.ovh/celeste/gamebanana-search";
const SEARCH_TIMEOUT_MS = 30_000;

const rawSearchFetch = (query: string): Promise<Response> => {
	const timeout = new Promise<never>((_, rej) =>
		setTimeout(() => rej(new Error("mod search timed out")), SEARCH_TIMEOUT_MS)
	);
	return Promise.race([
		epoxyFetch(`${GAMEBANANA_SEARCH_URL}?q=${encodeURIComponent(query)}`),
		timeout,
	]);
};

const SEARCH_TTL_MS = 5 * 60 * 1000;
const searchCache = new Map<string, { at: number; results: any[] | null }>();

const searchFetch = async (query: string): Promise<any[] | null> => {
	const hit = searchCache.get(query);
	if (hit && Date.now() - hit.at <= SEARCH_TTL_MS) return hit.results;
	try {
		const res = await rawSearchFetch(query);
		const results = res.ok ? ((await res.json()) as any) : null;
		const list = Array.isArray(results) ? results : null;
		searchCache.set(query, { at: Date.now(), results: list });
		return list;
	} catch (err) {
		console.warn(`Mod search failed for ${query}`, err);
		return null;
	}
};

export async function checkForUpdates(): Promise<string> {
	if (checking) return "";
	checking = true;
	gameState.missingDeps = [];
	try {
		// List installed mods first without requiring the game runtime, so the
		// "Installed mods" section always resolves instead of loading forever
		// when the runtime is still starting (or never starts).
		await refreshInstalledMods();
		const installed = installedMods().mods;

		try {
			await waitForRuntime();
		} catch (err) {
			const msg = err instanceof Error ? err.message : String(err);
			return `Check failed: ${msg}. Try again in a moment.`;
		}

		const entries: UpdateEntry[] = [];
		const problems: string[] = [];

		const [everestRes, modRes] = await Promise.allSettled([
			(async () => {
				const { store } = await import("./store");
				const versions = await fetchEverestVersions();
				const latest = selectEverestBuild(versions, "stable");
				const latestIdentity = normalizeEverestIdentity(latest);
				const installed = (store.installedEverestVersion ?? "").trim();
				if (!installed || everestIdentityMatches(installed, latestIdentity)) {
					return null;
				}
				return {
					kind: "everest" as const,
					name: "Everest",
					installed: installed || "unknown",
					latest: latestIdentity,
					selector: latestIdentity,
				};
			})(),
			Promise.all(
				installed.map(async (mod) => {
					try {
						return await checkModUpdate(mod);
					} catch (err) {
						console.warn(`Update check failed for mod ${mod.name}`, err);
						return null;
					}
				})
			),
		]);

		if (everestRes.status === "fulfilled") {
			if (everestRes.value) entries.push(everestRes.value);
		} else {
			const msg =
				everestRes.reason instanceof Error
					? everestRes.reason.message
					: String(everestRes.reason);
			console.warn("Everest update check failed", everestRes.reason);
			problems.push(`Everest check failed (${msg})`);
		}

		if (modRes.status === "fulfilled") {
			for (const update of modRes.value) {
				if (update) entries.push(update);
			}
		} else {
			const msg =
				modRes.reason instanceof Error
					? modRes.reason.message
					: String(modRes.reason);
			console.warn("Mod update check failed", modRes.reason);
			problems.push(`Mod check failed (${msg})`);
		}

		gameState.missingDeps = findMissingDeps(installed);

		gameState.updates = entries;
		gameState.updatesAvailable = entries.length;
		event("updates-checked", { count: entries.length });
		if (entries.length > 0) {
			return problems.length ? `Note: ${problems.join("; ")}` : "";
		}
		if (problems.length) return `Check failed: ${problems.join("; ")}.`;
		return "";
	} finally {
		checking = false;
	}
}

const entryKey = (entry: UpdateEntry) => `${entry.kind}:${entry.name}`;

const depKey = (dep: MissingDep) => `dep:${dep.name.toLowerCase()}`;

async function removeSupersededFile(oldFile: string | null, filename: string) {
	if (!oldFile || oldFile.toLowerCase() === filename.toLowerCase()) return;
	try {
		await (await modsDir()).removeEntry(oldFile);
	} catch (err) {
		console.warn(`Could not remove superseded ${oldFile}`, err);
	}
}

const versionLine = (mod: InstalledMod): string =>
	mod.version ? `v${mod.version}` : "unknown version";

const searchGamebanana = async (name: string): Promise<any[] | null> => {
	for (const query of gamebananaSearchNames(name)) {
		const parsed = await searchFetch(query);
		if (parsed && parsed.length > 0) return parsed;
	}
	return null;
};

const gamebananaRelease = (
	list: any[],
	name: string,
	file?: string
): { name: string; url: string; description: string } | null => {
	const hasFiles = (e: any) =>
		e && Array.isArray(e.Files) && e.Files.length > 0;
	const match =
		list.find(
			(e: any) =>
				e &&
				typeof e.Name === "string" &&
				e.Name.toLowerCase() === name.toLowerCase()
		) ??
		(file
			? list.find(
					(e: any) =>
						hasFiles(e) &&
						e.Files.some(
							(f: any) =>
								f?.Name && String(f.Name).toLowerCase() === file.toLowerCase()
						)
				)
			: list.find(hasFiles)) ??
		null;
	if (!hasFiles(match)) return null;
	const latest =
		match.Files.find((f: any) => f?.IsLatestVersion) ?? match.Files[0];
	if (
		!latest?.URL ||
		!latest?.Name ||
		!String(latest.URL).startsWith("https://")
	)
		return null;
	return {
		name: latest.Name,
		url: latest.URL,
		description: latest.Description,
	};
};

async function checkModUpdate(mod: {
	file: string;
	name: string;
	version: string;
}): Promise<UpdateEntry | null> {
	if (BUILTIN_DEPENDENCIES.has(mod.name.toLowerCase())) return null;
	const list = await searchGamebanana(mod.name);
	if (!list) return null;
	const release = gamebananaRelease(list, mod.name, mod.file);
	if (!release) return null;
	const latest = parseModVersion(release.description);
	const current = parseModVersion(mod.version);
	if (!latest || !current) return null;
	if (compareVersions(latest, current) <= 0) return null;
	return {
		kind: "mod",
		name: mod.name,
		installed: mod.version,
		latest: latest.join("."),
		selector: JSON.stringify({
			url: release.url,
			filename: release.name,
			oldFile: mod.file,
		}),
	};
}

export const UpdateDialog: Component<
	{ open: boolean },
	{
		selected: Record<string, boolean>;
		working: boolean;
		status: string;
		confirming: string | null;
		deleting: string | null;
		applyLabel: string;
		canApply: boolean;
	}
> = function () {
	this.selected = {};
	this.working = false;
	this.status = "";
	this.confirming = null;
	this.deleting = null;
	this.applyLabel = "Install selected";
	this.canApply = false;
	this.css = `
		display: flex;
		flex-direction: column;
		gap: 0.75rem;

		.modrow {
			display: flex;
			align-items: center;
			gap: 0.75rem;
			padding: 0.5rem 0.75rem;
			border-radius: 0.85rem;
		}
		.modrow .rowmain {
			flex: 1;
			min-width: 0;
		}
		.modrow .name {
			font-weight: 600;
		}
		.modrow .versions {
			color: var(--fg6);
			font-size: 0.85rem;
			font-family: var(--font-mono);
		}
		.updatetoggle {
			flex: 0 0 auto;
		}
		
		.component-switch:has(.updatetoggle) .switch-label {
			display: none;
		}
		.installedlist {
			display: flex;
			flex-direction: column;
			gap: 0.75rem;
		}
		.sectionhead {
			font-weight: 700;
			margin-top: 0.25rem;
		}
		.depinstall {
			flex: 0 0 auto;
			white-space: nowrap;
		}
		.moddelete {
			flex: 0 0 auto;
			width: auto;
			aspect-ratio: 1;
			padding: 0.5rem;
		}
		.moddelete svg {
			width: 1.25rem;
			height: 1.25rem;
		}
		.confirmrow {
			display: flex;
			align-items: center;
			gap: 0.5rem;
			flex-wrap: wrap;
		}
		.confirmrow span {
			flex: 1;
			min-width: 8rem;
		}
		.footer {
			display: flex;
			flex-direction: column;
			align-items: stretch;
			gap: 0.5rem;
		}
		.footer .apply {
			width: 100%;
		}
		.status {
			color: var(--fg6);
			font-size: 0.85rem;
		}
		.note {
			color: var(--fg6);
			font-size: 0.85rem;
		}
		.empty {
			color: var(--fg6);
			font-style: italic;
			text-align: center;
			padding: 1rem;
		}
	`;

	useChange([this.open], () => {
		if (this.open) {
			const selected: Record<string, boolean> = {};
			for (const entry of gameState.updates) selected[entryKey(entry)] = true;

			for (const dep of gameState.missingDeps) selected[depKey(dep)] = true;
			this.selected = selected;

			this.status = "Checking for updates...";
			void checkForUpdates().then((outcome) => {
				this.status = outcome;
				const next: Record<string, boolean> = {};
				for (const entry of gameState.updates) next[entryKey(entry)] = true;
				for (const dep of gameState.missingDeps) next[depKey(dep)] = true;
				this.selected = next;
			});
		} else {
			this.confirming = null;
		}
	});

	useChange(
		[this.working, this.selected, gameState.updates, gameState.missingDeps],
		() => {
			if (this.working) {
				this.applyLabel = "Installing...";
				this.canApply = false;
				return;
			}
			const count =
				gameState.updates.filter((e) => this.selected[entryKey(e)]).length +
				gameState.missingDeps.filter((d) => this.selected[depKey(d)]).length;
			this.applyLabel =
				count === 0 ? "Install selected" : `Install ${count} selected`;
			this.canApply = count > 0 && !gameState.playing;
		}
	);

	const apply = async () => {
		if (this.working) return;
		const deps = gameState.missingDeps.filter((d) => this.selected[depKey(d)]);
		const selected = gameState.updates.filter(
			(e) => this.selected[entryKey(e)]
		);
		if (deps.length === 0 && selected.length === 0) return;
		if (gameState.playing) {
			this.status = "Quit the game before installing.";
			return;
		}
		const mods = selected.filter((e) => e.kind === "mod");
		const everest = selected.filter((e) => e.kind === "everest");
		this.working = true;
		try {
			let failedDeps = 0;
			for (const dep of deps) {
				try {
					await installOne(dep);
				} catch (err) {
					failedDeps++;
					console.error("Dependency install failed", err);
					this.status =
						err instanceof Error ? err.message : "Dependency install failed.";
				}
			}
			let done = 0;
			for (const entry of mods) {
				this.status = `Updating ${entry.name} (${done + 1}/${mods.length})...`;
				const { url, filename, oldFile } = JSON.parse(entry.selector);
				await downloadModFile(url, filename);
				await removeSupersededFile(oldFile ?? null, filename);
				done++;
			}
			if (done > 0) event("updates-applied", { count: done });
			await checkForUpdates();

			const still = new Set(gameState.updates.map(entryKey));
			const stillDeps = new Set(gameState.missingDeps.map(depKey));
			const next: Record<string, boolean> = {};
			for (const [key, value] of Object.entries(this.selected)) {
				if (value && (still.has(key) || stillDeps.has(key))) next[key] = true;
			}
			this.selected = next;
			if (everest.length > 0) {
				this.status = "";
				this.open = false;
				// The patch flow reads store.everestVersion, so point it at the
				// update target. Otherwise a stale picker selection would make
				// the repatch silently reinstall the already-installed build.
				const { store } = await import("./store");
				if (everest[0].selector) store.everestVersion = everest[0].selector;
				gameState.patchFlowOpen = true;
			} else {
				const parts: string[] = [];
				if (deps.length - failedDeps > 0)
					parts.push(
						deps.length - failedDeps === 1
							? "1 dependency installed"
							: `${deps.length - failedDeps} dependencies installed`
					);
				if (done > 0)
					parts.push(
						done === 1 ? "1 update installed" : `${done} updates installed`
					);
				if (failedDeps > 0)
					parts.push(
						failedDeps === 1
							? "1 dependency could not be installed"
							: `${failedDeps} dependencies could not be installed`
					);
				this.status = parts.length ? `${parts.join("; ")}.` : "Nothing to do.";
			}
		} catch (err) {
			console.error("Update failed", err);
			this.status =
				err instanceof Error
					? `Update failed: ${err.message}`
					: "Update failed.";
		} finally {
			this.working = false;
		}
	};

	const installOne = async (dep: MissingDep) => {
		if (gameState.playing) throw new Error("Quit the game first.");
		this.status = `Finding ${dep.name}...`;
		const list = await searchGamebanana(dep.name);
		const release = list ? gamebananaRelease(list, dep.name) : null;
		if (!release) throw new Error(`No downloadable file found for ${dep.name}`);
		this.status = `Installing ${dep.name}...`;
		await downloadModFile(release.url, release.name);
		await removeSupersededFile(dep.file, release.name);
		event("dep-installed", { name: dep.name });
	};

	const confirmDelete = (mod: InstalledMod) => {
		if (gameState.playing || this.deleting) return;
		this.confirming = mod.file;
	};

	const cancelDelete = () => {
		this.confirming = null;
	};

	const doDelete = async (mod: InstalledMod) => {
		if (gameState.playing || this.deleting) return;
		this.confirming = null;
		this.deleting = mod.file;
		try {
			await deleteModZip(mod.file);
			event("mod-deleted", { name: mod.name });
			await refreshInstalledMods();
			this.status = `Deleted ${mod.name}. Re-checking...`;
			this.status = await checkForUpdates();
		} catch (err) {
			console.error("Mod deletion failed", err);
			this.status =
				err instanceof Error
					? `Delete failed: ${err.message}`
					: "Delete failed.";
		} finally {
			this.deleting = null;
		}
	};

	return (
		<div>
			{$if(
				use(gameState.updates, (updates) => updates.length === 0),
				<div class="empty">Everything is up to date.</div>
			)}
			{use(gameState.updates, (updates) =>
				updates.map((entry) => (
					<div class="modrow">
						<div class="rowmain">
							<div class="name">{entry.name}</div>
							<div class="versions">
								{shortIdentity(entry.installed)} → {shortIdentity(entry.latest)}
							</div>
						</div>
						<Switch
							title={`Select update for ${entry.name}`}
							on={use(this.selected, (s) => !!s[entryKey(entry)])}
							disabled={use(this.working)}
							on:change={() => {
								const key = entryKey(entry);
								this.selected = {
									...this.selected,
									[key]: !this.selected[key],
								};
							}}
							class="updatetoggle"
						/>
					</div>
				))
			)}
			{$if(
				use(gameState.missingDeps, (deps) => deps.length > 0),
				<div class="sectionhead">Missing dependencies</div>
			)}
			{use(gameState.missingDeps, (deps) =>
				deps.map((dep) => (
					<div class="modrow">
						<div class="rowmain">
							<div class="name">{dep.name}</div>
							<div class="versions">
								Required by {dep.mod}
								{dep.required ? ` (needs ${dep.required})` : ""}
								{dep.installed
									? `, has ${shortIdentity(dep.installed)}`
									: ", not installed"}
							</div>
						</div>
						<Switch
							title={`Select dependency ${dep.name}`}
							on={use(this.selected, (s) => !!s[depKey(dep)])}
							disabled={use(this.working)}
							on:change={() => {
								const key = depKey(dep);
								this.selected = {
									...this.selected,
									[key]: !this.selected[key],
								};
							}}
							class="updatetoggle"
						/>
					</div>
				))
			)}
			<div class="sectionhead">Installed mods</div>
			{$if(
				use(installedMods().loaded, (loaded) => !loaded),
				<div class="empty">Loading installed mods...</div>
			)}
			{$if(
				use(installedMods().loaded),
				<div class="installedlist">
					{use(installedMods().mods, (mods) => {
						if (mods.length === 0)
							return (
								<div class="empty">
									No mods installed.
								</div>
							);
						return mods.map((mod) => (
							<div class="modrow">
								<div class="rowmain">
									<div class="name">{mod.name}</div>
									<div class="versions">{versionLine(mod)}</div>
								</div>
								{$if(
									use(this.confirming, (c) => c === mod.file),
									<div class="confirmrow">
										<span>Delete {mod.name}?</span>
										<Button
											on:click={() => doDelete(mod)}
											type="primary"
											icon="none"
											class="depinstall"
											disabled={use(gameState.playing)}
											title={`Delete ${mod.name}`}
										>
											<span>Delete</span>
										</Button>
										<Button
											on:click={cancelDelete}
											type="normal"
											icon="none"
											class="depinstall"
											disabled={false}
											title="Cancel"
										>
											<span>Cancel</span>
										</Button>
									</div>
								)}
								{$if(
									use(this.confirming, (c) => c !== mod.file),
									<Button
										on:click={() => confirmDelete(mod)}
										type="listaction"
										icon="full"
										class="moddelete"
										disabled={use(gameState.playing)}
										title={
											use(gameState.playing, (x) => x)
												? "Quit the game before deleting mods"
												: `Delete ${mod.name}`
										}
									>
										<Icon icon={iconDelete} />
									</Button>
								)}
							</div>
						));
					})}
				</div>
			)}
			<div class="footer">
				<Button
					on:click={apply}
					type="primary"
					icon="none"
					class="apply"
					disabled={use(this.canApply, (x) => !x)}
					title={"Install selected updates"}
				>
					<span>{use(this.applyLabel)}</span>
				</Button>
				<span class="status">{use(this.status)}</span>
			</div>
			{$if(
				use(gameState.playing),
				<div class="note">
					Quit the game before updating, installing or deleting mods.
				</div>
			)}
		</div>
	);
};
