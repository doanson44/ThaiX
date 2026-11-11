---
description: "Use when changing the /games pages, game board components, or game scoring in ThaiX.Client. Covers the GameBase architecture, the JavaScript budget, Razor SVG pitfalls, and how to verify."
applyTo: "src/ThaiX.Client/Pages/Games/**,src/ThaiX.Client/Services/Games/**"
---

# Games Rules (/games)

Session memory from the port of the games to Blazor (PR #153). Read this before touching
`src/ThaiX.Client/Pages/Games/`.

## Architecture

- Every game is a Blazor component in `Pages/Games/` that inherits `GameBase` and draws SVG in
  its own `viewBox`. Do not add canvas or JavaScript game code.
- `GameBase` owns the shared lifecycle: `Reset()`, `OnKey`/`OnKeyUp` (lower-cased DOM key names),
  `SwipeStart`/`SwipeEnd` (a swipe becomes an arrow key), `StartLoop(ms, tick)`, `Finish(score, mode)`,
  the pause-aware `ElapsedSeconds`, and `Px()` for culture-invariant SVG numbers.
- `GamePlayer.razor` hosts the board through `DynamicComponent`, forwards keyboard events from the
  focusable `.game-canvas` wrapper, and renders the paused / game-over overlay once for all games.
- `GameCatalog` maps each game id to its component type. Adding a game means: catalog entry,
  component, resource keys in both `.resx` files, and `GameScoreRules` on the server.
- Pure rules live in `Services/Games/GameLogic.cs` and are unit tested in
  `tests/ThaiX.Client.UnitTests/Services/Games/GameLogicTests.cs`.
- Shared board styles are in `GamePlayer.razor.css` behind `::deep` (`.game-svg`, `.g-hit`,
  `.g-cursor`, `.g-muted`, `.typing-*`).

## JavaScript budget

- Allowed: `localStorage` through `IJSRuntime` in `GameScoreStore`, and the keydown guard in
  `wwwroot/js/scrollHelper.js` keyed on `[data-game-keys]`.
- The guard exists because `@onkeydown:preventDefault` applies to every key and would swallow Tab.
- Pointer position is not available without script, so boards use one clickable SVG element per
  cell, and Breakout / Pong use hold-to-move zones instead of following the pointer.

## Razor pitfalls hit during the port

- `<text>` as the first tag of an `@if` / `@for` block is parsed as the Razor text tag (RZ1023).
  Wrap it: `<g><text ...>...</text></g>`.
- Inside a code block write `for` / `if`, not `@for` / `@if` (RZ1010). The `@` is only needed in
  markup context.
- Do not set `fill` or `font-size` on SVG `text` in CSS: a CSS rule overrides the presentation
  attribute on the element. Defaults are inherited from `.game-svg` instead.
- Capture loop variables in a local before using them in an event lambda.
- A board that re-renders on every tick must not create a new lambda per clickable cell: cache the
  delegates once (see `_cellClicks` in `TowerDefenseGame`), or the renderer re-registers every
  handler on every frame.
- Format doubles for SVG with `Px()`; the Vietnamese culture uses a decimal comma.

## Scoring contract

- Client scores must stay valid for `GameScoreRules.IsValidScore`. Lower-is-better games:
  minesweeper, sudoku, memory-cards, 15-puzzle, lights-out, wordle.
- Tower Defense scores are multiples of 10 (kill 10, boss 100), capped at 100000 on both sides.
- A lost Minesweeper board reports 86400 and a failed Wordle reports 7. These still reach local
  history and the leaderboard; changing that needs a server-side decision.

## Verification

- `dotnet build src/ThaiX.Client/ThaiX.Client.csproj`
- `dotnet test tests/ThaiX.Client.UnitTests --filter FullyQualifiedName~Games`
- `dotnet test tests/ThaiX.Client.VisualTests --filter FullyQualifiedName~GamesHubCoverageTests`
- Browser smoke: `GamesSmokeTests` (Playwright). The fixture needs Docker. Without Docker, start
  the client with `dotnet run --launch-profile visual-tests` in `src/ThaiX.Client` and set
  `APP_BASE_URL=http://localhost:5201` and `VISUAL_SKIP_AUTH_LOGIN=true`. The Playwright browser
  must be installed once with `bin/Debug/net10.0/playwright.ps1 install chromium`.
- `GamesSmokeTests` locates the board by the `.game-canvas` class; keep that class on the wrapper.

## Open items from PR #153

- The games were not run in a browser before the PR was opened: the local Playwright run did not
  finish within its time limit and gave no result. Treat browser behavior as unverified until
  `GamesSmokeTests` passes and each game is play-tested on desktop and touch.
- Sudoku has a single fixed puzzle.
- Tower Defense (added after PR #153) was not run in a browser either, and its balance numbers
  (tower cost, damage, range, wave health growth of 1.18 per wave) are reasoned, not play-tested.
- On 2026-11-11 the local `visual-tests` profile served pages that never became idle for any game:
  `snake` and `tower-defense` both timed out in headless Chromium and in Chrome. The cause is not
  diagnosed, so a hang there does not point at the game being opened.
