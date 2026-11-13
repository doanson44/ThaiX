# ThaiX Brand Guidelines

Status: IMPLEMENTED BASELINE
Source: ThaiX visual boards supplied in `THAIX-design.zip`, existing production logo,
and the current ThaiX client/application implementation.

## Brand concept

ThaiX uses a precise, technology-oriented visual identity: blue as the primary brand
signal, deep navy and neutral surfaces for structure, and restrained semantic colors for
status. Finance and market pages should remain dense and analytical. Gaming and other
domains may use their own illustrations without changing the global application identity.

The supplied SVG archive contains visual boards, not a formal token export. Exact source
color values, typography specifications, spacing rules, and component-state definitions
are not machine-readable in the archive. The values below are the operational baseline
inferred from the blue ThaiX mark/boards and existing production assets. They are not
claimed to be an exact Canva token extraction.

## Logo

- Current production mark: `src/ThaiX.Client/wwwroot/favicon.svg`.
- Use the existing mark without stretching, recoloring, or adding effects.
- Keep the mark legible and preserve its aspect ratio.
- The main application header pairs the mark with the localized application title.
- The design archive includes a larger brand board; a replacement mark should only be
  introduced after an individual clean asset is exported and visually verified.

## Quick Reference

| Primary Color | #2563EB |
| Secondary Color | #0B1220 |
| Accent Color | #60A5FA |

### Primary Colors

| Primary Blue | #2563EB |
| Primary Light | #60A5FA |
| Primary Dark | #1D4ED8 |

### Secondary Colors

| Navy | #0B1220 |
| Deep Navy | #020617 |

### Accent Colors

| Sky Blue | #60A5FA |

## Color palette

| Role | Token | Hex | Usage |
|---|---|---|---|
| Primary | `--tx-accent` | `#2563EB` | Primary actions, active navigation, links |
| Primary hover | `--tx-accent-dark` | `#1D4ED8` | Hover and active emphasis |
| Primary light | `--tx-accent-light` | `#60A5FA` | Soft highlights and hover accents |
| Navy | `--tx-navy-950` | `#020617` | Deep brand surface |
| Success | `--tx-positive` | `#10B981` | Successful operations and positive values |
| Warning | `--tx-warning` | `#F59E0B` | Warnings and caution |
| Danger | `--tx-negative` | `#F43F5E` | Errors, destructive states, negative values |
| Info | `--tx-info` | `#5B7C99` | Informational status |

Blue is the brand primary. Amber is retained only as a semantic warning color or
domain-specific illustration color; it is no longer the global primary accent.

## Typography

| Role | Font | Usage |
|---|---|---|
| Display | IBM Plex Sans | Headings, brand text, primary labels |
| Body | IBM Plex Sans | Application content and forms |
| Monospace | IBM Plex Mono | Code, identifiers, tabular/numeric detail |

The fonts are already loaded by `src/ThaiX.Client/wwwroot/index.html`. Do not add a second
font provider or introduce a parallel typography dependency.

## Theme and surfaces

- Support both existing Bootstrap dark and light themes.
- Dark theme uses deep navy/charcoal surfaces with readable neutral foregrounds.
- Light theme uses white/neutral surfaces with blue-tinted highlights.
- Keep success, warning, danger, and information visually distinguishable.
- Do not use a brand color as a substitute for a semantic status color.

## Accessibility

- Primary blue `#2563EB` with white text has a contrast ratio of approximately 5.17:1.
- Use white foreground text on primary blue buttons and controls.
- Use a dark foreground on pale blue surfaces.
- Preserve visible keyboard focus indicators.
- Never communicate state by color alone; retain text, labels, or icons.
- Verify both themes at 375px, 768px, and 1280px widths.

## Runtime source of truth

The runtime token layers are maintained in
`src/ThaiX.Client/wwwroot/css/app.css`:

1. Primitive values: `--tx-blue-*`, semantic color primitives, spacing, radius, motion.
2. Semantic aliases: `--tx-accent`, `--tx-positive`, `--tx-warning`, `--tx-negative`.
3. Component/platform mappings: `--tx-button-*`, `--rz-*`, and `--bs-*`.

Cross-channel notification colors are maintained in
`src/ThaiX.Application/Common/Models/NotificationBrand.cs`. Keep those values aligned with
this document. Do not add page-local copies of the primary palette.

## Governance

- Change this document and the runtime token definitions together.
- Update notification/email brand values when changing shared brand colors.
- Update visual regression expectations when primary tokens change.
- Keep domain-specific art and semantic warning colors separate from global primary.
- Keep source artwork in the design archive; ship only optimized, individually verified
  production assets.
