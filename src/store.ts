export let store = $store(
	{
		theme: (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) ? "light" : "dark",
	},
	{ ident: "options", backing: "localstorage", autosave: "auto" }
);
