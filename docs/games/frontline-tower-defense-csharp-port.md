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
with a cap of 8 on the per-tick multiplier. The game world initializes a 25 by 25 grid with 32-pixel cells. The level simulation starts
with 1,000 level health; an enemy reaching an exit subtracts its remaining health from that
pool. A direct kill awards its credit value and twice that value as level score. The asset
library contains bitmap and sound records; the checked-in directory contains 2,193 bitmap
files and 20 sound files. The runtime currently owns input, simulation, rendering, audio, and
local persistence.

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
enemy behavior, temperature thresholds, and freeze/unfreeze state. Enemy pathing uses breadth-first traversal with eight-way movement; diagonal movement is
allowed only when both adjacent orthogonal cells are walkable. Start and end cells are selected
randomly from each level's authored endpoints. Tower selection supports near, far, strong,
weak, heat and cold targeting modes. The runtime registers 11 standard tower types, 3 support
types, and 13 enemy types including infantry, vehicles and a bot.

Verified enemy definitions now cover all 13 runtime types. The five infantry types use base
health 50; the bot uses 1,000; vehicle base health ranges from 120 to 550. Their base speed and
resistance modifiers are recorded in FrontlineEnemyCatalog. Every spawned enemy's health is
scaled by 10 percent per dispatched wave group. The 11 standard tower base stats, targeting modes, build costs, unlock indexes, and War-Mart
unlock prices are recorded in FrontlineTowerCatalog. Three support tower stat sets and shot
limits are also recorded; their special projectile and finite-shot behavior still requires
porting. The base tower class increments its firing meter by rate-of-fire multiplied by the
simulation scale, fires at 100, and applies upgrades in 20-percent increments at half of its
sale value (which is half of the purchase cost). The first campaign level's 24 authored wave groups have
been transcribed into FrontlineWaveSchedule.
The save schema is partially verified: bPlayed, credits, stFunds, Tower0..Tower19 and
Level0..Level39. FrontlinePlayerProgress now models the legacy 20 tower slots, 40 level slots,
first-time defaults (1,000 credits, 500 starting funds, tower indexes 2, 3, 4 and 13 unlocked,
and level index 0 unlocked), best-score persistence, War-Mart purchases, and conversion to and
from the legacy save record. Browser persistence and migration wiring remain pending.
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

- [x] Create an independent C# simulation boundary and tick entry point.
- [x] Add focused unit tests for placement, combat rewards, leaks, defeat, status effects, and wave timing.
- [x] Port the grid pathfinder's eight-way traversal and diagonal corner rule.
- [x] Convert grid paths into world-space cell-center movement paths.
- [x] Transcribe the first campaign level's 24 authored wave groups.
- [x] Extract all 13 enemy types' base health, speed, metal flag, and observed resistance modifiers.
- [x] Extract all 11 standard tower stats, 3 support tower stats, targeting modes, build prices, unlock indexes, and War-Mart prices.
- [x] Port legacy player progression defaults, best-level score handling, and War-Mart purchase rules.
- [x] Extract the 24 Hz timer, elapsed-time scale, level health leak rule, and direct-kill score rule.
- [ ] Integrate per-level grid maps, random authored endpoints, and path recomputation into the simulation loop.
- [x] Model base tower fire-meter threshold, six target modes, and 20-percent upgrade increments.
- [x] Separate direct-kill score awards from damage-over-time kill awards.
- [ ] Port tower-specific projectiles, area effects, support abilities, and exact firing parameters.
- [ ] Extract complete source tower/enemy/mission/wave definitions and balance formulas.
- [ ] Port tower-specific projectiles, support strike finite-shot behavior, and special damage/status effects.
- [ ] Port campaign, War-Mart, bonus missions, and progression.
- [ ] Implement versioned local save/load and migration from the original schema.
- [ ] Replace the JavaScript wrapper with a fully Blazor-rendered UI.
- [ ] Port all visuals and interaction states to accessible responsive Razor/SVG.
- [ ] Verify gameplay parity and remove the legacy runtime only after acceptance.
