# ThaiX.Client Visual Regression Tests (Layer 2: Playwright)

Playwright suite for `src/ThaiX.Client` (Blazor WASM + Radzen).

**Fixture builds and starts the client in Docker** — no local `dotnet run` / F5 required.

## Flow

```
docker build (Dockerfile) → start container :8080 → Playwright tests → dispose
```

| Piece | Choice | Why |
|-------|--------|-----|
| Orchestration | Testcontainers `ImageFromDockerfileBuilder` | Build + start/stop with the test collection |
| Dockerfile | `tests/ThaiX.Client.VisualTests/Dockerfile` | NuGet restore → LibMan restore → build |
| Runtime | SDK 10 + `dotnet run` DevServer | Fingerprinted Blazor assets rewrite; ForceDemo |
| Not used | plain nginx + publish | `blazor.webassembly.js` 404 without DevServer rewrite |

Docker Desktop required (same as `ThaiX.Tests.API`).

## Prerequisites

```powershell
dotnet tool install --global Microsoft.Playwright.CLI
playwright install chromium
```

## Run (recommended)

```powershell
# From repo root — fixture builds image, starts container, then runs tests
./tests/ThaiX.Client.VisualTests/run.ps1

# Or directly (do NOT set APP_BASE_URL)
dotnet test tests/ThaiX.Client.VisualTests/ThaiX.Client.VisualTests.csproj
```

Update baselines:

```powershell
./tests/ThaiX.Client.VisualTests/run.ps1 -UpdateBaselines -Filter "FullyQualifiedName~VisualRegressionTests"
```

## Optional: attach to a manually started container

```powershell
docker compose -f .docker-composes/docker-compose.client-visualtests.yaml up --build -d
$env:APP_BASE_URL = "http://localhost:5202"
dotnet test tests/ThaiX.Client.VisualTests/ThaiX.Client.VisualTests.csproj
Remove-Item Env:APP_BASE_URL -ErrorAction SilentlyContinue
docker compose -f .docker-composes/docker-compose.client-visualtests.yaml down
```

## Environment variables

| Variable | Default | Meaning |
|----------|---------|---------|
| `APP_BASE_URL` | unset | Skip Docker build/start; attach to this URL |
| `VISUAL_TEST_PARALLELISM` | `min(CPU, 6)` (min 2) | Concurrent Playwright workers (isolated contexts) |
| `ALLOW_REAL_API` | unset | `true` = skip Demo-mode banner check |
| `TEST_USERNAME` / `TEST_PASSWORD` | `admin` / `P@ssw0rd` | Any values work in demo |
| `UPDATE_BASELINES` | unset | Overwrite PNGs under `Baselines/` |

## Overlay coverage (dialogs / confirms / toasts)

`OverlayCatalog` + `OverlayVisualTests` open Radzen overlays via UI actions (add/edit/delete icons, donate, etc.) and assert visibility + dark theme.

| Kind | Examples |
|------|----------|
| Form dialogs | MasterData/CRM/Portfolio/Security create+edit, DonateModal, TradeSuggestion, Resume dialogs |
| Confirms | Delete / regenerate-secret prompts |
| Toasts | Success/error `NotificationService` banners |

`OverlayCatalogCoverageTests` fails if any `DialogService.OpenAsync<T>` component is missing from the catalog.

## Notes

- `RouteCatalog` must cover every `@page` in `src/ThaiX.Client` — enforced by `RouteCatalogCoverageTests` (no Docker).
- `OverlayCatalog` must cover every `DialogService.OpenAsync<T>` — enforced by `OverlayCatalogCoverageTests`.
- Parameterized routes use smoke values (`route-catalog-smoke-test`, `RouteCatalog.SmokeEntityId`).
- Dockerfile runs `dotnet tool restore` + `dotnet tool run libman restore` before `dotnet build`.
- Repo `global.json` (SDK 6) and `Directory.Build.props` (TFM net6) are **not** copied into the image.
- Root `.dockerignore` limits build context to `Directory.Packages.props` + `src/ThaiX.Client`.
- First image build is slow (restore + build); later builds use Docker layer cache.
- `NotificationDesignPreviewTests.cs` is excluded from compile.
- Route/overlay sweeps use `Parallel.ForEachAsync` (`VISUAL_TEST_PARALLELISM` / `run.ps1 -Parallelism N`).

