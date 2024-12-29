import { Logo } from "./main";
import { Button, Icon, Link } from "./ui";

import iconFolderOpen from "@ktibow/iconset-material-symbols/folder-open-outline";

export const Splash: Component<{
	"on:next": () => void,
}, {}> = function() {
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

	const opfs = () => {
		// TODO opfs
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
					</div>

					<div>
						It needs around 0.8GB of memory and will probably not work on low-end devices.
					</div>

					<div>
						You will need to own Celeste to play this. Make sure you have it downloaded and installed on your computer.
					</div>

					<div>
						The background is from <Link href="https://www.fangamer.com/products/celeste-desk-mat-skies">fangamer merch</Link>.
					</div>

					<Button on:click={opfs} type="primary" icon="left" disabled={false}>
						<Icon icon={iconFolderOpen} />
						Select Celeste Content folder
					</Button>
				</div>
			</div>
		</div>
	)
}
