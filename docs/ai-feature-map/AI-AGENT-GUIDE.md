# ThaiX AI Agent Guide

## Source precedence
When information conflicts, use this order:
1. Current source code and tests.
2. AGENTS.md.
3. Applicable .agents/rules/.
4. Applicable .agents/skills/.
5. This AI Feature Map.
6. Older prompts, copied examples or external assumptions.

The Feature Map accelerates discovery; it never overrides source or mandatory repository instructions.

## First-read protocol
1. Read root AGENTS.md.
2. Read applicable .agents/rules/ files.
3. Read the matching .agents/skills/<skill>/SKILL.md.
4. Use the Feature Catalog and Traceability Map to locate source.
5. Read affected code and tests; trace the complete flow before choosing a change.
6. Reuse existing helpers, constants, patterns and dependencies.
7. Make the smallest correct change; avoid unrelated refactoring.

## Task-to-skill routing
| Task | Skill |
|---|---|
| Bug fix | .agents/skills/bug-fix/SKILL.md |
| New feature / CRUD | .agents/skills/create-feature/SKILL.md |
| New business module | .agents/skills/new-module/SKILL.md |
| List page | .agents/skills/create-list-page/SKILL.md |
| Form page | .agents/skills/create-form-page/SKILL.md |
| External API provider | .agents/skills/add-external-api/SKILL.md |
| Widget | .agents/skills/add-widget/SKILL.md |
| Integration test | .agents/skills/add-integration-test/SKILL.md |
| Database query | .agents/skills/db-query/SKILL.md |
| EF migration | .agents/skills/ef-migration/SKILL.md |
| Responsive Razor | .agents/skills/responsive-razor/SKILL.md |
| Performance audit | .agents/skills/performance-audit/SKILL.md |
| PR review | .agents/skills/code-review-pr/SKILL.md |

Read the actual skill file before acting. Examples may have feature-specific assumptions.

## Architecture boundaries
- Domain has no outward project dependencies.
- Application depends only on Domain.
- Infrastructure depends on Application and Domain.
- Presentation depends on Application and Infrastructure.
- Client consumes API contracts; do not expose EF/Domain entities to Client.
- Business rules belong in Domain/Application; Minimal API endpoints stay thin.
- Do not add a repository abstraction over EF Core or AutoMapper.
- Do not add direct HttpClient usage in Application/Presentation.
- Do not hardcode external API URLs.
- Do not bypass the MediatR pipeline, Outbox or established external API transport.

## CQRS, EF and caching
- Commands return void or operation identifiers, never Domain entities.
- Queries return DTOs only.
- Follow the current request contract: use IAppQuery<T> where required; otherwise follow the neighboring feature.
- Use FluentValidation, ErrorCodes and colocated validators; do not duplicate validation in handlers.
- Propagate CancellationToken.
- Keep filtering, ordering and projection server-side.
- Do not use Func<T, bool>, Compile(), premature AsEnumerable() or materialize large result sets before filtering/sorting.
- Use existing pagination helpers; do not hand-roll pagination.
- Cache queries through the existing behavior and constants.
- Invalidate cache groups from commands with [InvalidateCache(...)]; do not cache mutations or use domain events for cache invalidation.
- Preserve pipeline order: Logging, Validation, Transaction, QueryCaching, CommandCacheInvalidation, handler.

## Security
- Use server Permissions.* constants in RequireAuthorization(); never hardcode permission strings.
- Add new permission constants to server and client catalogs and Client GetAll() where applicable.
- Client navigation guards are not a server security boundary.
- Identity belongs in Infrastructure; do not merge ApplicationUser into Domain.
- JWT validation checks issuer, audience, lifetime and signing key; clock skew is zero.
- JWT validation requires actor_type user or system. User tokens require sub and email; system tokens must not contain email.
- JWT validation copies non-empty scope values into permission claims when permission claims are absent. Client navigation and policy registration currently read scope. Trace issuance, validation, authorization handlers, Client registration and tests before changing the contract.
- Do not infer token issuance TTL from validation code.
- Never log secrets, tokens, passwords or unnecessary PII.

## Blazor Client
- Follow existing Radzen and Bootstrap patterns and shared components.
- Use server-side paging/filtering/sorting for remote or large datasets.
- Preserve responsive behavior; avoid fixed pixel widths.
- Localize user-facing text through IStringLocalizer<SharedResource>, ResourceKeys and synchronized .resx files.
- Preserve loading, empty, validation and error states.
- Reuse Client API/envelope/cache helpers; do not create parallel API parsing or cache layers.
- Do not show raw upstream technical errors directly to users.

## External APIs
Expected path:
Endpoint -> Application handler -> IExternalDataService -> Infrastructure provider -> ExternalApiService -> HTTP transport/resilience/cache/concurrency/logging -> external API.
Use IOptions<ExternalApisOptions> and ExternalApiService.ExecuteAsync<T>(). Probe the live upstream response and define records from observed shape. Do not invent fields or hardcode URLs.

## Background work and Outbox
- Use Hangfire; do not introduce Task.Run or BackgroundService.
- Preserve Domain event -> Outbox persistence -> Hangfire -> MediatR handler.
- Do not bypass Outbox or use domain events for cache invalidation.
- Keep recurring job configuration aligned with HangfireJobIds, WebApplicationExtensions and job implementations.

## Persistence and migration safety
- Application DB: DefaultConnection, Oracle MySQL EF provider via UseMySQL, MariaDB history repository.
- Hangfire: HangfireConnection, SQL Server via UseSqlServerStorage.
- Integration tests: Testcontainers.MariaDb, MariaDB 11.4.
- Oracle EF Core 11 provider is prerelease; validate actual DB behavior.
- Read-only database diagnostics by default; mask secrets and scope SELECT queries.
- Never apply migrations unless explicitly requested.
- Never manually edit migration files or model snapshot; use EF CLI only.

## Tests and verification
Projects:
- tests/ThaiX.Application.UnitTests/
- tests/ThaiX.Client.UnitTests/
- tests/ThaiX.Client.VisualTests/
- tests/ThaiX.Presentation.IntegrationTests/

Integration tests use WebApplicationFactory and MariaDB 11.4 via Testcontainers.MariaDb. Do not copy SQL Server/Testcontainers.MsSql setup from old examples.

Cover applicable happy path, persisted state, 401/403, 400 validation, 404, domain invariants, conflict and regression cases. Use deterministic test data and repository fixtures. Test naming: {Method}_{Scenario}_{ExpectedResult}.

Run narrow checks first, then broader checks as appropriate:
dotnet --version
dotnet build ThaiX.slnx -c Debug --no-restore
dotnet test ThaiX.slnx --no-restore
dotnet build src/ThaiX.Client/ThaiX.Client.csproj --no-restore

Report exact commands and actual results. Never claim a check was run if it was not. SDK pin is 11.0.100; use root global.json.

## Git and delivery
- For implementation, create a task branch from master before modifying files.
- Use Conventional Commits, English and ASCII-only.
- Inspect final diff before committing.
- When requested, open a PR against master; do not merge unless explicitly requested.
- Do not apply migrations, alter live data, commit to master or force-push unless explicitly requested.

## Definition of done
- Correct layer owns the behavior.
- Existing helpers, constants, patterns and skills are reused.
- API route, permission, correlation ID and response contract remain correct.
- Localization resources are updated for UI text changes.
- Tests cover changed behavior.
- Relevant verification is run and actual results are reported.
- Update this map when routes, permissions, ownership, integrations, data flows or architecture change.
- No unrelated refactoring, new dependency or destructive database action.
