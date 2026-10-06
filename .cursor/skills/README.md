---
title: ThaiX Cursor Skills Index
---

# ThaiX Cursor Skills

Quick index for available project skills in `.cursor/skills/`.

## Usage

- Keep skill names in kebab-case.
- Keep `description` in `SKILL.md` focused on:
  - what the skill does
  - when the skill should be used
- Prefer reusing these skills before creating new workflows.

## Skill Catalog

| Skill | Primary Use | Trigger Keywords |
|---|---|---|
| `add-external-api` | Add/extend third-party API integration end-to-end | external API, provider, integration, third-party |
| `add-integration-test` | Add integration tests for Minimal API endpoints | integration test, endpoint test, 401, 403, validation |
| `add-widget` | Build dashboard/market widget across backend and UI | widget, dashboard, market card, data tile |
| `banner-design` | Design banners for social, ads, web heroes, and print | banner, cover, header, hero, ad creative |
| `brand` | Define brand voice, identity, assets, and consistency rules | brand, tone of voice, style guide, brand audit |
| `bug-fix` | Diagnose and fix defects with minimal changes | bug, exception, regression, unexpected behavior |
| `code-review-pr` | Review diffs against ThaiX architecture/security standards | review PR, code review, staged diff, audit |
| `create-feature` | Generate CRUD feature across all layers | CRUD, entity feature, create module skeleton |
| `create-form-page` | Build responsive admin create/edit forms | form page, create form, edit form, RadzenTemplateForm |
| `create-list-page` | Build responsive admin list/grid pages | list page, grid page, RadzenDataGrid, paging |
| `db-query` | Run direct SQL diagnostics/reporting queries | SQL query, inspect data, database check |
| `design` | Handle broad design work across UI, brand, slides, banners, and assets | design, logo, banner, slide deck, brand asset |
| `design-system` | Define tokens, component specs, and presentation structure | design tokens, CSS variables, component specs, token system |
| `ef-migration` | Manage EF Core migration lifecycle | migration, add migration, revert migration, remove migration |
| `frontend-design` | Build distinctive frontend UI with strong visual direction | frontend UI, landing page, beautify UI, visual polish |
| `implement-spec` | Write an implementation spec and stop for review before coding | implement spec, plan first, spec before code, review plan |
| `new-module` | Build brand-new module across all layers | new module, new business feature, full module |
| `performance-audit` | Audit feature performance bottlenecks | performance, N+1, index, over-fetching, pagination |
| `publish-code` | Package/deploy ThaiX.Presentation via script | publish, deploy, package, webdeploy |
| `responsive-razor` | Refactor Razor component responsiveness | responsive UI, mobile layout, viewport, razor fix |
| `slides` | Create strategic HTML presentations and pitch decks | slides, presentation, pitch deck, chart deck |
| `thaix-frontend-design` | Route ThaiX Blazor UI work to the right frontend skill | ThaiX.Client, Razor, Radzen, localization, responsive |
| `ui-styling` | Build accessible React UI with shadcn/ui and Tailwind | shadcn, Tailwind, theming, dark mode, component styling |
| `ui-ux-pro-max` | Plan and review UI/UX quality across products and screens | UX review, hierarchy, accessibility, interaction design |

## Rule Pairing (Recommended)

- Backend CQRS: pair with `.cursor/rules/application-layer.mdc`
- Infrastructure data work: pair with `.cursor/rules/data-access.mdc`
- Domain modeling: pair with `.cursor/rules/domain-layer.mdc`
- External integration: pair with `.cursor/rules/external-api.mdc` and `.cursor/rules/external-api-application.mdc`
- Security/auth: pair with `.cursor/rules/security-auth.mdc` and `.cursor/rules/security-identity-infra.mdc`
- Client UI: pair with `.cursor/rules/blazor-ui.mdc`
- Tests: pair with `.cursor/rules/testing.mdc`
