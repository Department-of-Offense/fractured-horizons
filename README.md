# Fractured Horizons

**The proving ground for [godot-systems](https://github.com/Department-of-Offense/godot-systems)**, the reusable Godot 4 C# modules every Department of Offense game is built from.

Fractured Horizons is a collection of playable **labs**. Each lab puts specific systems under realistic game conditions so we can feel them, test them, measure them and find their gaps before other games depend on them. The labs build toward a sci-fi game that fuses real-time strategy with first- and third-person shooting (see [docs/DESIGN.md](docs/DESIGN.md)), and that concept is what keeps them honest. But the product here is the systems.

## The labs

| Lab | Systems under test | What it proves |
|---|---|---|
| **Camera Lab** | cameras | Cameras work with nothing else: a free-flying drone, every view, spring arm in corridors and tunnels |
| **Firing Range** | units, combat, cameras | Piloting and shooting without the strategy layer; weapons hurt non-unit targets |
| **Squad Command** | command, units, cameras | RTS control, plus a game-defined order (Follow) and click rule built on the extension points |
| **Skirmish** | hybrid, ui, command, units, combat, cameras | The full command ↔ pilot loop with win and lose conditions |
| **Stress Test** | everything, at scale | 100+ units per side, with live frame and physics timings |

It has already paid off: the stress test exposed quadratic slowdown in unit target scanning (145 ms/frame at 120 per side), now fixed in godot-systems (25 ms).

## Run it

Requirements: [Godot 4.7 .NET](https://godotengine.org/download), [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer, [Git LFS](https://git-lfs.com) (`git lfs install` once per machine).

```sh
git clone https://github.com/Department-of-Offense/fractured-horizons.git
cd fractured-horizons
dotnet build
godot --path .                          # lab menu (or open project.godot and press F5)
godot --path . -- --lab=range           # straight into a lab: camera, range, squad, skirmish, stress
godot --path . -- --lab=stress --units=200
```

Press **F1** in any lab to return to the menu. Each lab shows its controls and live readouts in the bottom-left corner.

## How the loop works

1. **Find a gap** while playing or testing a lab: a missing feature, a bug, an awkward API, a slow path.
2. **Fix it in [godot-systems](https://github.com/Department-of-Offense/godot-systems)**, not here. Use `SYSTEMS_PATH=../godot-systems tools/sync-systems.sh` to try the change in the labs before opening the PR.
3. **Tag a release** of godot-systems, then `tools/sync-systems.sh vX.Y.Z` here and commit.
4. **Prove it** with a lab test, so the gap stays closed.

The vendored `addons/dof_*` folders are read-only here; CI fails if they differ from `SYSTEMS_VERSION`. A **nightly canary** re-runs every lab against godot-systems `main`, so we hear about breakage before a release is tagged. Every CI run also posts a performance snapshot of the stress lab in its summary.

Lab code itself should stay small. If a lab needs a lot of code to do something, that code probably belongs in a system. See [docs/LABS.md](docs/LABS.md) for adding labs and moving code out of them into a system.

## Layout

```
addons/dof_*/            Vendored godot-systems modules (read-only here)
SYSTEMS_VERSION          Systems tag, commit and module list this repo is pinned to
scenes/Main.tscn         Lab menu
scenes/labs/             One scene per lab
src/Labs/                Lab scripts (namespace FracturedHorizons.Labs)
tests/LabTests.cs        A test for every lab, on the dof_testing runner
tools/sync-systems.sh    Update the vendored systems
docs/LABS.md             Writing labs; moving code into godot-systems
docs/DESIGN.md           The game concept the labs build toward
```

## Tests

```sh
dotnet build
godot --headless --path . --import
godot --headless --fixed-fps 60 --path . res://tests/TestRunner.tscn
```

## Licence

Copyright © 2026 Department of Offense. All rights reserved. See [LICENSE](LICENSE). Public for collaboration, not open source.
