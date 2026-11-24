# Fractured Horizons

Sci-fi combat that fuses **real-time strategy** with **first- and third-person shooting**. Zoomed out, you command your forces from above. Pick any unit, press **F**, and you drop into it: fight as that soldier in first person, over the shoulder, or with a free orbit camera, while the rest of your army keeps executing your orders. Press **F** again to rise back to command.

Built on [Command Engine](https://github.com/Department-of-Offense/command-engine), our reusable Godot 4 C# framework for hybrid strategy/shooter games.

> **Status:** early prototype. Greybox units and terrain; the first playable slice is a "hold the sector" skirmish.

## Getting started

Requirements:
- [Godot 4.7 .NET edition](https://godotengine.org/download) (the .NET/Mono build, not the standard one)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer
- [Git LFS](https://git-lfs.com) (`git lfs install` once per machine) for art and audio

```sh
git clone https://github.com/Department-of-Offense/fractured-horizons.git
cd fractured-horizons
dotnet build
godot --path .            # or open project.godot in the editor and press F5
```

## Controls

| Command mode | |
|---|---|
| LMB / drag | Select units / box select |
| RMB | Move in formation, or attack the clicked enemy |
| Ctrl + RMB | Attack-move |
| Shift + order | Queue the order |
| H | Stop |
| WASD, Q/E, wheel, MMB drag | Pan, rotate, zoom, tilt |
| **F** | **Pilot the selected unit** |

| Pilot mode | |
|---|---|
| WASD, mouse, Space | Move, look, jump |
| LMB | Fire |
| V | Cycle view: first person → chase → orbit |
| F / Esc | Back to command |

## Project layout

```
addons/command_engine/   Vendored engine. Don't edit here (see below)
ENGINE_VERSION           Engine tag + commit this game is pinned to
scenes/                  Godot scenes (Main.tscn is the skirmish)
src/                     Game code (namespace FracturedHorizons)
tests/                   Game tests, run by the engine's headless runner
tools/sync-engine.sh     Update the vendored engine
docs/DESIGN.md           Game concept and design pillars
```

## Working with the engine

The engine is **copied into this repo** at a tagged release, so the game builds with no submodules or extra steps. CI fails if anyone edits `addons/command_engine` by hand.

To change engine behaviour, make the change in the [engine repo](https://github.com/Department-of-Offense/command-engine), merge and tag it, then:

```sh
tools/sync-engine.sh v0.2.0      # pull a tagged engine release
dotnet build && <run tests>      # see below
git add addons/command_engine ENGINE_VERSION && git commit -m "Update engine to v0.2.0"
```

While iterating on both repos at once: `ENGINE_PATH=../command-engine tools/sync-engine.sh` copies from your local checkout (CI warns until you sync a real tag). On Windows, run the script from Git Bash.

## Tests

```sh
dotnet build
godot --headless --path . --import
godot --headless --fixed-fps 60 --path . res://tests/TestRunner.tscn
```

CI runs this on every push and PR, plus a 30-second headless run of the main scene that fails on any engine error.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). In short: branch, PR, green CI, one approval, squash-merge.

## Licence

Copyright © 2026 Department of Offense. All rights reserved. See [LICENSE](LICENSE). Public for collaboration, not open source.
