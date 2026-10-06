---
title: ThaiX GitHub Copilot Prompts Index
---

# GitHub Copilot Prompts

UI/Design prompt bundle for GitHub Copilot following native `.github/prompts/` convention.

## Prompt Catalog

| Prompt | Primary Use | Trigger Keywords |
|---|---|---|
| `banner-design` | Design banners for social, ads, web heroes, and print | banner, cover, header, hero, ad creative |
| `brand` | Define brand voice, identity, assets, and consistency rules | brand, tone of voice, style guide, brand audit |
| `design` | Handle broad design work across UI, brand, slides, banners, and assets | design, logo, banner, slide deck, brand asset |
| `design-system` | Define tokens, component specs, and presentation structure | design tokens, CSS variables, component specs, token system |
| `frontend-design` | Build distinctive frontend UI with strong visual direction | frontend UI, landing page, beautify UI, visual polish |
| `slides` | Create strategic HTML presentations and pitch decks | slides, presentation, pitch deck, chart deck |
| `thaix-frontend-design` | Route ThaiX Blazor UI work to the right frontend skill | ThaiX.Client, Razor, Radzen, localization, responsive |
| `ui-styling` | Build accessible React UI with shadcn/ui and Tailwind | shadcn, Tailwind, theming, dark mode, component styling |
| `ui-ux-pro-max` | Plan and review UI/UX quality across products and screens | UX review, hierarchy, accessibility, interaction design |

## Format

Each prompt follows Copilot native format:
- `PROMPT.md` with frontmatter: `description`, `agent`, `argument-hint`
- Supporting files: `references/`, `data/`, `scripts/`, `templates/`

## Rule Pairing (Recommended)

- Client UI: pair with `.github/instructions/blazor-ui.instructions.md`
- Backend CQRS: pair with `.github/instructions/application-layer.instructions.md`
- Infrastructure data work: pair with `.github/instructions/data-access.instructions.md`
- Domain modeling: pair with `.github/instructions/domain-layer.instructions.md`
- External integration: pair with `.github/instructions/external-api.instructions.md`
- Security/auth: pair with `.github/instructions/security-auth.instructions.md`
- Tests: pair with `.github/instructions/testing.instructions.md`
