---
title: ThaiX GitHub Skills Index
---

# ThaiX GitHub Skills

Quick index for available project skills in `.github/skills/`.

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
| `bug-fix` | Diagnose and fix defects with minimal changes | bug, exception, regression, unexpected behavior |
| `code-review-pr` | Review diffs against ThaiX architecture/security standards | review PR, code review, staged diff, audit |
| `create-feature` | Generate CRUD feature across all layers | CRUD, entity feature, create module skeleton |
| `create-form-page` | Build responsive admin create/edit forms | form page, create form, edit form, RadzenTemplateForm |
| `create-list-page` | Build responsive admin list/grid pages | list page, grid page, RadzenDataGrid, paging |
| `db-query` | Run direct SQL diagnostics/reporting queries | SQL query, inspect data, database check |
| `ef-migration` | Manage EF Core migration lifecycle | migration, add migration, revert migration, remove migration |
| `implement-spec` | Write an implementation spec and stop for review before coding | implement spec, plan first, spec before code, review plan |
| `new-module` | Build brand-new module across all layers | new module, new business feature, full module |
| `performance-audit` | Audit feature performance bottlenecks | performance, N+1, index, over-fetching, pagination |
| `publish-code` | Package/deploy ThaiX.Presentation via script | publish, deploy, package, webdeploy |
| `remove-page` | Remove a Blazor page and all related artifacts | delete page, merge pages, remove UI page |
| `responsive-razor` | Refactor Razor component responsiveness | responsive UI, mobile layout, viewport, razor fix |

## UI/Design Skills

UI/design skills (brand, banners, slides, frontend-design, ui-ux-pro-max, etc.) are located in `.github/prompts/` following Copilot native prompt format.

## Rule Pairing (Recommended)

- Backend CQRS: pair with `.github/instructions/application-layer.instructions.md`
- Infrastructure data work: pair with `.github/instructions/data-access.instructions.md`
- Domain modeling: pair with `.github/instructions/domain-layer.instructions.md`
- External integration: pair with `.github/instructions/external-api.instructions.md`
- Security/auth: pair with `.github/instructions/security-auth.instructions.md`
- Client UI: pair with `.github/instructions/blazor-ui.instructions.md`
- Tests: pair with `.github/instructions/testing.instructions.md`
