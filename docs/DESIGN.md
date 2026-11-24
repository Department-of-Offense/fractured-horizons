# Fractured Horizons: design notes

A living document. Change it through PRs like code.

> **Current focus:** this repo is a systems lab (see the [README](../README.md)). The concept below is the north star the labs build toward: it decides which systems we need and what "good" feels like. The game itself is not being built out yet.

## Pitch

Command a sci-fi strike force from orbit, then drop into any soldier, vehicle or craft and fight the battle yourself, without pausing the war around you.

## The core loop

1. **Command** (zoomed out, top-down strategy): survey the battlefield, select units, issue move / attack / attack-move orders, queue plans.
2. **Possess** (select a unit, press F): the camera swoops down and you *are* that unit. You inherit its situation and its job. Its orders stay queued.
3. **Fight** (FPS, chase cam or orbit cam): aim, move and shoot with your own skill. You can do better than the AI, but while you fight you're not commanding.
4. **Return** (F again): the unit picks its orders back up, and you're back above the battle, now knowing what it looks like on the ground.

The tension we want: **attention is the scarce resource.** Every second spent piloting is a second the rest of the army runs on autopilot.

## Design pillars

- **Seamless switching.** Command to pilot to command must be instant and readable. No loading, no pause, camera transitions under half a second.
- **Every unit is playable.** If it's on the field and it's yours, you can drive it. Each unit type gets controls and a camera that suit it.
- **The AI is competent, not perfect.** Units follow orders sensibly and defend themselves, so leaving them is safe. A human pilot should still clearly outperform them.
- **Readable from both heights.** Silhouettes, team colours and effects must read from 100 m up and from 2 m away.

## Camera modes (implemented in the engine)

| Mode | Use | Body facing |
|---|---|---|
| Strategy | Command view: pan, rotate, zoom, tilt | n/a |
| First person | Precision shooting | Follows mouse |
| Chase | Over-the-shoulder, better awareness | Follows mouse |
| Orbit | Free camera around the unit, zoomable; good for vehicles and tactical overview | Faces movement direction |

## Open questions

- Setting and factions: who's fighting, and why is the horizon "fractured"?
- Unit roster: infantry first. Then which vehicles, aircraft, mechs?
- Win conditions beyond elimination: capture points, objectives, resources?
- Should possessing a unit cost anything (cooldown, command points), or is the attention cost enough?
- Single-player campaign, skirmish vs AI, multiplayer, or all three? This decides how early we build networking into the engine.

## Current slice

`scenes/Main.tscn`, "hold the sector": 5 friendly troopers vs 7 hostiles who attack-move toward you after 8 seconds. Greybox only. Its job is to prove the command → possess → return loop feels good.
