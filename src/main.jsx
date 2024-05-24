import "dreamland/dev";

import { App, Logo } from "./App.jsx";
import { init } from "./game.js";
import { files } from "jszip";

export let store = $store(
	{
		theme: (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) ? "light" : "dark",
	},
	{ ident: "user-options", backing: "localstorage", autosave: "auto" }
);

if (window.SINGLEFILE) {
	document.body.querySelector("#interstitial").remove();
}

const chunkify = function*(itr, size) {
	let chunk = [];
	for (const v of itr) {
		chunk.push(v);
		if (chunk.length === size) {
			yield chunk;
			chunk = [];
		}
	}
	if (chunk.length) yield chunk;
};

function IntroSplash() {
	this.css = `
		position: absolute;
		top: 0;
		left: 0;
		z-index: 9999999;
		width: 100vw;
		height: 100vh;
		display: flex;
		flex-direction: column;
		align-items: center;
		margin: 0;
		background-color: var(--bg);
		color: var(--fg);

		display: flex;
		justify-content: center;
		align-items: center;

		.mountain {
			width: 100vw;
			height: 100vh;
			position: absolute;
			top: 0;
			left: 0;
			z-index: 0;
			background-image: url("/assets/mountain.png");
			background-size: cover;
			background-position: center;
		}

		#opaque {
			position: absolute;
			top: 0;
			left: 0;
			width: 100vw;
			height: 100vh;
			z-index: 1;
			background-color: var(--bg);
			opacity: 0;
			animation: fadeout 1.3s ease 0s 1;
		}

		#blur {
			backdrop-filter: blur(24px);
			background-color: color-mix(in srgb, var(--bg) 60%, transparent);
			position: absolute;
			top: 0;
			left: 0;
			width: 100vw;
			height: 100vh;
			z-index: 2;
		}

		.info {
			z-index: 3;
			border-radius: 1em;
			background-color: color-mix(in srgb, var(--surface0) 80%, transparent);
			backdrop-filter: blur(30px);
			max-width: max(480px, 40%);

			padding: 2em;

			animation: fadeandmove 1s ease 0s 1;

			div {
				margin-block: 1em;
			}
		}

		button,
		#bar {
			animation: fadein 0.15s ease 0s 1;
			width: 100%;
			height: 0.5em;
			background-color: var(--bg);
			border-radius: 0.5em;
		}

		#progress {
			transition: width 0.2s ease;
			height: 0.5em;
			background-color: var(--accent);
			border-radius: 0.5em;
		}

		.inner {
			margin-inline: 0.8rem;
		}

		.action {
			padding: 1em;
		}

		&.fadeout {
			animation: fadeout 0.5s ease;
		}

		@keyframes fadeandmove {
			from { opacity: 0; transform: translateY(1em); }
			to { opacity: 1; transform: translateY(0); }
		}

		@keyframes fadeout {
			from { opacity: 1; }
			to { opacity: 0; }
		}
    `

	let encbuf;
	this.progress = 0;

	this.downloading = false;
	this.downloaded = false;
	this.showprogress = true;

	if (window.SINGLEFILE) {
		this.downloaded = true;
		this.downloading = true;
		this.showprogress = false;
		this.progress = 100;
		encbuf = window.xorbuf;
		window.xorbuf = null;
	}
	this.decrypted = !DRM;

	this.decrypterror = "";



	let download = async () => {
		this.downloading = true;
		this.progress = 0;

		if (SPLIT) {
			encbuf = new Uint8Array(SIZE);

			await Promise.all([...chunkify(splits.entries(), Math.ceil(splits.length / 5))].map(async (chunk) => {
				for (let [idx, file] of chunk) {
					let data = await fetch(`_framework/data/${file}`);
					let buf = new Uint8Array(await data.arrayBuffer());
					encbuf.set(buf, idx * CHUNKSIZE);

					this.progress += (CHUNKSIZE / SIZE) * 100;
					console.log("File finished");
				}
			}));
		} else {
			encbuf = new Uint8Array(SIZE);
			let data = await fetch("_framework/data.data");
			let cur = 0;
			for await (const chunk of data.body) {
				encbuf.set(chunk, cur);
				cur += chunk.length;
				this.progress = (cur / SIZE) * 100;
			}
		}

		this.downloaded = true;
	};

	let decrypt = async () => {
		let input = h("input", { type: "file" });

		input.addEventListener("change", async () => {
			if (input.files.length === 0) {
				this.decrypterror = "No file selected";
				return;
			}
			let file = input.files[0];

			if (file.size !== 125540) {
				this.decrypterror = "Invalid key file (or a different version of the game?)";
				return;
			}

			let reader = new FileReader();
			reader.onload = async () => {
				let key = new Uint8Array(reader.result);

				this.progress = 0;
				for (let i = 0; i < encbuf.length; i += 4096) {
					encbuf[i] ^= key[i % key.length];

					if (i % (4096 * 200) == 0) {
						this.progress = i / encbuf.length * 100;
						await new Promise(r => requestAnimationFrame(r));
					}
				}
				this.decrypted = true;
			};
			reader.readAsArrayBuffer(file);
		});

		document.body.appendChild(input);
		input.click();
		input.remove();
	};

	let finish = async () => {
		this.playlabel.innerText = "Initializing...";
		window.assetblob = URL.createObjectURL(new Blob([encbuf]));

		await init();

		await new Promise(r => loadData(dotnet.instance.Module, r));
		console.info("Cached and loaded assets into VFS");
		localStorage["vfs_populated"] = true

		await loadfrontend();

		this.root.addEventListener("animationend", this.root.remove);
		this.root.classList.add("fadeout");
	};

	return (
		<main class={[use(store.theme)]}>
			<div class="mountain"></div>
			<div id="opaque"></div>
			<div id="blur"></div>
			<div class="info">
				<Logo />
				<div class="inner">
					<p>
						This is a mostly-complete port of <a href="https://store.steampowered.com/app/504230/Celeste/">Celeste</a> to
						the browser using dotnet's <code>wasmbrowser</code> template
						and <a href="https://github.com/RedMike/FNA.WASM.Sample">FNA.WASM.Sample</a>.
					</p>
					<p>
						It needs around 1.6GB of memory and will probably not work on mobile devices.
					</p>
					<p>
						{DRM && "You will need to own Celeste to play this. Make sure you have it downloaded and installed on your computer. " || ""}
						The game will autosave your progress, but the browser may wipe it after a while. Remember to periodically use the save icon at the top.
						{!window.SINGLEFILE && "This will download around ~700MB of assets to your browser's local storage." || ""}
					</p>

					{$if(use(this.downloading),
						$if(use(this.showprogress), (
							<div>
								<p>Downloading... ({use(this.progress, Math.floor)}% done)</p>
								<div id="bar">
									<div id="progress" style={{
										width: use`${this.progress}%`,
									}}></div>
								</div>
							</div>
						)),
						(
							<div>
								<button class="action important" on:click={download}>
									<span class="material-symbols-rounded">download</span>
									<span class="label">Download Assets</span>
								</button>
							</div>
						),
					)}

					{$if(use(this.downloaded),
						$if(use(this.decrypted),
							(
								<button class="action important" on:click={finish}>
									<span class="material-symbols-rounded">stadia_controller</span>
									<span bind:this={use(this.playlabel)} class="label">Play</span>
								</button>
							),
							(
								<div>
									<p>
										Downloaded assets.
										Now you need to decrypt them.
										Find the game files directory for your copy of Celeste and upload <code>Content/Dialog/english.txt</code>.
									</p>

									<button class="action important" on:click={decrypt}>
										<span class="material-symbols-rounded">encrypted</span>
										<span class="label">Decrypt</span>
									</button>

									<p>{use(this.decrypterror)}</p>
								</div>
							)
						)
					)}
				</div>
			</div>
		</main>
	)
}


export let app;
async function loadfrontend() {
	app = h(App).$;

	document.body.appendChild(app.root);
}

if (localStorage["vfs_populated"] !== "true") {
	document.body.appendChild(h(IntroSplash));
} else {
	loadfrontend();
}
