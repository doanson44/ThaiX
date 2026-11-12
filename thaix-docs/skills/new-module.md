# New Module Workflow

Build and test with the repository-pinned .NET SDK 11.0.100. Current projects remain on
`net10.0` / C# 14; do not change their target framework as part of SDK maintenance. See
[`SDK-11-TOOLCHAIN.md`](../SDK-11-TOOLCHAIN.md).

Create a complete module across the layers when the requested scope is clear. Do not pause for
confirmation after every layer. Ask only when a material business rule, public contract, destructive
database operation, or architectural decision is genuinely ambiguous. Never apply migrations unless
explicitly requested.

## 1. Inspect and design

- Read `AGENTS.md`, relevant `.agents/rules/`, and the applicable skill.
- Inspect current EF model, entity configurations, migrations, and relevant Domain/Application code.
- Do not assume a separate schema/glossary document exists.
- Reuse existing aggregates, permissions, cache groups, API contracts, and UI patterns.
- Identify the smallest consistent implementation.

## 2. Domain

- Put aggregates under `Domain/Aggregates/{Module}/`.
- Use `BaseAuditableEntity`, private setters, static factory `Create()`, and aggregate methods.
- Raise domain events only for meaningful past-tense domain facts.

## 3. Application

- Add separate command, handler, and validator files for writes.
- Add DTO-only queries and handlers for reads.
- Use FluentValidation, `ErrorCodes`, `ICacheableQuery`, `[InvalidateCache]`,
  `CacheKeys`, and `CacheGroups` according to existing feature patterns.
- Update `IApplicationDbContext` only when a new DbSet is needed.

## 4. Infrastructure

- Add EF configuration under `Persistence/Configurations/`.
- Update `ApplicationDbContext` and DI only as needed.
- Respect the actual configured provider; current app database is MariaDB/MySQL.

## 5. Presentation

- Keep Minimal API endpoints thin and delegate to MediatR.
- Use `Permissions.*`, standard response envelopes, and correlation IDs.

## 6. Client

- Add client DTO/service; do not expose server Domain entities.
- Use server-side paging/filtering/sorting for remote or large collections.
- Add responsive Radzen UI, dialogs/side panels, localization, breadcrumb registration, and
  NavMenu permission guards.

## 7. Tests and documentation

- Add relevant unit and integration tests using the current test fixture (MariaDB 11.4 /
  Testcontainers.MariaDb).
- Build and run the narrowest relevant tests; report actual results.
- Update maintained documentation under `thaix-docs/` when architecture, schema, contracts, or
  operational behavior changes.
- Do not reference or create `knowledge-docs/` unless that structure is deliberately introduced.
- Do not auto-commit unless requested.
