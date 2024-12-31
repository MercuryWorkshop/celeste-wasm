import type { IconifyIcon } from "@iconify/types";

import iconClose from "@ktibow/iconset-material-symbols/close";

export const Button: Component<{
	"on:click": (() => void) | ((e: PointerEvent) => void),

	class?: string,
	type: "primary" | "normal" | "listitem" | "listaction",
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
		border-radius: 1rem;
		padding: 0.5rem;

		transition: background 0.25s;
		font-family: var(--font-body);
		cursor: pointer;

		&.icon-full svg, &.icon-left svg {
			width: 1.5rem;
			height: 1.5rem;
		}
		&.icon-full {
			border-radius: 4rem;
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
		&.type-listitem {
			background: transparent;
			color: var(--fg);
			border-radius: 0.5rem;
		}
		&.type-listaction {
			background: var(--surface2);
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
		&.type-listitem:not(:disabled):hover {
			background: var(--surface1);
		}
		&.type-listitem:not(:disabled):active {
			background: var(--surface2);
		}
		&.type-listaction:not(:disabled):hover {
			background: var(--surface3);
		}
		&.type-listaction:not(:disabled):active {
			background: var(--surface4);
		}

		&:disabled {
			background: var(--surface0);
			cursor: not-allowed;
		}
	`;
	return (
		<button
			on:click={this["on:click"]}
			class={`icon-${this.icon} type-${this.type} ${this.class}`}
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

export const Dialog: Component<{ name: string, open: boolean }, { children: any[] }> = function() {
	this.css = `
		display: flex;
		flex-direction: column;
		gap: 0.5rem;

		background: var(--bg-sub);
		color: var(--fg);
		border: none;
		border-radius: 1rem;

		width: min(32rem, 100%);
		max-height: min(40rem, 100%);

		position: fixed;
		inset: 0;
		opacity: 0;
		visibility: hidden;
		pointer-events: none;
		transition: opacity 200ms, visibility 200ms;
		
		&[open] {
			opacity: 1;
			visibility: visible;
			pointer-events: auto;
		}

		&::backdrop {
			background: var(--bg-sub);
			opacity: 40%;
		}

		.header {
			display: flex;
			gap: 0.5rem;
			align-items: center;
		}

		.header h2 {
			margin: 0;
		}

		.children {
			overflow: scroll;
		}

		.expand { flex: 1 }
	`;
	this.mount = () => {
		const root = this.root as HTMLDialogElement;
		useChange([this.open], () => {
			if (this.open) {
				root.showModal();
			} else {
				root.close();
			}
		});
	}
	return (
		<dialog>
			<div class="header">
				<h2>{this.name}</h2>
				<div class="expand" />
				<Button on:click={() => { this.open = false }} type="normal" icon="full" disabled={false}>
					<Icon icon={iconClose} />
				</Button>
			</div>
			<div class="children">
				{this.children}
			</div>
		</dialog>
	)
}
