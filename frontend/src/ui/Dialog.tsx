import iconClose from "@ktibow/iconset-material-symbols/close";
import { Button, Icon } from "./Button";

export const Dialog: Component<
	{ name: string; open: boolean },
	{ children: any[] }
> = function () {
	this.css = `
		display: flex;
		flex-direction: column;
		gap: 0.8rem;

		background: var(--bg-sub);
		color: var(--fg);
		border: 1.25px solid var(--surface3);
		border-radius: 1.5rem;

		width: min(40rem, calc(100% - 2rem));
		max-height: min(50rem, calc(100% - 2rem));

		position: fixed;
		inset: 0;
		
		margin: auto;
		opacity: 0;

		scale: .9;
		transform: rotate3d(1, 0, 0, -20deg);
		filter: brightness(1.5);

		pointer-events: none;
		transition: opacity 0.25s, transform 0.175s, filter 0.2s, scale 0.2s, background 0.1s, border-color 0.1s;
		transition-timing-function: ease;
		transition-delay: 0.05s, 0.05s, 0.05s, 0.05s;
		transform-origin: 50% 0%;
		perspective: 1250px;

		&[open] {
			opacity: 1;
			transform: rotate3d(1,0,0,0deg);
			filter: brightness(1.0);
			transition-delay: 0.05s, 0.05s, 0.05s, 0.2s;
			pointer-events: auto;
		}

		&[open]::backdrop {
			background: rgba(32, 28, 28, 0.35);
		}

		&::backdrop {
			background: rgba(32, 28, 28, 0);
			transition: background 0.2s;
		}

		.header {
			display: flex;
			gap: 0.5rem;
			align-items: center;
			border-bottom: 1.8px solid var(--surface2);
			transition: border-color 0.1s ease;
			padding-bottom: 0.5rem;
			user-select: none;
			-webkit-user-select: none;
		}

		.header h2 {
			margin: 0;
		}

		.children {
			overflow-y: scroll;
			overflow-x: hidden;
			scrollbar-width: none;
			scrollbar-color: transparent transparent;
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

		let downOnBackdrop = false;
		root.addEventListener("mousedown", (e) => {
			downOnBackdrop = e.target === root;
		});
		root.addEventListener("click", (e) => {
			if (e.target === root && downOnBackdrop) this.open = false;
		});
	};
	return (
		<dialog class="component-dialog">
			<div class="header">
				<h2>{this.name}</h2>
				<div class="expand" />
				<Button
					on:click={() => {
						this.open = false;
					}}
					type="normal"
					icon="full"
					disabled={false}
					title={"Close"}
				>
					<Icon icon={iconClose} />
				</Button>
			</div>
			<div class="children">{this.children}</div>
		</dialog>
	);
};
