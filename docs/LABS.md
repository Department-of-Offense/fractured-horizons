# Writing labs

A lab is a small playable scene that puts one or more godot-systems modules under realistic conditions. Labs exist to make the systems better. They are not features of a shipping game.

## What makes a good lab

- **It names what it tests.** Its `LabCatalog` entry lists the systems under test and the one-line question it answers ("do cameras work with no units?").
- **It uses only what it claims.** The Camera Lab touches nothing but `dof_cameras` (its scenery comes from plain-Godot `LabWorld`). If a lab says "units, combat, cameras", don't let it quietly lean on `dof_hybrid`.
- **It uses the systems like a real game would:** public APIs, subclassing, interfaces, signals. Never reflection or private access. If a lab can't do something cleanly, that's the finding.
- **It exposes numbers.** Use `SetReadout(...)` for live stats (accuracy, arm fraction, frame times) so problems show up on screen.
- **It has a test.** At minimum it's in `LabCatalog` (so `EveryLabLoadsAndRuns` covers it). Ideally it also gets a behaviour test driven by simulated input (`Input.ActionPress`) in `tests/LabTests.cs`.

## Adding a lab

1. Write `src/Labs/MyLab.cs`, deriving from `Lab` (gives you F1-to-menu and the info panel):

   ```csharp
   public partial class MyLab : Lab
   {
       protected override string LabId => "mylab";
       public override void _Ready()
       {
           // build the scene with the systems under test
           AddInfoPanel("controls go here");
       }
       public override void _Process(double delta) => SetReadout($"live numbers");
   }
   ```

2. Add `scenes/labs/MyLab.tscn` with that script on the root node (copy an existing lab scene and change the path).
3. Add a `LabInfo` to `LabCatalog.All` in `src/Labs/Lab.cs`: id, title, scene, systems, purpose.
4. Add a behaviour test to `tests/LabTests.cs`.
5. Run it: `godot --path . -- --lab=mylab`.

## When lab code should move into godot-systems

Lab code is allowed to prototype something new. Move it into godot-systems when **any** of these is true:

- A second lab, or another game, wants the same code.
- It's generic: nothing in it is specific to this lab's scenario.
- It works around a limitation in a system. Fix the system instead.
- It's grown past a page or so of logic.

Examples already in this repo:

| Lab code | Status |
|---|---|
| `FollowCommand` and `SquadSelection` (Squad Command) | Deliberately lab-side: they show that games *can* extend orders and click rules. If a second game wants "follow", move it into `dof_units`. |
| `UnitSubject` (Firing Range) | Stays: it shows how a game without `dof_hybrid` connects units to cameras. `dof_hybrid` ships the same adapter. |
| `Crosshair` (Firing Range) | Candidate: a generic crosshair belongs in `dof_ui` once something else needs one. |
| `Drone` (Camera Lab) | Candidate: a free-fly spectator body could become a `dof_cameras` helper. |

To move code: open a PR in godot-systems with the code and a test, tag a release, sync it here, and replace the lab copy with the system's version.

## Reporting what a lab finds

Open the issue in **[godot-systems](https://github.com/Department-of-Offense/godot-systems/issues)** (that's where the fix goes). Name the lab, how to reproduce it (`godot --path . -- --lab=...`), and any readout numbers.
