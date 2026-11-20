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

The inspected Electro Tower implementation damages one selected target and creates a single lightning-beam effect; no multi-target chain-damage loop was found in its Fire method. The C# port therefore models a single-target lightning trace rather than inventing chain damage.

Verified enemy definitions now cover all 13 runtime types. The five infantry types use base
health 50; the bot uses 1,000; vehicle base health ranges from 120 to 550. Metal enemy types
start at core temperature 200 (rather than 35), use 500/0 heat/freeze thresholds, and award
20 credits on kill rather than the infantry default of 10. Their base speed and resistance
modifiers are recorded in FrontlineEnemyCatalog. Every spawned enemy's health is scaled by
10 percent per dispatched wave group. The 11 standard tower base stats, targeting modes, build costs, unlock indexes, and War-Mart
unlock prices are recorded in FrontlineTowerCatalog. Three support tower stat sets and shot
limits are also recorded; their special projectile and finite-shot behavior still requires
porting. The base tower class increments its firing meter by rate-of-fire multiplied by the
simulation scale, fires at 100, and applies upgrades in 20-percent increments at half of its
sale value (which is half of the purchase cost). All 14 campaign and 4 bonus map definitions have been extracted from the Game timeline,
including every PathTile, BlockSquare, StartTile, and EndTile coordinate. The first campaign
level's 24 authored wave groups were initially transcribed, then expanded to all 14 campaign
and 4 bonus wave schedules (963 total groups) in FrontlineWaveCatalog. Level 1 and bonus 1
have route-validation tests; other maps still need endpoint/path consistency verification.
The save schema is partially verified: bPlayed, credits, stFunds, Tower0..Tower19 and
Level0..Level39. FrontlinePlayerProgress now models the legacy 20 tower slots, 40 level slots,
first-time defaults (1,000 credits, 500 starting funds, tower indexes 2, 3, 4 and 13 unlocked,
and level index 0 unlocked), best-score persistence, War-Mart purchases, and conversion to and
from the legacy save record. Legacy `currentLevel` is one-based for score storage (`Level1` for
campaign level 1); unlock checks use the preceding slot (`Level0` unlocks campaign level 1).
Bonus mission IDs 16, 18, 20 and 22 use unlock slots 15, 17, 19 and 21, and score slots 16, 18,
20 and 22. FrontlineGameSession now connects map routes, wave schedules, enemy scaling, the 24 Hz elapsed-time
scale, pause/speed controls, level health, and progression score updates. FrontlinePlayerProgressStore now reads and writes the original localStorage key
fd2:so:FD-2-32-317 and preserves unknown JSON fields while updating bPlayed, credits, stFunds,
Tower0..Tower19, and Level0..Level39. The Blazor host loads this profile and saves it on victory.
Campaign level availability is derived from the previous level's best-score slot; bonus mission
availability is derived from the War-Mart unlock slots.
Do not substitute the separate ThaiX Tower Defense definitions.

## Static route connectivity audit

The current `FrontlineGridPathfinder` requires the destination to be a walkable cell and applies the diagonal corner rule used by the C# implementation. A static audit of the extracted `WalkableCells`, `StartPoints`, and `EndPoints` found:

| Map | Reachable endpoint pairs | Total pairs |
|---|---:|---:|
| level-01 | 1 | 1 |
| level-02 | 0 | 1 |
| level-03 | 0 | 2 |
| level-04 | 0 | 4 |
| level-05 | 0 | 9 |
| level-06 | 0 | 2 |
| level-07 | 0 | 4 |
| level-08 | 0 | 4 |
| level-09 | 0 | 2 |
| level-10 | 0 | 6 |
| level-11 | 0 | 1 |
| level-12 | 0 | 4 |
| level-13 | 0 | 56 |
| level-14 | 0 | 6 |
| bonus-01 | 32 | 32 |
| bonus-02 | 0 | 36 |
| bonus-03 | 0 | 88 |
| bonus-04 | 32 | 88 |

`FrontlineLevelRouteFactory` now filters to reachable authored endpoint pairs before choosing a random route, preventing bonus-04 from selecting one of its unreachable pairs. This does not repair the extracted map data: only level-01, bonus-01, and some bonus-04 endpoint pairs currently produce routes. Campaign levels 2-14 and bonus missions 2-3 remain blocked until the source timeline coordinates or omitted PathTile data are verified. The Sniper Elite host uses bonus-map route data, so bonus missions 2 and 3 currently show the route-unavailable state rather than silently using a fabricated path.

## Porting rules

1. Treat the existing Frontline runtime as the behavioral reference until parity is verified.
2. Extract definitions and formulas into explicit C# data records before implementing them.
3. Keep simulation deterministic and independent of Blazor components.
4. Keep UI state and rendering in Razor/C#; use SVG/HTML elements and CSS, not Canvas.
5. Keep asset identifiers and source asset files stable while the renderer is being replaced.
6. Preserve the existing route and authentication requirement.
7. Preserve the original Frontline storage key and field schema until an explicit migration plan is approved; do not collide with the independent Tower Defense save.
8. Do not delete the old runtime or assets until feature parity and save migration are verified.
9. Never import TowerDefenseMatch, TowerDefenseData, or their gameplay contracts.

## Implementation status

- [x] Create an independent C# simulation boundary and tick entry point.
- [x] Add focused unit tests for placement, combat rewards, leaks, defeat, status effects, and wave timing.
- [x] Port the grid pathfinder's eight-way traversal and diagonal corner rule.
- [x] Convert grid paths into world-space cell-center movement paths.
- [x] Extract all 14 campaign and 4 bonus map grids and their multiple start/end endpoints.
- [x] Transcribe all 14 campaign and 4 bonus wave schedules (963 authored groups).
- [x] Extract all 13 enemy types' base health, speed, metal flag, and observed resistance modifiers.
- [x] Extract all 11 standard tower stats, 3 support tower stats, targeting modes, build prices, unlock indexes, and War-Mart prices.
- [x] Port legacy player progression defaults, best-level score handling, and War-Mart purchase rules.
- [x] Add a session orchestrator connecting authored routes, concurrent wave spawners, enemy scaling, simulation, pause, and speed controls.
- [x] Port buildable-cell validation, grid snapping, tower relocation fee, sale refund, and base upgrades.
- [x] Extract the 24 Hz timer, elapsed-time scale, level health leak rule, and direct-kill score rule.
- [x] Integrate per-level grid maps, random authored endpoints, and per-enemy route generation for route-validated maps.
- [ ] Validate route connectivity for every authored endpoint pair; some extracted levels currently produce no path in the independent pathfinder and need source timeline/coordinate verification.
- [x] Model base tower fire-meter threshold, six target modes, and 20-percent upgrade increments.
- [x] Encode standard tower direct/status damage profiles and metal-specific rifle, sniper, and machine-gun damage.
- [x] Separate direct-kill score awards from damage-over-time kill awards.
- [x] Port limited-use support strike placement, randomized impact zones, area damage payloads, shot limits, and automatic expiration.
- [x] Add localized campaign/bonus mission selection with unlock state and best-score display.
- [x] Add localized War-Mart purchase UI and save progression back to the original Frontline browser key.
- [x] Add standard tower projectile simulation for homing missiles, ballistic shells, tracking bio shots, and tracking flame shots; add splash impacts and transient hitscan/weapon traces.
- [x] Port mission completion score and credit rewards using the original health/funds/score formula.
- [x] Add a separate Sniper Elite session and view with manual target selection, shot cost, firing cadence, metal damage, and hit/miss tracking.
- [ ] Verify Sniper Elite parity and route availability for all four bonus missions; bonus maps 2 and 3 currently have no route in the extracted C# grid data.
- [ ] Verify all projectile collision radii, random launch drift, firing timing, and status payloads against recorded runtime scenarios.

- [x] Implement legacy-compatible browser save/load for the verified player progression schema.
- [x] Replace the active route with a Blazor mission/War-Mart shell, C# game host, and HTML/CSS board renderer.
- [x] Add new Razor/SVG/CSS artwork for all 11 tower archetypes, all 13 enemy archetypes, projectiles, support strikes, impact areas, and weapon traces; include reduced-motion handling.
- [ ] Port or replace original game artwork only after distribution rights are verified; complete remaining animation and interaction parity.
- [ ] Verify gameplay parity and remove the legacy runtime only after acceptance.
- [ ] Clear distribution rights for the original game bundle, bitmap artwork, and audio before release; see docs/games/frontline-tower-defense-release-clearance.md.
- [ ] Run the full solution build and unit test suite on a .NET 11 toolchain before merge.
