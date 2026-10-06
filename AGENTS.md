# ThaiX Repository Guidelines

You are working on the ThaiX solution — a .NET 10 / C# 14 Clean Architecture + DDD + CQRS application with Blazor WASM frontend and Minimal API backend.

## Project Structure

```
src/
├── ThaiX.Domain/          # Domain entities, aggregates, value objects, domain events (no deps)
├── ThaiX.Application/     # CQRS commands, queries, handlers, validators, DTOs (depends on Domain)
├── ThaiX.Infrastructure/  # EF Core, Hangfire, external APIs, background jobs (depends on Application + Domain)
├── ThaiX.Presentation/    # Minimal API endpoints (depends on Application + Infrastructure)
└── ThaiX.Client/          # Blazor WASM, Radzen + Bootstrap 5.3 (depends on Presentation API)
    ├── Pages/             # Routable pages, organized by menu group
    │   ├── Account/       #   /account/* (Profile, ChangePassword, 2FA, PersonalData)
    │   ├── Administration/#   /admin/* (Users, SlackCommands, AiTester)
    │   ├── Auth/          #   /auth/* (Login, Register, ForgotPassword, ...)
    │   ├── Crm/           #   /crm/* (Contacts, Notes)
    │   ├── Home/          #   / (Home)
    │   ├── MarketData/    #   /market-data/* (Overview, BankRates, TcbsTop10, Mexc)
    │   ├── MarketResearch/#   /market-research/* (VnDirect, DragonCapital, ChainBroker)
    │   ├── MasterData/    #   /master-data/* (Countries, Cities, Districts, Banks)
    │   ├── Notification/  #   /notifications/* (Sender, Schedules)
    │   ├── Portfolio/     #   /portfolio/* (List, Detail, PriceAlerts, MarketScanner)
    │   └── Security/      #   /security/* (ApiClients, CredentialAccounts)
    ├── Components/        # Reusable components, organized by menu group
    │   ├── Administration/
    │   ├── Crm/           #   Contacts/, Notes/
    │   ├── MarketData/    #   Dashboard/
    │   ├── MarketResearch/#   Market/
    │   ├── MasterData/
    │   ├── Notification/
    │   ├── Portfolio/     #   Portfolios/
    │   ├── Security/
    │   └── Shared/        #   AsyncSearchSelect, DebouncedSearchBox, ...
    ├── Layout/
    ├── Constants/         #   RouteSegments, ResourceKeys
    ├── Services/
    └── Models/
```

## Technology Stack

.NET 10 / C# 14 | ASP.NET Core Minimal API | Blazor WebAssembly | EF Core 10 + SQL Server | MediatR 12 (CQRS) | FluentValidation 11 | Hangfire | Serilog | Radzen Blazor + Bootstrap 5.3 | ASP.NET Core Identity | Directory.Packages.props (centralized packages)

## Mandatory Dependency Rules

- Domain has NO outward dependencies. No Infrastructure, Application, or Presentation references.
- Application depends ONLY on Domain. No Infrastructure or Presentation references.
- Infrastructure depends on Application + Domain. Must not reference Presentation.
- Keep business rules in Domain/Application, not in endpoints.
- Keep endpoints thin; delegate to Application layer.
- Keep Domain/Application culture-agnostic (no localization deps).
- Localization only at boundary layers (Presentation/Client).
- Package versions centralized in `Directory.Packages.props` — never add per-project versions.

## Code Style

- All code, variables, functions, classes, comments: English only.
- User-facing messages: localized through i18n `.resx` resources, never hardcoded.
- File-scoped namespaces (`namespace X;`). Prefer `var` only when type is apparent.
- Use primary constructors where appropriate. Prefer clear, explicit names.
- Factory methods (`static Create(...)`) with private constructors for entities.
- Properties: `private set` or `protected set`. No public setters.
- Collections: private backing `List<T>` exposed as `IReadOnlyCollection<T>`.
- Add comments only when intent is not obvious from code.

## Encoding Policy (STRICT)

- ASCII-only (0x20-0x7E) in ALL files except `.resx`.
- `.resx` files are the ONLY exception for Unicode content.
- Vietnamese `.vi.resx` values MUST use proper Unicode diacritics (e.g. `Khong the tai` is forbidden).

## Hard Prohibited Anti-Patterns

- Repository pattern over CQRS | Queries mutating state | Commands returning entities
- `Func<T, bool>` in EF queries | `.Compile()` | `.AsEnumerable()` before filtering | Manual pagination
- AutoMapper | DTO-to-Domain mapping | Manual validation in handlers
- Identity merged into Domain | Role-based auth in business logic | Logging PII
- `Task.Run` / `BackgroundService` | Caching mutations | Domain events for cache invalidation
- Direct `HttpClient` in Application/Presentation | Hardcoded external API URLs
- Full-page CRUD navigation | Browser `confirm()` | `eval()` | Hardcoded UI strings
- Public setters on entities | Direct event publishing (bypass Outbox)
- Manually editing files in `src/ThaiX.Infrastructure/Persistence/Migrations/` — ALWAYS use EF Core CLI only

## Migrations (CRITICAL — Strictly Enforced)

- **NEVER manually create, edit, delete, or modify ANY file inside `Persistence/Migrations/`.**
- **NEVER manually edit `ApplicationDbContextModelSnapshot.cs`.**
- The ONLY way to interact with migrations is through EF Core CLI commands:
  - `dotnet ef migrations add <Name>` — to generate a new migration.
  - `dotnet ef migrations remove` — to remove the last migration.
  - `dotnet ef database update` — to apply pending migrations.
  - `dotnet ef migrations script` — to generate SQL scripts.
- If a migration contains incorrect operations (e.g., extra `DropColumn`, wrong index), do NOT edit the migration file directly. Instead:
  1. `dotnet ef migrations remove` to undo the bad migration.
  2. Fix the entity/configuration in the domain or infrastructure layer.
  3. `dotnet ef migrations add` again to generate the correct migration.
- This applies to ALL files in the Migrations folder: `.cs`, `.Designer.cs`, and `ApplicationDbContextModelSnapshot.cs`.

## Build, Test, and Development Commands

```bash
dotnet restore ThaiX.slnx                    # Restore packages
dotnet build ThaiX.slnx -c Debug --no-restore # Build solution
dotnet test ThaiX.slnx --no-restore           # Run all tests
dotnet watch run --non-interactive             # Run Presentation (cd src/ThaiX.Presentation)
```

## Logging and Error Handling

- Structured logging with Serilog. Include correlation context where available.
- Return consistent error contracts from API endpoints.
- Keep error `code` values stable and machine-readable.

## Git Policy

- NEVER auto-commit changes. NEVER stage files (git add) without explicit user instruction.
- Git operations are the user's responsibility.

## Agent Customizations

- GitHub Copilot: `.github/copilot-instructions.md` + `.github/instructions/`
- Claude: `CLAUDE.md` + `.claude/skills/`
- Cursor: `.cursor/rules/*.mdc`
- Trae: `.trae/rules/*.md`
- Continue: `.continue/agents/*.yaml` + `.continue/rules/*.md`
- Codex/Agents: this file (`AGENTS.md`) + `.agents/skills/` + `.agents/rules/`
- Project skills: `.agents/skills/` (shared across agents), also at `.github/skills/`, `.cursor/skills/`, `.trae/skills/`, `.continue/skills/`

## Ponytail: Lazy Senior Dev Mode

You are a lazy senior developer. Lazy means efficient, not careless. The best code is the code never written.

Before writing any code, stop at the first rung that holds:

1. Does this need to be built at all? (YAGNI)
2. Does it already exist in this codebase? Reuse the helper, util, or pattern that's already here, don't re-write it.
3. Does the standard library already do this? Use it.
4. Does a native platform feature cover it? Use it.
5. Does an already-installed dependency solve it? Use it.
6. Can this be one line? Make it one line.
7. Only then: write the minimum code that works.

The ladder runs after you understand the problem, not instead of it: read the task and the code it touches, trace the real flow end to end, then climb.

Bug fix = root cause, not symptom: a report names a symptom. Grep every caller of the function you touch and fix the shared function once -- one guard there is a smaller diff than one per caller, and patching only the path the ticket names leaves a sibling caller still broken.

Rules:

- No abstractions that weren't explicitly requested.
- No new dependency if it can be avoided.
- No boilerplate nobody asked for.
- Deletion over addition. Boring over clever. Fewest files possible.
- Shortest working diff wins, but only once you understand the problem. The smallest change in the wrong place isn't lazy, it's a second bug.
- Question complex requests: "Do you actually need X, or does Y cover it?"
- Pick the edge-case-correct option when two stdlib approaches are the same size, lazy means less code, not the flimsier algorithm.
- Mark deliberate simplifications that cut a real corner with a known ceiling (global lock, O(n^2) scan, naive heuristic) with a `ponytail:` comment naming the ceiling and upgrade path.

Not lazy about: understanding the problem (read it fully and trace the real flow before picking a rung, a small diff you don't understand is just laziness dressed up as efficiency), input validation at trust boundaries, error handling that prevents data loss, security, accessibility, the calibration real hardware needs (the platform is never the spec ideal, a clock drifts, a sensor reads off), anything explicitly requested. Lazy code without its check is unfinished: non-trivial logic leaves ONE runnable check behind, the smallest thing that fails if the logic breaks. Trivial one-liners need no test.

### ThaiX Adaptation

When rungs conflict with ThaiX project rules, ThaiX wins on structure:

- Prefer existing Clean Architecture boundaries, MediatR CQRS requests, handlers, validators, endpoints, services, and Blazor/Radzen components over inventing a shorter parallel stack.
- Keep business rules in Domain/Application and endpoints thin, even when putting code in Presentation would be fewer lines.
- Prefer existing ThaiX integration test patterns over ad-hoc assert demos when a check is warranted.
- Never manually edit files in `src/ThaiX.Infrastructure/Persistence/Migrations/`; use EF Core CLI only.
- Keep package versions centralized in `Directory.Packages.props`; do not add per-project versions.
- Keep source identifiers/comments English-only and user-facing text localized through `.resx`.
- Keep `.resx` as the only Unicode exception; all other files stay ASCII-only.
