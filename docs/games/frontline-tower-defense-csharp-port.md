# Frontline Tower Defense C# Port

## Scope

This port replaces only the Frontline Tower Defense JavaScript game runtime. The existing
/games/tower-defense game, its match model, data, progression and leaderboard integration
remain independent and must not be reused as Frontline gameplay.

Target presentation is 100% C# and Blazor rendering. No JavaScript gameplay, Canvas renderer,
or JavaScript game loop is part of the target architecture.

## Source inventory

The current branch contains a dedicated Razor wrapper at
src/ThaiX.Client/Pages/Games/FrontlineTowerDefenseGame.razor and a separate browser runtime
under wwwroot/games/frontline-tower-defense/assets/.

The runtime manifest declares an 800 by 480 logical stage and 42 frames per second. The game
simulation itself uses a 24 Hz timer and scales movement by elapsed time and a speed multiplier,
with a cap of 8 on the per-tick multiplier. The game world uses a 25 by 25 grid with 32-pixel
cells. The asset library contains bitmap and sound records; the checked-in directory contains
2,193 bitmap files and 20 sound files. The runtime currently owns input, simulation, rendering,
audio, and local persistence.

The original save namespace is FD-2-32-317, accessed through the runtime storage prefix
fd2:so:. The port must preserve compatible player progression where the original serialized
schema can be established. Do not infer field meanings from storage key names alone.

## Verified gameplay concepts

The inspected runtime includes campaign and bonus mission progression, a War-Mart credit
economy, tower unlocks, starting-funds upgrades, wave spawning, tower targeting, tower
placement/upgrades/selling, enemy movement, combat effects, pause/speed controls, and local
save/load behavior. Special content includes four sniper bonus missions, named shop unlocks such as Flame Thrower,
Napalm Support Strike, Bio Cannon, Bio Mortar, Particle Beam Cannon, Mounted Machine Gun,
Plasma Cannon, Missile Launcher and Shotgun Tower. The simulation has four enemy damage-over-time
or status channels (burn, radiation, poison and freeze), enemy resistance multipliers, metal
enemy behavior, temperature thresholds, and freeze/unfreeze state. Enemy pathing uses a grid
pathfinder and random start/end points selected from each level's authored endpoints. Tower
selection supports near, far, strong, weak, heat and cold targeting modes. The runtime registers
14 tower classes (11 standard and 3 support) and 13
enemy classes including infantry, vehicles and a bot.

Exact balancing values, all tower and enemy definitions, mission layouts, complete wave schedules,
upgrade formulas, and save schema still require extraction from the runtime and asset metadata.
The save schema is partially verified: bPlayed, credits, stFunds, Tower0..Tower19 and
Level0..Level39. A first-time profile starts with 1,000 credits, unlocks tower indexes 2, 3, 4
and 13, and unlocks level index 0. Existing saves must be migrated without changing these
semantics.
Do not substitute the separate ThaiX Tower Defense definitions.

## Porting rules

1. Treat the existing Frontline runtime as the behavioral reference until parity is verified.
2. Extract definitions and formulas into explicit C# data records before implementing them.
3. Keep simulation deterministic and independent of Blazor components.
4. Keep UI state and rendering in Razor/C#; use SVG/HTML elements and CSS, not Canvas.
5. Keep asset identifiers and source asset files stable while the renderer is being replaced.
6. Preserve the existing route and authentication requirement.
7. Keep persistence isolated under a Frontline-specific key and version the new schema.
8. Do not delete the old runtime or assets until feature parity and save migration are verified.
9. Never import TowerDefenseMatch, TowerDefenseData, or their gameplay contracts.

## Implementation status

- [x] Create an independent C# simulation boundary and deterministic tick entry point.
- [x] Add focused unit tests for placement, combat rewards, leaks, and defeat.
- [x] Extract the legacy 24 Hz simulation timer, 25 by 25 grid, 32-pixel cell size, enemy status channels, target modes, and partial player save schema.
- [ ] Extract complete source tower/enemy/mission/wave definitions and balance formulas.
- [ ] Port every tower and enemy behavior, including special damage/status effects.
- [ ] Port campaign, War-Mart, bonus missions, and progression.
- [ ] Implement versioned local save/load and migration from the original schema.
- [ ] Replace the JavaScript wrapper with a fully Blazor-rendered UI.
- [ ] Port all visuals and interaction states to accessible responsive Razor/SVG.
- [ ] Verify gameplay parity and remove the legacy runtime only after acceptance.
