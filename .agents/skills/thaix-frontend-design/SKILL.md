---
name: thaix-frontend-design
description: "Routes ThaiX frontend work to the right Blazor/UI skill. Invoke for ThaiX.Client pages, Razor components, Radzen forms/grids, localization, responsiveness, or visual polish."
argument-hint: "[ThaiX.Client page, component, or UI request]"
---

# ThaiX Frontend Design

Use this skill as the entry point for frontend work in this repository when the request targets `ThaiX.Client`, Blazor pages/components, or ThaiX-specific UI conventions.

## Purpose

Route the request to the correct existing skill while enforcing ThaiX UI rules:

- `Radzen` components and layout patterns
- `IStringLocalizer<SharedResource>` with `ResourceKeys`
- `IViewportService` responsive behavior
- mobile/tablet/desktop correctness
- ThaiX page/component folder conventions

If there is any conflict between a generic UI/design skill and ThaiX repository rules, ThaiX rules win.

## Use This Skill When

- Building or editing UI under `ThaiX.Client`
- Creating a new Blazor page or Razor component
- Fixing mobile/tablet layout issues in existing Razor files
- Improving the visual quality of a ThaiX page without breaking Radzen and localization conventions
- Choosing which frontend/design skill should handle a ThaiX request

## Mandatory ThaiX Constraints

Before routing or implementing anything, apply these constraints:

1. All user-facing text must use `IStringLocalizer<SharedResource>` and `ResourceKeys`.
2. All responsive behavior must use `IViewportService` and ThaiX breakpoint conventions.
3. Use `RadzenRow` and `RadzenColumn` with `Size`, `SizeMD`, and `SizeLG` instead of fixed-width layouts.
4. Prefer Bootstrap/Radzen utility classes over inline pixel styles.
5. Keep pages and components in the correct `Pages/*` and `Components/*` group folders.
6. Respect ThaiX CRUD conventions: grid + edit panel/dialog, not full-page admin CRUD navigation.

Reference rules:

- `.trae/rules/blazor-ui.md`
- `.trae/rules/application-layer.md` when UI work also touches CQRS/API contracts

## Routing Guide

Choose the primary sub-skill based on the request.

| Request Type | Primary Skill | Notes |
|---|---|---|
| New admin list/grid page | `create-list-page` | Use for `RadzenDataGrid`, server paging, filtering, edit panel/dialog flows |
| New create/edit admin form | `create-form-page` | Use for `RadzenTemplateForm`, validation, submit handling |
| Existing Razor component is broken on phone/tablet | `responsive-razor` | Use to audit layout anti-patterns and fix breakpoint behavior |
| User wants stronger visual polish or a more distinctive UI | `frontend-design` | Keep ThaiX constraints intact while raising visual quality |
| User wants broader UX direction, hierarchy, accessibility, or interaction review | `ui-ux-pro-max` | Good for design reasoning before or during implementation |
| User wants tokens, component specs, or systematic UI standards | `design-system` | Use for design token or component system work |
| User asks for brand, banner, slide deck, or marketing asset work | `brand`, `banner-design`, `slides`, or `design` | Usually outside core Blazor admin UI |

## Mixed Requests

When the request spans multiple concerns, route in this order:

1. Structural page/component generation first: `create-list-page`, `create-form-page`, or `responsive-razor`
2. UX and visual refinement second: `ui-ux-pro-max` and/or `frontend-design`
3. Brand/asset work last: `brand`, `banner-design`, `slides`, or `design`

Do not start from a generic design skill when the user clearly needs a ThaiX Blazor page pattern.

## Delivery Checklist

Before finishing any ThaiX frontend task, confirm all items below:

- No hardcoded user-facing text remains in `.razor`
- No fixed pixel widths create horizontal scroll on mobile
- `RadzenColumn` breakpoints are present where layout can split
- `IViewportService` lifecycle is correct when breakpoint-based rendering is used
- Routes, breadcrumb labels, and menu placement are updated when a new page is created
- Localization resources and `ResourceKeys` are added when new UI text is introduced
- **Quality gate**: Run the [10K Checklist](references/10k-checklist.md) (Metics Media Field Guide) — 8-point bar for premium web quality

## Examples

- "Create a new admin positions list page in ThaiX" -> start with `create-list-page`
- "Make this Razor widget usable on mobile" -> start with `responsive-razor`
- "This ThaiX page works but looks bland; improve the UX" -> use `thaix-frontend-design`, then route to `ui-ux-pro-max` and `frontend-design`
