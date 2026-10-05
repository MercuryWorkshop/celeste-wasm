export type EverestBuild = {
	branch?: string;
	version?: string | number;
	commit?: string;
	date?: string;
	mainDownload?: string;
};

function trimEverestField(value: string | number | undefined): string {
	return String(value ?? "").trim();
}

function everestVersionAliases(value: string | number | undefined): string[] {
	const normalized = trimEverestField(value).toLowerCase();
	const aliases = new Set([normalized]);
	const compact = normalized.match(/^1\.(\d+)(?:\.0)?$/);
	if (compact) aliases.add(compact[1]);
	if (/^\d+$/.test(normalized)) aliases.add(`1.${normalized}.0`);
	return [...aliases];
}

function parseEverestVersion(
	value: string | number | undefined
): number[] | undefined {
	let normalized = trimEverestField(value).toLowerCase();
	if (/^\d+$/.test(normalized)) normalized = `1.${normalized}.0`;
	const parts = normalized.split(".");
	if (parts.length < 2 || !parts.every((part) => /^\d+$/.test(part))) {
		return undefined;
	}
	return parts.map(Number);
}

const compareVersionsDesc = (a: number[], b: number[]): number => {
	for (let i = 0; i < Math.max(a.length, b.length); i++) {
		const delta = (b[i] ?? 0) - (a[i] ?? 0);
		if (delta) return delta;
	}
	return 0;
};

type EverestIdentityParts = {
	branch: string;
	version: string;
	commit: string;
};

export function parseEverestIdentity(value: string): EverestIdentityParts {
	const normalized = value.trim().toLowerCase();
	const at = normalized.indexOf("@");
	const core = at < 0 ? normalized : normalized.substring(0, at);
	const colon = core.indexOf(":");
	return {
		branch: colon < 0 ? "" : core.substring(0, colon),
		version: colon < 0 ? core : core.substring(colon + 1),
		commit: at < 0 ? "" : normalized.substring(at + 1),
	};
}

export function formatEverestDisplayVersion(value: unknown): string {
	const core = String(value ?? "")
		.trim()
		.split("@")[0];
	const colon = core.indexOf(":");
	return (colon < 0 ? core : core.substring(colon + 1)).trim();
}

export function everestIdentityMatches(installed: string, expected: string) {
	const actualParts = parseEverestIdentity(installed);
	const wanted = expected.trim().toLowerCase();
	if (!actualParts.version || !wanted) return false;
	if (wanted === "stable") return actualParts.branch === "stable";

	const expectedParts = parseEverestIdentity(wanted);
	if (expectedParts.branch && actualParts.branch !== expectedParts.branch) {
		return false;
	}
	if (expectedParts.commit && actualParts.commit !== expectedParts.commit) {
		return false;
	}

	if (!expectedParts.branch && !expectedParts.commit) {
		if (actualParts.commit === wanted) return true;
		return everestVersionAliases(expectedParts.version).some((version) =>
			everestVersionAliases(actualParts.version).includes(version)
		);
	}

	if (
		expectedParts.branch &&
		!expectedParts.commit &&
		actualParts.commit === expectedParts.version
	) {
		return true;
	}

	return (
		!!expectedParts.version &&
		everestVersionAliases(expectedParts.version).some((version) =>
			everestVersionAliases(actualParts.version).includes(version)
		)
	);
}

export function normalizeEverestIdentity(build: EverestBuild): string {
	const branch = trimEverestField(build.branch).toLowerCase();
	const version = trimEverestField(build.version).toLowerCase();
	const commit = trimEverestField(build.commit).toLowerCase();
	const identity =
		version && commit ? `${version}@${commit}` : version || commit;
	return branch && identity
		? `${branch}:${identity}`
		: identity || trimEverestField(build.date).toLowerCase();
}

export function selectEverestBuild(
	versions: EverestBuild[],
	requested?: string
): EverestBuild {
	if (!Array.isArray(versions) || versions.length === 0) {
		throw new Error("Everest version list is empty");
	}
	const wanted = (requested ?? "").trim().toLowerCase();
	if (!wanted || wanted === "stable") {
		const stable = versions.filter(
			(v) => trimEverestField(v.branch).toLowerCase() === "stable"
		);
		if (stable.length === 0)
			throw new Error("No stable Everest build is available");
		if (stable.length === 1) return stable[0];

		const withVersion = stable.map((build) => ({
			build,
			version: parseEverestVersion(trimEverestField(build.version)),
		}));
		if (withVersion.every(({ version }) => version)) {
			withVersion.sort((a, b) => compareVersionsDesc(a.version!, b.version!));
			if (
				withVersion.length < 2 ||
				compareVersionsDesc(
					withVersion[0].version!,
					withVersion[1].version!
				) !== 0
			) {
				return withVersion[0].build;
			}
		}
		throw new Error("Stable Everest builds are ambiguous");
	}
	const matches = versions.filter((build) => {
		const branch = trimEverestField(build.branch).toLowerCase();
		const version = trimEverestField(build.version);
		const versionAliases = everestVersionAliases(version);
		const commit = trimEverestField(build.commit).toLowerCase();
		const candidates = [
			version,
			...versionAliases,
			commit,
			normalizeEverestIdentity(build),
			...versionAliases.map((alias) => `${branch}:${alias}`),
			...versionAliases.map((alias) =>
				commit ? `${branch}:${alias}@${commit}` : ""
			),
			...versionAliases.map((alias) => (commit ? `${alias}@${commit}` : "")),
			commit ? `${branch}:${commit}` : "",
		];
		return candidates.some((candidate) => candidate.toLowerCase() === wanted);
	});
	if (matches.length === 0) {
		throw new Error(`Requested Everest version is unavailable: ${requested}`);
	}
	if (matches.length > 1) {
		throw new Error(
			`Requested Everest version is ambiguous: ${requested} matches ${matches.length} builds`
		);
	}
	return matches[0];
}

const EVEREST_VERSIONS_TTL_MS = 10 * 60 * 1000;

let everestVersionsCache: EverestBuild[] | null = null;
let everestVersionsFetchedAt = 0;

export const cachedEverestVersions = async (
	fetcher: () => Promise<EverestBuild[]>,
	ttlMs: number = EVEREST_VERSIONS_TTL_MS
): Promise<EverestBuild[]> => {
	const fresh =
		everestVersionsCache !== null &&
		Date.now() - everestVersionsFetchedAt <= ttlMs;
	if (fresh) return everestVersionsCache!;
	try {
		const versions = await fetcher();
		everestVersionsCache = versions;
		everestVersionsFetchedAt = Date.now();
		return versions;
	} catch (err) {
		if (everestVersionsCache !== null) {
			return everestVersionsCache;
		}
		throw err;
	}
};
