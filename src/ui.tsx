import type { IconifyIcon } from "@iconify/types";


export const Button: Component<{
	"on:click": () => void,

	type: "primary" | "normal",
	icon: "full" | "left" | "none",
	disabled: boolean,
}, {
	children: any,
}> = function() {
	// @ts-expect-error
	this._leak = true;
	this.css = `
		display: flex;
		align-items: center;
		justify-content: center;

		border: none;
		border-radius: 4rem;
		padding: 0.5rem;

		transition: background 0.25s;
		font-family: var(--font-body);
		cursor: pointer;

		&.icon-full svg, &.icon-left svg {
			width: 1.5rem;
			height: 1.5rem;
		}
		&.icon-left {
			gap: 0.25rem;
		}

		&.type-primary {
			background: var(--accent);
			color: var(--fg);
		}
		&.type-normal {
			background: var(--surface1);
			color: var(--fg);
		}

		&.type-primary:not(:disabled):hover {
			background: color-mix(in srgb, var(--accent) 80%, white);
		}
		&.type-primary:not(:disabled):active {
			background: color-mix(in srgb, var(--accent) 70%, white);
		}
		&.type-normal:not(:disabled):hover {
			background: var(--surface2);
		}
		&.type-normal:not(:disabled):active {
			background: var(--surface3);
		}

		&:disabled {
			background: var(--surface0);
			cursor: not-allowed;
		}
	`;
	return (
		<button 
			on:click={this["on:click"]}
			class={`icon-${this.icon} type-${this.type}`}
			disabled={use(this.disabled)}
		>{use(this.children)}</button>
	)
}

export const Icon: Component<{ icon: IconifyIcon }, {}> = function() {
	// @ts-expect-error
	this._leak = true;
	this.mount = () => {
		this.root.innerHTML = this.icon.body;
		useChange([this.icon], () => {
			this.root.innerHTML = this.icon.body;
		})
	};
	return (
		<svg
			width="1em"
			height="1em"
			viewBox={use`0 0 ${this.icon.width} ${this.icon.height}`}
			xmlns="http://www.w3.org/2000/svg"
		></svg>
	);
}

export const Link: Component<{ href: string }, { children: any[] }> = function() {
	return <a href={this.href} target="_blank">{this.children}</a>
}
