# ThaiX Documentation Audit

- Repository: [`doanson44/ThaiX`](https://github.com/doanson44/ThaiX)
- Baseline: [`master` at `db9eb84c2747fef7aa8216eb4c3e8923f731b7d0`](https://github.com/doanson44/ThaiX/commit/db9eb84c2747fef7aa8216eb4c3e8923f731b7d0)
- Date: 2026-11-12
- Branch: [`docs/audit-and-refresh-thaix-docs`](https://github.com/doanson44/ThaiX/tree/docs/audit-and-refresh-thaix-docs)

## Executive summary

The supplied Markdown set contains multiple material mismatches with the current source. The
highest-risk issues are the incomplete MediatR pipeline, incorrect test/database provider claims,
an obsolete module-generation workflow, and JWT lifetime assertions not established by the
validator implementation.

## .NET SDK 11 toolchain audit

The branch pins SDK `11.0.100` in root `global.json`, while all application and test projects
remain on `net10.0` and C# 14. The first pass found SDK 10 installations in the main GitHub
Actions workflow, production workflow, Azure Pipelines, and production Docker build stage. Those
would conflict with `global.json` because SDK roll-forward does not move from major version 10 to
11. The GitHub Actions workflows, Azure Pipelines, and main Docker build stage have been aligned
to SDK 11; CI installs SDK 10 side-by-side to provide the runtime required to execute
`net10.0` tests. The ASP.NET runtime image remains 10.0.

The isolated visual-test Docker image intentionally remains on SDK 10 and does not copy root
`global.json`; it builds and runs the `net10.0` client in the same container. Its README now records
this exception and corrects the obsolete claim that the repository pins SDK 6 / TFM net6.

No project `TargetFramework`, C# language version, or package major version was changed. The
SDK-only update does not constitute a .NET 11 application migration.

## .NET 11 target-framework migration decision

The target-framework migration was evaluated against the actual ThaiX dependency graph and is
intentionally deferred. All projects currently target `net10.0`; the central package file pins
ASP.NET Core and EF Core 10.0.15 and Oracle `MySql.EntityFrameworkCore` 10.0.12. The application
registers that provider through `UseMySQL()` for its primary database. Although EF Core 11.0.0 is
stable, Oracle's available `MySql.EntityFrameworkCore` 11.0.0 package remains prerelease and the
Connector/NET 26.10 support table describes EF Core 11 support as preview/RC. Retargeting the
solution now would put its primary database provider on a prerelease compatibility path.

Decision: keep all application and test projects on `net10.0`, keep EF Core / ASP.NET Core on the
10.0.x package line, and keep the ASP.NET runtime image on 10.0. Continue using the stable .NET
11.0.100 SDK for build tooling. Revisit the target migration only after a stable Oracle provider
supports EF Core 11 and .NET 11, then review .NET 11 breaking changes and execute the full test,
container, and publish matrix. No project TFM, package version, database schema, or migration file
was changed for this decision.

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


## Follow-up: .NET 11 framework migration

The earlier decision above records the state and risk assessment before the user authorized the migration.
It is superseded by the follow-up implementation on this branch:

- All application and test projects now target `net11.0` (C# 15).
- Microsoft ASP.NET Core, EF Core, and Microsoft.Extensions package references are aligned to
  `11.0.0`.
- Oracle `MySql.EntityFrameworkCore` is set to `11.0.0-preview`; NuGet declares a `net11.0`
  asset group and an EF Core 11 dependency group. This remains a prerelease production dependency.
- GitHub Actions, Azure Pipelines, and both Dockerfiles are aligned to .NET 11.
- No EF migration file, database schema, or live database was changed.

Official provider metadata confirms that the preview package contains a `net11.0` asset group and
requires EF Core 11 preview.7 or later; the Connector/NET 26.10 documentation describes the
provider line as prerelease support. The package graph can therefore be restored for .NET 11, but
runtime behavior against ThaiX's MariaDB deployment still requires user-side verification.
See the [Oracle provider package](https://www.nuget.org/packages/MySql.EntityFrameworkCore/11.0.0-preview)
and [Connector/NET EF Core support matrix](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html).

Build and tests were not run in this editing environment. Treat the migration as implemented but
not verified until restore, build, unit, integration, visual, and publish checks pass.
