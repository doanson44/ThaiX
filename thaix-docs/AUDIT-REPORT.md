# ThaiX Documentation Audit

- Repository: `doanson44/ThaiX`
- Baseline: `master`, commit `db9eb84c2747fef7aa8216eb4c3e8923f731b7d0`
- Date: 2026-11-12
- Branch: `docs/audit-and-refresh-thaix-docs`

## Executive summary

The supplied Markdown set contains multiple material mismatches with the current source. The
highest-risk issues are the incomplete MediatR pipeline, incorrect test/database provider claims,
an obsolete module-generation workflow, and JWT lifetime assertions not established by the
validator implementation.

## Confirmed stale documents

| Supplied document | Finding | Source evidence | Priority |
|---|---|---|---|
| `instruction-application-layer.md` | Omits `TransactionBehavior` from pipeline order. | `src/ThaiX.Application/DependencyInjection.cs` registers Logging, Validation, Transaction, QueryCaching, CommandCacheInvalidation. | High |
| `instruction-testing.md` | Claims integration tests use Testcontainers.MsSql. | `tests/ThaiX.Presentation.IntegrationTests/Infrastructure/ThaiXWebApplicationFactory.cs` imports Testcontainers.MariaDb; maintained repo skill identifies MariaDB 11.4. | High |
| `skill-add-integration-test.md` | Describes SQL Server container. | Current integration factory and `.agents/skills/add-integration-test/SKILL.md` use Testcontainers.MariaDb. | High |
| `skill-db-query.md` | Assumes SQL Server for app DB. | `ApplicationDbContext` uses `UseMySQL()` and `MariaDbHistoryRepository`; Hangfire separately uses `UseSqlServerStorage()`. | High |
| `skill-new-module.md` | Requires a knowledge-docs schema file and pauses after each layer. | Current `.agents/skills/new-module/SKILL.md` directs agents to inspect EF/source and proceed end-to-end when scope is clear. | High |
| `instruction-security-auth.md` | States fixed 30-minute user / 1-hour system JWT lifetimes. | `ServiceCollectionExtensions.AddJwtAuthentication` validates tokens but does not define issuance TTL. | High |
| `skill-ef-migration.md` | Omits current provider context. | Infrastructure DI uses Oracle MySQL EF provider and MariaDB history repository. | Medium |
| `skill-create-list-page.md` | Client-side filtering examples can be mistaken for full-dataset filtering. | Current project conventions require server-side paging/filtering/sorting for remote/large datasets. | Medium |
| `instruction-blazor-ui.md` | Local in-memory lookup examples need scope boundaries. | In-memory filtering is valid only for intentionally loaded complete lookup collections, not one server-paged slice. | Medium |
| `skill-add-widget.md` | Older examples omit current query, envelope, cached client helper, and error-handling conventions. | Current `.agents/skills/add-widget/SKILL.md` uses `IAppQuery<T>`, distinguishes payload state from `ApiResponse<T>`, uses cached envelope helpers, and logs failures. | Medium |

## Important stale documentation already in the repository

The existing `.agents/rules/repository-facts.md` and `.agents/skills/db-query/SKILL.md` claim
Hangfire uses `MySqlConnector` and may fall back to `DefaultConnection`. That claim conflicts with
the current `src/ThaiX.Infrastructure/DependencyInjection.cs`, which configures Hangfire with
`UseSqlServerStorage(hangfireConnectionString, ...)` and throws if `HangfireConnection` is missing
when Hangfire is enabled. The refreshed guide in this folder records the implementation currently
present in source. The original repository instruction files should be updated in a follow-up
synchronization change so agents do not continue reading stale guidance.

## Other supplied documents

The remaining documents were reviewed for broad alignment and retained for future verification:
`instruction-domain-layer.md`, `instruction-data-access.md`, `instruction-external-api.md`,
`skill-add-external-api.md`, `skill-bug-fix.md`, `skill-code-review-pr.md`,
`skill-create-feature.md`, `skill-create-form-page.md`, `skill-performance-audit.md`,
`skill-responsive-razor.md`, `prompt-fix-bug.md`, `prompt-gen-migration.md`,
`prompt-plan.md`, `prompt-refactor.md`, and `prompt-review.md`.

"Reviewed" does not mean every example was compiled or every referenced helper was exhaustively
checked. Verify examples against the target feature before copy/paste.

## Verification scope

Inspected the repository guidelines, current Application DI pipeline, Infrastructure DI/provider
registration, JWT validation extension, permission constants, client permission constants,
integration test factory, and maintained agent skills. No production code, database, migrations,
or runtime configuration was changed. This is a documentation-only refresh.
