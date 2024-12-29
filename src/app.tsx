import "dreamland";
import "./styles.css";
import { Main } from "./main";

const App: Component<{}, {}> = function() {
	return (
		<div id="app" class="dark">
			<Main />
		</div>
	)
}

const root = document.getElementById("app")!;
try {
	root.replaceWith(<App />);
} catch(err) {
	console.log(err);
	root.replaceWith(document.createTextNode(`Failed to load: ${err}`));
}
