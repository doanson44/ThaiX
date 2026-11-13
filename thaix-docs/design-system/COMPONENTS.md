# ThaiX UI Component Standards

Status: BASELINE SPECIFICATION
Runtime: Blazor WebAssembly, Radzen Blazor, Bootstrap 5.3

This document defines shared visual behavior for existing ThaiX controls. It does not
authorize replacing Radzen components or changing the application's functional contracts.

## Global rules

- Use existing Radzen and Bootstrap components.
- Source all brand colors from `wwwroot/css/app.css` tokens.
- Keep dark and light themes functional.
- Localize all user-facing text through the existing resource system.
- Preserve keyboard operation, visible focus, labels, validation feedback, and ARIA state.
- Do not rely on color alone to communicate status.
- Keep responsive layouts valid at 375px, 768px, and 1280px.

## Component baseline

| Component | Default behavior | Interactive states | Responsive behavior |
|---|---|---|---|
| Primary button | ThaiX blue background, white text | darker blue hover, visible focus, disabled/loading feedback | full-width only when form layout requires it |
| Secondary button | neutral surface and foreground | neutral hover, visible focus | preserve touch target |
| Destructive action | semantic danger color | explicit confirmation for destructive operations | keep action reachable without overflow |
| Text input/select | theme surface, neutral border | blue focus ring, semantic error border, disabled state | fluid width; labels remain visible |
| Card | theme surface, subtle border | hover elevation only when interactive | stack on mobile; columns at larger widths |
| Badge | compact semantic label | success/warning/danger/info variants | wrap rather than force horizontal overflow |
| Alert | semantic icon, title/message | dismissible only where the workflow permits | content wraps within viewport |
| Dialog | Radzen DialogService | focus remains usable; loading and validation visible | fit mobile viewport and scroll internally |
| Data grid | existing server-side paging/filter/sort | loading, empty, error, selected states | card/list fallback on mobile where needed |
| Navigation | Radzen PanelMenu | active indicator uses ThaiX blue | overlay drawer on small screens |

## State semantics

| State | Color role | Required non-color cue |
|---|---|---|
| Success | `--tx-positive` | success label or check icon |
| Warning | `--tx-warning` | warning label or alert icon |
| Error/destructive | `--tx-negative` | error text or alert icon |
| Information | `--tx-info` | info label or info icon |
| Focus | `--tx-focus-ring` | visible outline/ring |
| Disabled | neutral/muted | disabled attribute and disabled semantics |
| Loading | neutral with primary indicator | spinner and disabled/busy state |

## Spacing and shape

Use the 4px-base spacing tokens already defined in the runtime stylesheet:

- `--tx-space-1` through `--tx-space-8` for consistent spacing.
- `--tx-radius-sm`, `--tx-radius-md`, and `--tx-radius-lg` for shape.
- `--tx-duration-fast` and `--tx-duration-normal` for transitions.

Prefer existing Radzen spacing utilities and Bootstrap spacing utilities before adding
new page-specific CSS.

## Not yet visually audited

The design archive does not specify exact button heights, card shadows, table row heights,
dialog dimensions, or complete component anatomy. These values should be calibrated against
the rendered application and its existing Radzen defaults, not guessed from the icon boards.
