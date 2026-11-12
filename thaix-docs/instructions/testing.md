# Testing Rules

Use the repository-pinned .NET SDK 11.0.100 from root `global.json`. Test projects currently target
`net10.0`, so the .NET 10 runtime must also be installed to execute them. See
[`SDK-11-TOOLCHAIN.md`](../SDK-11-TOOLCHAIN.md). 

## Organization

- Unit tests: `tests/ThaiX.Application.UnitTests/` using xUnit, Moq, and FluentAssertions.
- Presentation integration tests: `tests/ThaiX.Presentation.IntegrationTests/` using
  `WebApplicationFactory` and `Testcontainers.MariaDb`.
- The maintained integration-test workflow identifies MariaDB 11.4, `UseMySQL()`, and
  `MariaDbHistoryRepository`. Do not copy SQL Server/Testcontainers.MsSql setup unless the runtime
  provider and test fixture are intentionally migrated together.

## Coverage

For each endpoint, cover applicable cases:

1. Happy path and expected persisted state.
2. Unauthenticated (401) and missing permission (403).
3. Validation errors (400).
4. Not found (404) for single-resource operations.
5. Domain/business invariants and conflict behavior.

Do not force irrelevant cases onto endpoints that do not support them.

## Conventions

- Name tests `{Method}_{Scenario}_{ExpectedResult}`.
- Use Arrange/Act/Assert comments.
- Mock `IApplicationDbContext` in unit tests; do not use EF InMemory as a substitute for focused
  unit tests.
- Use deterministic, isolated test data; avoid shared mutable state.
- Add or update tests when behavior changes.

## Verification

Run the narrowest relevant test project/filter first, then broader tests when appropriate. Report
the exact commands and actual results; never claim a test was run if it was not.
