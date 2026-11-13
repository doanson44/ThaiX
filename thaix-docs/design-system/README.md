# ThaiX Design System

Status: FOUNDATION IMPLEMENTED; FULL VISUAL ROLLOUT PENDING
Source: user-provided `THAIX-design.zip` (32 SVG artboards)
Target: `src/ThaiX.Client` (Blazor WebAssembly, Radzen, Bootstrap 5.3)

## Purpose

Establish one recognizable ThaiX visual identity across the application while allowing
domain-specific iconography for Finance, Markets, Enterprise, AI, Data, Blockchain,
Gaming, Media, Education, Security, and Commerce.

The design archive is a set of visual reference boards. It is not a ready-to-use icon
font, SVG sprite, component library, or token export. The SVG files contain complete
artboards with embedded image elements and vector paths. They must not be rendered as
large page backgrounds or copied wholesale into the runtime asset directory.

## Source-of-truth hierarchy

1. Product behavior, accessibility, authorization, and localization remain governed by
   the existing ThaiX source code and repository instructions.
2. This document records how the supplied design boards are interpreted for the app.
3. The design boards provide visual direction. They do not override functional behavior
   or existing application contracts.
4. Where the boards do not define a value, use the existing Bootstrap/Radzen system and
   the tokens below. Do not invent a separate component framework.

## Brand direction

- Brand mark: existing ThaiX mark in `src/ThaiX.Client/wwwroot/favicon.svg`.
- Primary identity: blue with deep navy and neutral surfaces.
- Typography: IBM Plex Sans for display and body; IBM Plex Mono for code and numeric
  data. Keep tabular/numeric information legible.
- Semantic status colors: green for positive/success, amber for warning, rose for
  negative/danger, and desaturated steel blue for informational states.
- Finance, market, and analytics screens should remain information-dense and restrained.
- Gaming screens may use the supplied Tower Defense artwork as domain-specific content;
  they must not change the global application chrome or semantic status colors.

## Runtime token baseline

These values are an implementation baseline inferred from the visual boards and the
existing ThaiX logo, not an exact token export from Canva:

| Token | Value | Usage |
|---|---|---|
| `--tx-accent` | `#2563EB` | Brand primary |
| `--tx-accent-light` | `#60A5FA` | Hover/highlight |
| `--tx-accent-dark` | `#1D4ED8` | Active emphasis |
| `--tx-positive` | `#10B981` | Success/positive |
| `--tx-negative` | `#F43F5E` | Error/negative |
| `--rz-info` | `#5B7C99` | Informational status |
| `--tx-font-display` | IBM Plex Sans | Headings and brand text |
| `--tx-font-body` | IBM Plex Sans | Application content |
| `--tx-font-mono` | IBM Plex Mono | Code and numeric detail |

The canonical brand reference is `docs/brand-guidelines.md`. The runtime stylesheet
`src/ThaiX.Client/wwwroot/css/app.css` implements primitive, semantic, and component token
layers, then maps the same ThaiX accent to Bootstrap and Radzen. Do not define a second
primary palette in individual pages.

## Implementation rules

- Use existing Radzen components and Bootstrap 5.3 utilities; do not add a new UI library.
- Use `var(--tx-*)`, `var(--rz-*)`, and `var(--bs-*)` rather than page-local brand colors.
- Keep light and dark themes readable. Do not assume the artwork's dark background is a
  requirement for every page.
- Reuse the existing ThaiX brand mark; do not duplicate a hand-drawn logo in Razor.
- Use Radzen/Material icon names already supported by the installed Radzen version for
  navigation and controls. Use domain-specific artwork only where an actual illustration
  improves comprehension.
- Keep icon meaning consistent: navigation icons identify a module; status icons identify
  state; action icons identify an operation.
- Keep all UI text localized through the existing resource system.
- Preserve permission-based navigation, keyboard focus, accessible names, and responsive
  layouts at 375px, 768px, and 1280px.
- Do not add full-page SVG artboards to normal page content. Extract or recreate individual
  assets only after checking their license, transparency, dimensions, and rendering quality.

## Current integration

- `docs/brand-guidelines.md` records the operational brand baseline and its inferred values.
- Global Radzen and Bootstrap primary colors now use the ThaiX blue identity.
- Existing semantic success, danger, and informational colors remain distinct.
- Display typography now uses the existing IBM Plex Sans family instead of a serif face.
- The main application header and authentication layout display the updated ThaiX X-mark
  favicon beside the localized/application title.
- The navigation, profile menu, shell controls, shared responsive action buttons, and selected
  page-level icons use the ThaiX icon library.
- Existing Radzen navigation icons remain the default for generic controls. A shared
  `ThaiXIcon` component and 32 currentColor glyphs are available for ThaiX-specific
  visual identity; the extracted set is cataloged in `ICON-CATALOG.md`.
- Shared component states and responsive rules are recorded in `COMPONENTS.md`.

## Known gaps before calling this a complete design system

The archive does not provide machine-readable design tokens, individual icon exports,
component specifications, spacing scale, typography scale, grid rules, elevation/shadow
specifications, or interaction-state definitions. Those values cannot be verified from
the archive alone. This implementation therefore establishes a coherent first layer,
not a claim that every screen has been visually redesigned.

Next design-system increments should be delivered in this order:

1. Confirm exact brand colors and typography from the source design file.
2. Export individual transparent SVG/PNG icons for the icons actually used by ThaiX.
3. Define component states and reusable patterns for buttons, forms, cards, tables,
   dialogs, alerts, and navigation.
4. Apply and visually verify the shared patterns across existing pages.
5. Add or update Playwright visual baselines for dark/light themes and 375/768/1280 widths.
