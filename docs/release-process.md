# Releases: merge often, release on purpose

The same process as [Keel](https://github.com/SEARATS/RatsSeaPowerKeel/blob/main/docs/community/releases.md) and Flotilla in the SEARATS org. Merging only collects work; a release is a snapshot we choose to make.

## Every merge

- Merge into `main` as often as needed. Nothing ships.
- Each PR adds its bullets under `## Unreleased` in [CHANGELOG.md](../CHANGELOG.md).
- No version bump in a PR: `<Version>` in `src/SpInternals/SpInternals.csproj` stays at the last release.
- Mods that pin this repo as a submodule should pin a release tag, not a random `main` commit.

## Release day

One release owner runs it on their own PC, start to finish.

| # | Step | What |
|---|---|---|
| 1 | Decide | "Today we release", who owns it, the number (table below) |
| 2 | Snapshot | PR "Release x.y.z": `## Unreleased` becomes `## x.y.z - <date>`, `<Version>` set, a fresh empty `## Unreleased`; merge it; tag `vx.y.z` on the merge commit |
| 3 | Build from the tag | `dotnet build src/SpInternals -c Release` against the game's current main branch; note the game build in the release notes (the bindings are generated from it) |
| 4 | Checks | every program file through VirusTotal |
| 5 | Ship | GitHub release of the tag: `SP.Internals.dll`, notes from the CHANGELOG section, the game build it was generated from |
| 6 | Tell people | the release owner tells the mods that use it (sp-controls and others) which tag to pin |

## Which number

| Bump | When |
|---|---|
| x.y.**z+1** | only fixes |
| x.**y+1**.0 | new helpers or assemblies, or a new game build regenerated without breaking names |
| **x+1**.0.0 | a generated name or helper that mods use is gone or renamed |

## Never

- Bump the version in a normal PR.
- Ship a DLL that is not built from a release tag.
- Skip the virus scan on release day.
- Commit or attach the game's own DLLs.
