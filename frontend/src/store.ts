// Steam connection is opt-in: nothing Steam-related may start unless the
// user explicitly enabled it in Settings. Lives here (not dotnet.ts) to
// avoid a circular import.
export const DEFAULT_STEAM_CONNECTION_DISABLED = true;

export let store = $store(
	{
		logs: 0,

		theme:
			window.matchMedia &&
			window.matchMedia("(prefers-color-scheme: light)").matches
				? "light"
				: "dark",

		wispServer: import.meta.env.VITE_WISP_URL || "wss://anura.pro",
		everestVersion: "stable",
		installedEverestVersion: "",
		epoxyVersion: "",
		accentColor: undefined,

		analytics: true,
		steamConnectionDisabled: DEFAULT_STEAM_CONNECTION_DISABLED,
		dismissedOutdatedAlert: false,
	},
	{ ident: "options", backing: "localstorage", autosave: "auto" }
);

if (typeof store.everestVersion !== "string" || !store.everestVersion.trim()) {
	store.everestVersion = "stable";
}
if (typeof store.installedEverestVersion !== "string") {
	store.installedEverestVersion = "";
}
if (typeof store.steamConnectionDisabled !== "boolean") {
	store.steamConnectionDisabled = DEFAULT_STEAM_CONNECTION_DISABLED;
}
if (typeof store.analytics === "undefined") store.analytics = true;
if (typeof store.dismissedOutdatedAlert !== "boolean") {
	store.dismissedOutdatedAlert = false;
}

(self as any).store = store;
