import "dreamland";
import "./styles.css";
import { Main } from "./main";
import { Splash } from "./splash";
import { store } from "./store";

const App: Component<{}, {
	el: HTMLElement
}> = function() {
	this.css = `
		position: relative;

		div {
			position: absolute;
			width: 100%;
			height: 100%;
			top: 0;
			left: 0;
		}
		#splash {
			z-index: 1;
		}

		@keyframes fadeout {
			from { opacity: 1; scale: 1; }
			to { opacity: 0; scale: 1.2; }
		}
	`;

	return (
		<div id="app" class={use(store.theme)}>
			<div id="splash" bind:this={use(this.el)}>
				<Splash on:next={() => {
					this.el.addEventListener("animationend", this.el.remove);
					this.el.style.animation = "fadeout 0.5s ease";
				}} />
			</div>
			<div id="main">
				<Main />
			</div>
		</div>
	)
}

const root = document.getElementById("app")!;
try {
	root.replaceWith(<App />);
} catch (err) {
	console.log(err);
	root.replaceWith(document.createTextNode(`Failed to load: ${err}`));
}
