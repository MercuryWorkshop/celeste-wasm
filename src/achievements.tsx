import { achievements, glowingAchievements } from "./achievementData";
import { rootFolder } from "./fs";

import glowOuter from "./steam-glow-outer.png";
import glowInner from "./steam-glow-inner.png";

export type Achievement = {
	hidden: boolean,
	unlocked_image: string,
	locked_image: string,
	name: string,
	description: string,
};

async function getAchievementsFile(): Promise<FileSystemFileHandle | null> {
	try {
		return await rootFolder.getFileHandle("achievements.json");
	} catch {
		return null;
	}
}

export async function getUnlockedAchievements(): Promise<Record<string, Achievement>> {
	const file = await getAchievementsFile()
	if (file) {
		const unlocked = JSON.parse(await file.getFile().then(r => r.text())) as string[];
		return Object.fromEntries(unlocked.map(x => [x, achievements[x]] as const));
	} else {
		return {};
	}
}

export async function getLockedAchievements(): Promise<Record<string, Achievement>> {
	const file = await getAchievementsFile()
	if (file) {
		const unlocked = JSON.parse(await file.getFile().then(r => r.text())) as string[];
		return Object.fromEntries(Object.entries(achievements).filter(([id, _]) => !unlocked.includes(id)));
	} else {
		return achievements;
	}
}

(self as any).achievements = {
	getUnlockedAchievements,
	getLockedAchievements,
};

export const Achievements: Component<{
	open: boolean,
}, {
	unlocked: [string, Achievement][],
	locked: [string, Achievement][],
}> = function() {
	this.unlocked = [];
	this.locked = [];

	this.css = `
		.achievements {
			display: flex;
			flex-direction: column;
			gap: 1em;
		}

		.achievement {
			display: flex;
			gap: 0.5rem;

			background: var(--surface1);
			padding: 0.5rem;
		}
		.achievement.unlocked {
			background: var(--surface2);
		}

		.achievement .inner {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
			justify-content: center;
		}

		.achievement > .inner > :first-child {
			font-size: 1.25rem;
			font-family: var(--font-display);
		}

		.achievement:not(.unlocked).hidden > .inner > :last-child {
			display: none;
		}

		.padding {
			margin-bottom: 8rem;
		}

		/* https://codepen.io/Dataminer/pen/pooWzpo */
		.image {
			display: inline-block;
			position: relative;
			height: 64px;
			width: 64px;
		}
		.image .glow-root, .image .glow-container, .image .glow {
			position: absolute;
			top: -10px;
			right: -10px;
			bottom: -10px;
			left: -10px;
		}
		.image .glow-root {
			mask-image: url("${glowOuter}");
			mask-repeat: repeat;
			mask-size: 100%;	
		}
		.image .glow-container {
			animation-name: rotate;
			animation-duration: 18s;
			animation-timing-function: linear;
			animation-iteration-count: infinite;
			animation-direction: reverse;
			mask-image: url("${glowInner}");
			mask-repeat: repeat;
			mask-size: 100%;
		}
		.image .glow {
			animation-name: rotate;
			animation-duration: 6s;
			animation-timing-function: linear;
			animation-iteration-count: infinite;	
		}
		.image img {
			position: relative;
			vertical-align: top;
			height: 64px;
			width: 64px;
		}
		.unlocked .image .glows .glow {
			background: radial-gradient(rgba(255, 201, 109, 0.178) 0%, rgba(255, 201, 109, 0) 6%, rgba(255, 201, 109, 0.178) 10%, #ffb84e 26%, rgba(255, 201, 109, 0.178) 35%, #ffb84e 40%, rgba(255, 201, 109, 0.178) 60%, #ffb84e 82%, rgba(255, 201, 109, 0.178) 100%);
		}
		.unlocked .image:has(.glows) img {
			box-shadow: 0px 0px 2px 1px rgba(255, 184, 78, 0.6), 0px 0px 16px 1px rgba(255, 217, 78, 0.4);
		}
		@keyframes rotate {
			to {
				transform: rotate(1turn);
			} 
		}
	`;

	useChange([this.open], async () => {
		this.unlocked = Object.entries(await getUnlockedAchievements());
		this.locked = Object.entries(await getLockedAchievements());
	});

	const createAchievement = (id: string, achievement: Achievement, unlocked: boolean) => {
		return (
			<div class={`achievement ${unlocked ? "unlocked" : ""} ${achievement.hidden ? "hidden" : ""}`}>
				<div class="image">
					<div class={`glow-root ${glowingAchievements.includes(id) ? "glows" : ""}`}>
						<div class="glow-container">
							<div class="glow" />
						</div>
					</div>
					<img src={unlocked ? achievement.unlocked_image : achievement.locked_image} />
				</div>
				<div class="inner">
					<div>{achievement.name}</div>
					<div>{achievement.description}</div>
				</div>
			</div>
		)
	}

	return (
		<div>
			<h3>Unlocked</h3>
			<div class="achievements">
				{use(this.unlocked, x => x.map(([id, x]) => createAchievement(id, x, true)))}
			</div>
			<h3>Locked</h3>
			<div class="achievements">
				{use(this.locked, x => x.map(([id, x]) => createAchievement(id, x, false)))}
			</div>
			<div class="padding" />
		</div>
	)
}
