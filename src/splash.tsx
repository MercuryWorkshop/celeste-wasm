import { Logo } from "./main";
import { Button, Icon, Link } from "./ui";

import iconFolderOpen from "@ktibow/iconset-material-symbols/folder-open-outline";
import iconDownload from "@ktibow/iconset-material-symbols/download";
import { copyFolder, rootFolder } from "./fs";

export const Splash: Component<{
	"on:next": () => void,
}, {
	copyDisabled: boolean,
	downloadDisabled: boolean,
	status: string,
}> = function() {
	this.css = `
		position: relative;

		.splash, .blur, .main {
			position: absolute;
			width: 100%;
			height: 100%;
			top: 0;
			left: 0;
		}

		.splash {
			object-fit: cover;
			z-index: 1;
		}

		.blur {
			backdrop-filter: blur(0.5vw);
			background-color: color-mix(in srgb, var(--bg) 40%, transparent);
			z-index: 2;
		}

		.main {
			display: flex;
			align-items: center;
			justify-content: center;
			z-index: 3;
		}

		.container {
			backdrop-filter: blur(0.5vw);
			background-color: color-mix(in srgb, var(--bg) 80%, transparent);
			width: min(32rem, 100%);
			margin: 0 1rem;
			padding: 1em;
			border-radius: 1rem;

			color: var(--fg);

			display: flex;
			flex-direction: column;
			gap: 0.5em;
		}

		.logo {
			display: flex;
			justify-content: center;
		}
	`;

	this.copyDisabled = false;
	this.downloadDisabled = true;
	this.status = "";

	const validateDirectory = async (directory: FileSystemDirectoryHandle) => {
		if (directory.name != "Content") {
			return "Directory name is not Content";
		}
		for (const child of ["Celeste", "Dialog", "Effects", "FMOD", "Graphics", "Maps", "Monocle", "Overworld", "Tutorials"]) {
			try {
				await directory.getDirectoryHandle(child, { create: false });
			} catch {
				return `Failed to find subdirectory ${child}`
			}
		}
		return "";
	};

	const opfs = async () => {
		const directory = await showDirectoryPicker();
		const res = await validateDirectory(directory);
		if (res) {
			this.status = res;
			return;
		}

		this.copyDisabled = true;
		this.status = "Copying...";
		await copyFolder(directory, rootFolder);
		this.status = "Copied!";

		this["on:next"]();
	}

	return (
		<div>
			<img class="splash" src="/splash.png" />
			<div class="blur" />
			<div class="main">
				<div class="container">
					<div class="logo">
						<Logo />
					</div>
					<div>
						This is a mostly-complete port of <Link href="https://www.celestegame.com/">Celeste</Link> to the browser using dotnet's WASM support.
						It needs around 0.8GB of memory and will probably not work on low-end devices.
					</div>

					<div>
						You will need to own Celeste to play this. Make sure you have it downloaded and installed on your computer.
					</div>

					<div>
						The background is from <Link href="https://www.fangamer.com/products/celeste-desk-mat-skies">fangamer merch</Link>.
					</div>

					<Button on:click={opfs} type="primary" icon="left" disabled={use(this.copyDisabled)}>
						<Icon icon={iconFolderOpen} />
						Select Celeste Content folder
					</Button>
					<Button on:click={() => { }} type="primary" icon="left" disabled={use(this.downloadDisabled)}>
						<Icon icon={iconDownload} />
						Download/decrypt coming soon
					</Button>
					{$if(use(this.status, x => x.length > 0), <span>{use(this.status)}</span>)}
				</div>
			</div>
		</div>
	)
}
