# Frontline Tower Defense C# Port

## Scope

This port replaces only the Frontline Tower Defense JavaScript game runtime. The existing
/games/tower-defense game, its match model, data, progression and leaderboard integration
remain independent and must not be reused as Frontline gameplay.

Target presentation is 100% C# and Blazor rendering. No JavaScript gameplay, Canvas renderer,
or JavaScript game loop is part of the target architecture.

## Source inventory

The branch contains a dedicated Razor wrapper at
src/ThaiX.Client/Pages/Games/FrontlineTowerDefenseGame.razor and the legacy Flump art/audio data
under wwwroot/games/frontline-tower-defense/assets/. The three legacy JavaScript runtime bundles
were removed from this branch; only the audio and pointer browser bridge modules remain.

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
types, and 13 enemy types including infantry, vehicles and a bot. The manual Sniper Tower uses
an authored per-mission origin, charges at RoF 8 until its meter reaches 100, and samples ten
points along the last 45 pixels of the cursor-to-shooter ray at 5-pixel intervals. Its source
hit test uses an 8-pixel enemy radius on both axes (squared threshold 128), with 300 damage and
Burn 2 for infantry, or half damage and Burn 5 for metal targets. Tracking projectile collision
uses the sum of squared projectile and target radii; the source enemy radius is 8 pixels.

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
including every PathTile, BlockSquare, StartTile, and EndTile coordinate. The extracted
WalkableCells and BlockedCells were rechecked against the active root-timeline placements in
assets/library.json after applying each frame's depth removals and placements; all 18 map sets
match exactly. The first campaign level's 24 authored wave groups were initially transcribed,
then expanded to all 14 campaign and 4 bonus wave schedules (963 total groups) in
FrontlineWaveCatalog. The route audit below validates every authored start/end pair.
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

The original `PathFinder.findPath` in `frontline-tower-defense-game.js` uses the active `levelArray`, explores eight directions, and permits a diagonal only when both adjacent orthogonal cells have value 1. `PathTile` sets a cell to 1 and `BlockSquare` sets it to 2. The C# pathfinder uses the same walkability and diagonal-corner rules. The timeline audit applied each root frame's remove/place operations before comparing the active tile sets with the C# catalog; all 18 `WalkableCells` and `BlockedCells` sets match exactly. A static audit of every authored `StartTile`/`EndTile` pair found:

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

`FrontlineLevelRouteFactory` filters to reachable authored endpoint pairs before choosing a random route. The earlier static route audit reported disconnected maps for most campaign levels; that finding was superseded. The maps are placed across multiple timeline frames and the legacy runtime builds them progressively, so a single-frame static extraction undercounted the path cells. Regenerating the catalog from the legacy runtime's live `levelArray` for each level confirmed every campaign and bonus map has at least one connected start/end pair.

## Porting rules

1. Treat the existing Frontline runtime as the behavioral reference until parity is verified.
2. Extract definitions and formulas into explicit C# data records before implementing them.
3. Keep simulation deterministic and independent of Blazor components.
4. Keep UI state and rendering in Razor/C#; use SVG/HTML elements and CSS, not Canvas.
5. Keep asset identifiers and source asset files stable while the renderer is being replaced.
6. Preserve the existing route and authentication requirement.
7. Preserve the original Frontline storage key and field schema until an explicit migration plan is approved; do not collide with the independent Tower Defense save.
8. Keep the original runtime available in the parent branch and Git history for behavioral comparison; do not execute or publish it. Keep the Flump library and media assets stable because the C# renderer still consumes them.
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
- [x] Validate route connectivity for every authored endpoint pair against the active source timeline placements.
- [x] Disable mission-selection actions and show the localized unavailable-route status for maps with no connected authored route.
- [x] Resolve the disconnected authored routes by regenerating the map catalog from the legacy runtime's live level data; all 14 campaign and 4 bonus maps now have at least one connected start/end pair.
- [x] Model base tower fire-meter threshold, six target modes, and 20-percent upgrade increments.
- [x] Encode standard tower direct/status damage profiles and metal-specific rifle, sniper, and machine-gun damage.
- [x] Separate direct-kill score awards from damage-over-time kill awards.
- [x] Port limited-use support strike placement, randomized impact zones, area damage payloads, shot limits, and automatic expiration.
- [x] Match the legacy support-tower tick path: strikes reload and launch independently of enemy targeting or wave-running state.
- [x] Add localized campaign/bonus mission selection with unlock state and best-score display.
- [x] Add localized War-Mart purchase UI and save progression back to the original Frontline browser key.
- [x] Add standard tower projectile simulation for homing missiles, ballistic shells, tracking bio shots, and tracking flame shots; add splash impacts and transient hitscan/weapon traces.
- [x] Preserve tower reload progress while the simulation is in Planning, and verify ballistic splash radius boundaries for Bio Mortar and Plasma Cannon with multi-target tests.
- [x] Prevent towers from firing on zero-elapsed simulation advances, including the paused-session tick path.
- [x] Keep ballistic shells tracking the predicted target during ascent, even when a large elapsed-time step crosses the apex threshold; advance the arc in bounded substeps and stop at the interpolated impact point.
- [x] Match the legacy ballistic lead distances: 40 world units when calculating launch height and 25 world units while steering during ascent.
- [x] Preserve legacy upgrade behavior for Bio Mortar, Bio Cannon, Flame Thrower, and Plasma Cannon, whose firing methods do not apply the base damage upgrade level.
- [x] Preserve an in-flight ballistic shell after its selected enemy dies so the shell still reaches its area-impact point.
- [x] Apply the legacy 50-unit lifetime to Flame Thrower tracking shots so they expire instead of pursuing indefinitely.
- [x] Match source projectile collision radii and lifetimes for Bio Cannon, Flame Thrower, Bio Mortar, and Plasma Cannon.
- [x] Port mission completion score and credit rewards using the original health/funds/score formula.
- [x] Add a separate Sniper Elite session and view with source-style 10-point ray sampling, per-mission shooter origin, manual turret artwork, transient sniper tracer, shot cost, firing cadence, metal damage, and hit/miss tracking.
- [x] Verify Sniper Elite parity and route availability for all four bonus missions: every campaign and bonus map resolves at least one connected authored start/end pair (`FrontlineLevelRouteFactoryTests.TryCreateRandomRoute_EveryLevel_HasAtLeastOneReachableRoute`), the sniper tracer now ends at the sampled hit point instead of the previous sample, the manual turret uses the legacy 3.16 radian divisor, and misses emit the three legacy sparks.
- [x] Verify all projectile collision radii, random launch drift, firing timing, and status payloads against recorded runtime scenarios: missile collision uses its own 6px radius while tracking projectiles use the legacy sum of squared projectile and enemy radii (128), the 80-unit splash and 50 percent damage split of the ballistic shells, the 50-unit flame lifetime, the 500-unit bio lifetime, the 20/20 launch drift ranges, and the per-tick uint32 truncation of the tower firing meter are all pinned by unit tests.

- [x] Implement legacy-compatible browser save/load for the verified player progression schema.
- [x] Replace the active route with a Blazor mission/War-Mart shell, C# game host, and HTML/CSS board renderer.
- [x] Replace hand-drawn board artwork with a C# Flump renderer (`FrontlineFlumpLibrary`) that flattens the legacy `library.json` sprite timelines and draws the original bitmap frames for terrain, all 11 tower archetypes, all 13 enemy archetypes, turret aim frames, and walk-cycle animation.
- [x] Recreate the legacy decalFX path texturing: bitmasked 48px autotiles from the per-level path atlas (forest/sand/rocky/ice), drawn under the level decorations.
- [x] Match the legacy playfield ground color (dark green #445720 from the Game symbol's ground rect), replacing the black placeholder background.
- [x] Render projectiles with original sprites: homing missiles use the Missile symbol aim frames; ballistic shells, tracking poison, and tracking flame use their 10-frame strip atlases.
- [x] Render the HUD with original sprite buttons: tower build buttons (`towerButtonN`/`supTowerButton_N`), pause button, the `testWaveButton` wave-start button, upgrade buttons (`damageButton`/`RoFButton`/`RangeButton`), six-frame upgrade bars (`ugBarD`/`ugBarR`/`ugBarRof`), and the move/sell buttons. Fixed the Flump renderer's `mv` timeline op to swap the instance symbol at a depth slot, which drives the upgrade bar fill and button hover frames.
- [x] Replace the Bootstrap side panel with the original overlaid HUD layout: the 800x480 stage hosts the board plus a right tower-shop panel (bitmap 4680), two-column tower-button slots, bottom-left support-tower slots, top bar stats, bottom health bar, bottom-right wave/pause buttons, and a top-right upgrade panel. All positions are derived from the Game and Hud symbol placements in `library.json`.
- [x] Match the legacy weapon-trace colors (yellow-white tracer, white laser/lightning beam) and render support strikes as falling shell sprites.
- [x] Complete effect parity with the legacy strip atlases: the render-sprite pool (flame, poison, radiation, freeze, blood, muzzle flash, smoke, trail, shell, plasma shell, bio shot, flame shot, bio cloud, shadow, laser blast, blue flash, explosion and fire explosion), the enemy damage-over-time particles every fifth tick, the per-archetype death bursts, the missile impact burst, the support-strike explosion with its fire/poison clouds and cloud particles, the ground decals (blood splat, crater), the tapered multi-width tracer polyline, the jittered lightning beam, the spark hit effects and the selected-tower range ring. Area clouds are no longer drawn as radial gradients; they emit the original cloud sprites.
- [x] Restore the Sniper Elite board contract (manual shooter position/aim, enemy selection and non-buildable cell selection) that the Flump renderer rewrite dropped, and sort towers and enemies by their Y position like the legacy `sortActors` pass.
- [x] Port the legacy `Soundbox`: pooled `HTMLAudio` playback through `FrontlineAudioService` and `wwwroot/js/frontlineAudio.js` covers all 13 weapon/impact sounds, the menu music loop (`85`, volume 0.9), the level noise loop (`84`, volume 0.6), the 0.5 master volume, and the menu-screen mute toggle that drives the `muteButton` frames. The simulation queues cues with `DrainSounds()` because the original spawns a sound per fire, impact, and death.
- [x] Port the six-step instruction manager (`FrontlineTutorialTracker`) placed on the legacy Game timeline: the welcome and placement dialogs have no close button and advance when the player buys and then places a tower, the remaining four dialogs show the `okButton`, after the last one the player must resume the game, and any later step replaces an open dialog exactly like the legacy `inList` chain. Each mission entry restarts the dialogs.
- [x] Port the pause page: `PauseButton` pauses, the `PausePage` overlay draws the original `quitButton`/`continueButton` bitmaps, `continueButton` resumes and hides the page, and `quitButton` zeroes level health so the mission ends as a defeat (which exposes the legacy `retryButton` behavior as a retry action that restarts the same level).
- [x] Allow support strikes on any cell: the legacy `dropTower` path ignores grid occupancy, so strikes can be placed on paths and on top of other towers, and they never mark the cell as occupied.
- [x] Match the legacy enemy selection folds exactly: `near`/`far` always pick the first enemy in range because the original `traveled` counter never advances, `heat` only upgrades its target above 120 degrees, and `cold` only considers metal targets when one is seen first.
- [x] Reproduce the legacy vehicle turn easing (30 percent of the desired heading per tick) and the Jeep1 idle floor frame.
- [x] Replace the enemy health bar with the legacy `healthView` numeric text (white, 8px, right aligned).
- [x] Aim and fire Sniper Elite shots from the exact pointer position through `wwwroot/js/frontlinePointer.js`, replacing cell-based targeting.
- [x] Replace every Razor navigation screen with the original Flump timeline frames: menu, credits, mission select (per-level lock/unlock frames and score text), level failed page, victory rundown, and the War-Mart shop grid, driven by transparent hotspots on the legacy placements.
- [x] Render the original embedded typography everywhere the Flash game showed text, including HTML-format fields, runtime tower prices, the wave countdown, the upgrade panel values, and the instruction dialogs.
- [x] Remove the three legacy JavaScript runtime bundles from this branch. The original files remain
  recoverable from the parent branch and Git history for behavioral comparison. Keep only
  `wwwroot/js/frontlineAudio.js` and `wwwroot/js/frontlinePointer.js` as browser bridges; the
  bitmap, sound and `library.json` assets remain required by the C# renderer.
- [ ] Clear distribution rights for the original game bundle, bitmap artwork, and audio before release; see docs/games/frontline-tower-defense-release-clearance.md.
- [x] Run the full solution build and unit test suite on a .NET 11 toolchain before merge.

### Presentation status

- The whole Frontline user interface renders from the original Flump library: menu (frame 0),
  credits (frame 1), mission select with per-level art and lock frames (frame 2), level failed
  (frame 3), War-Mart (`Mart` symbol with the 15 shop slots, `CRED`, `iName`/`iCost`), and the
  victory rundown (frame 5). Interactive children are transparent HTML hotspots placed on the
  legacy placements, so keyboard and screen-reader access still works.
- The in-game HUD, health bar (`hbar` with the legacy `scaleX = levelHealth * 0.001`), tower
  buttons with their runtime price text and build state alpha, upgrade panel, move/sell buttons
  and the instruction dialogs all render from the original symbols.
- Sniper Elite bonus missions use the same game view, HUD, pause page and result frames; only the
  pointer-to-aim board behaviour and the disabled build panel differ, matching the original.
- Razor text remains only for accessibility labels and screen-reader summaries.

### Flump text support

- `FrontlineFlumpFont` parses the embedded glyph fonts (outline paths in 1024 unit em space).
- `FrontlineFlumpTextLayout` reproduces the legacy layout: word wrap against the field width,
  left/right/center alignment, `leading`, `leftMargin`/`rightMargin`, first-line `indent`, the two
  pixel inset, the `size * 0.3` advance for missing glyphs, and the HTML text parsing the original
  applied to `html` fields.
- `FrontlineFlumpScene` renders any symbol frame as SVG: bitmap draws, bitmap-filled shapes
  (composed artwork such as the menu logo and the HUD panels), vector fills, static text runs and
  editable fields fed by instance-path values, with optional child exclusion so the original
  visibility toggles (frame controller fade, instruction close button, Mart purchase dialog) are
  reproduced.
