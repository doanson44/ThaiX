# ThaiX Brand Guidelines

Status: ACTIVE RUNTIME BASELINE
Scope: ThaiX application chrome, shared UI components, and transactional notifications.

## Brand direction

ThaiX uses a dark-finance visual foundation: deep charcoal and navy surfaces, neutral typography, and a restrained amber primary accent. Finance and market screens remain information-dense and analytical. Domain-specific illustrations and data visualizations may use their own palettes without changing the global application identity.

## Primary palette

| Role | Token | Value | Usage |
|---|---|---|---|
| Primary | `--tx-accent` | `#F59E0B` | Primary actions, active navigation, links, selected states |
| Primary light | `--tx-accent-light` | `#FBBF24` | Highlights and hover accents |
| Primary dark | `--tx-accent-dark` | `#D97706` | Hover and active emphasis |
| On primary | `--rz-on-primary` | `#1A1207` | Foreground on amber primary surfaces |
| Navy | `--tx-navy-900` | `#0B1220` | Dark brand surface |
| Deep navy | `--tx-navy-950` | `#020617` | Deep brand surface |

Amber is the global primary. Blue is not the global primary and must not be applied to all application controls, navigation, links, or shared component states.

## Semantic status colors

| Role | Token | Value |
|---|---|---|
| Success / positive | `--tx-positive` | `#10B981` |
| Warning | `--tx-warning` | `#F59E0B` |
| Danger / negative | `--tx-negative` | `#F43F5E` |
| Information | `--tx-info` | `#5B7C99` |

Status semantics remain independent from primary styling even where a status and brand token share a color value. Do not use primary color as a substitute for success, warning, danger, or information meaning.

## Typography and surfaces

- Display and body: IBM Plex Sans.
- Monospace: IBM Plex Mono.
- Dark theme: deep navy/charcoal surfaces with readable neutral foregrounds.
- Light theme: white and warm-neutral surfaces with restrained amber highlights.
- Preserve existing light/dark theme behavior and visible keyboard focus indicators.

## Runtime source of truth

The runtime tokens are maintained in `src/ThaiX.Client/wwwroot/css/app.css`:

1. Primitive values: `--tx-amber-*`, `--tx-blue-*`, semantic colors, spacing, radius, and motion.
2. Semantic aliases: `--tx-accent`, `--tx-positive`, `--tx-warning`, `--tx-negative`, and `--tx-info`.
3. Component/platform mappings: `--tx-button-*`, `--rz-*`, and `--bs-*`.

Use `var(--tx-accent)` for shared primary styling. Keep blue primitives available for domain-specific charts or informational contexts only. Do not add page-local copies of the primary palette.

Cross-channel notification colors are maintained in `src/ThaiX.Application/Common/Models/NotificationBrand.cs`. Keep notification and email accent values aligned with this document.

## Accessibility

- Use dark ink `#1A1207` on the amber primary surface; do not use white text on `#F59E0B`.
- Preserve visible focus indicators and keyboard operation.
- Never communicate state by color alone; retain text, labels, or icons.
- Verify both themes at 375px, 768px, and 1280px widths.

## Governance

- Change this document and runtime token definitions together.
- Update NotificationBrand and visual regression expectations when shared brand colors change.
- Keep semantic status colors and domain-specific artwork separate from global primary.
- Update the canonical skills in `.agents/skills/` and their existing mirrors in `.cursor/skills/` and `.github/prompts/` when brand-token guidance changes.
