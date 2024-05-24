let olog = console.log;

let ring = [];

export function Logs() {
	this.css = `
		overflow-y: scroll;
		font-size: 0.9em;
		max-height: 32em;
		border: 0.7px solid var(--surface5);
		border-radius: 0.7em;
		padding: 1em;
		background-color: var(--surface0);
		font-family: var(--font-mono);
		min-height: 16em;
    `

	let ringsize = 200;

	setInterval(() => {
		for (let log of ring) {
			this.root.append(h("span", { style: { color: log[0] } }, log[1]));

			if (this.root.children.length > ringsize) {
				this.root.children[0].remove();
			}
		}
		ring = []

		this.root.scrollTop = this.root.scrollHeight
	}, 1500);

	console.log = (...args) => {
		log("var(--fg)", ...args)
	}
	console.warn = (...args) => {
		log("var(--warning)", ...args)
	}
	console.error = (...args) => {
		log("var(--error)", ...args)
	}
	console.info = (...args) => {
		log("var(--info)", ...args)
	}
	console.debug = (...args) => {
		log("var(--fg6)", ...args)
	}


	return (<pre></pre>)
}

export function log(color, ...args) {
	olog(...args);
	ring.push([color, `[${new Date().toISOString()}] ` + args.join(" ") + "\n"]);
}
