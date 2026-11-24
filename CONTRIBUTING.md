# Contributing

## Workflow

1. Branch from `main`: `feature/<short-name>` or `fix/<short-name>`.
2. Keep PRs small and focused. One lab or fix per PR.
3. Open a PR. CI must pass and one teammate must approve before merging.
4. Squash-merge. The PR title becomes the commit message.

`main` is protected: no direct pushes, no force pushes.

## Where code goes

This repo is the **testbed**. The reusable code lives in [godot-systems](https://github.com/Department-of-Offense/godot-systems).

| If it is... | It goes in |
|---|---|
| Reusable gameplay: cameras, units, combat, commands, UI widgets | godot-systems |
| A fix for something a lab found | godot-systems (then tag and re-sync here) |
| A scenario that exercises systems | a lab here (see [docs/LABS.md](docs/LABS.md)) |
| A prototype that might become a system | a lab here, moved into godot-systems once it proves itself |

Never edit `addons/dof_*` here. They're vendored, and CI checks them against `SYSTEMS_VERSION`. To try an unreleased systems change in the labs: `SYSTEMS_PATH=../godot-systems tools/sync-systems.sh` (CI warns until you sync a real tag).

## Assets

- Art and audio go through **Git LFS**. Run `git lfs install` once per machine. The file types are in `.gitattributes`. Add a new type there *before* committing the first file of that type.
- Commit exported assets (`.glb`, `.png`, `.ogg`), not source files (`.blend`, `.psd`), unless the team agrees.
- LFS storage and bandwidth are limited, even on public repos.

## Code

- Follow `.editorconfig`: file-scoped namespaces, `_camelCase` private fields, nullable references on, no warnings.
- Every lab is in `LabCatalog` and has a test in `tests/LabTests.cs`.
