# Verified ThaiX Runtime Facts

Updated on 2026-11-12 after the .NET 11 framework migration. Re-check implementation before changing these facts.

## .NET SDK and target framework

- Root `global.json` pins .NET SDK `11.0.100` with `rollForward: latestMinor`.
- All application and test projects target `net11.0`.
- The default language version is C# 15.
- GitHub Actions, Azure Pipelines, and the production Docker build/runtime use .NET 11.
- The visual-test Dockerfile uses the .NET 11 SDK/runtime and targets `net11.0`.

## Database providers

- `Directory.Packages.props` pins Microsoft EF Core packages to `11.0.0`.
- Oracle `MySql.EntityFrameworkCore` is pinned to `11.0.0-preview`, a prerelease provider.
- `ApplicationDbContext` is registered with Oracle's provider via `UseMySQL(connectionString)`.
- EF migration history uses `MariaDbHistoryRepository`.
- Presentation integration tests use `Testcontainers.MariaDb` and MariaDB 11.4.
- Provider compatibility with the actual MariaDB/MySQL deployment must be validated before production rollout.

No migration files or database schema were changed as part of the target-framework migration.

## Other runtime facts

- `DefaultConnection` supplies the application/Identity database.
- Hangfire storage is configured separately with `UseSqlServerStorage(hangfireConnectionString, ...)`.
- When Hangfire is enabled, missing `HangfireConnection` throws; do not assume it falls back to `DefaultConnection`.
- MediatR pipeline order: `LoggingBehavior`, `ValidationBehavior`, `TransactionBehavior`, `QueryCachingBehavior`, `CommandCacheInvalidationBehavior`, then handler.
- JWT validation copies non-empty `scope` values into `permission` claims when missing. Do not conflate server `permission` claims with client navigation checks that currently read `scope`.
- JWT validation code does not define token issuance lifetimes. Inspect the token issuer and its configuration before documenting TTLs.

Evidence: `src/ThaiX.Infrastructure/DependencyInjection.cs`, `tests/ThaiX.Presentation.IntegrationTests/Infrastructure/ThaiXWebApplicationFactory.cs`, `src/ThaiX.Application/DependencyInjection.cs`, and `src/ThaiX.Presentation/Extensions/ServiceCollectionExtensions.cs`.
