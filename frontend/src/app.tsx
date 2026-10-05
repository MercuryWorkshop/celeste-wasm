import "dreamland";
import "./styles.css";
import { Main } from "./main";
import { Splash } from "./splash";
import { store } from "./store";
import "./analytics";

const App: Component<
	{},
	{
		el: HTMLElement;
		showInstructions: boolean;
	}
> = function () {
	this.css = `
		position: relative;

		#splash, #main {
			position: absolute;
			width: 100%;
			height: 100%;
			top: 0;
			left: 0;
		}
		#splash {
			z-index: 100;
		}

		@keyframes fadeout {
			from { opacity: 1; scale: 1; }
			to { opacity: 0; scale: 1.2; }
		}
	`;

	const main = <Main />;
	const start = () => (main.$ as ComponentType<typeof Main>).start();
	const next = (anim: boolean) => {
		if (anim) {
			this.el.addEventListener("animationend", this.el.remove);
			this.el.style.animation = "fadeout 0.5s ease";
		} else {
			this.el.remove();
		}
		start();
	};

	return (
		<div id="app" class={use`${store.theme} ${store.accentColor}`}>
			<div id="splash" bind:this={use(this.el)}>
				<Splash on:next={next} start={start} />
			</div>
			<div id="main">{main}</div>
		</div>
	);
};

if ("serviceWorker" in navigator && !import.meta.env.DEV) {
	const registerServiceWorker = () => {
		navigator.serviceWorker.register("./service-worker.js").catch((err) => {
			console.warn("service worker registration failed", err);
		});
	};

	if (document.readyState === "complete") {
		registerServiceWorker();
	} else {
		window.addEventListener("load", registerServiceWorker);
	}
}

const root = document.getElementById("app")!;
try {
	root.replaceWith(<App />);
} catch (err) {
	console.log(err);
	root.replaceWith(document.createTextNode(`Failed to load:\n ${err}`));
	document.body.classList.add("error");
}
