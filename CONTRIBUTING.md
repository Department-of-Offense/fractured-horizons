# Contributing

## Workflow

1. Branch from `main`: `feature/<short-name>` or `fix/<short-name>`.
2. Keep PRs small and focused. One feature or fix per PR.
3. Open a PR. CI must pass and one teammate must approve before merging.
4. Squash-merge. The PR title becomes the commit message.

`main` is protected: no direct pushes, no force pushes.

## Where code goes

| If it is... | It goes in |
|---|---|
| Useful to any hybrid strategy/shooter (unit control, cameras, commands, selection) | the [engine repo](https://github.com/Department-of-Offense/command-engine) |
| Specific to Fractured Horizons (factions, unit roster, maps, story, UI skin) | this repo, under `src/` and `scenes/` |

Never edit `addons/command_engine` here. It's vendored and CI checks it against `ENGINE_VERSION`.

## Assets

- Art and audio go through **Git LFS**. Run `git lfs install` once per machine. The file types are listed in `.gitattributes`. Add a new type there *before* committing the first file of that type.
- Keep source files (`.blend`, `.psd`) out of the repo unless the team agrees. Commit exported assets (`.glb`, `.png`, `.ogg`).
- LFS storage and bandwidth are limited, even on public repos. Avoid committing large files repeatedly.

## Code

- Follow `.editorconfig`: file-scoped namespaces, `_camelCase` private fields, nullable references on, no warnings.
- New behaviour gets a test in `tests/` where practical.
- Game code talks to units through the engine's API (`Issue(...)`, `GameModeManager`, signals). It doesn't reach into engine internals.
