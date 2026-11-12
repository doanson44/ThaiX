# Verified ThaiX Runtime Facts

Verified against source on 2026-11-12. Re-check implementation before changing these facts.

## .NET SDK and target framework

- Root `global.json` pins .NET SDK `11.0.100` with `rollForward: latestMinor`.
- Current application and test project files target `net10.0`; this is an SDK update, not a
  framework migration.
- The SDK compiler defaults to C# 14 for `net10.0`. C# 15 is the default for `net11.0` only.
- Build and CI use the .NET 11 SDK, while test execution needs the .NET 10 runtime for
  `net10.0` test hosts. CI installs the .NET 10 SDK as well to provide that runtime; root
  `global.json` still selects SDK 11 for CLI operations.
- The production Dockerfile builds with `mcr.microsoft.com/dotnet/sdk:11.0` and runs with
  `mcr.microsoft.com/dotnet/aspnet:10.0`, matching the current `net10.0` target.
- The visual-test Dockerfile is intentionally isolated from root `global.json` and uses SDK 10
  because its test host runs the `net10.0` Blazor client inside the same container.

## Database providers

- The current central package version for `MySql.EntityFrameworkCore` is `10.0.12`.
- Oracle's `MySql.EntityFrameworkCore 11.0.0` is not currently a stable release; its available
  `11.0.0-preview` package is prerelease and must not be introduced into production without an
  explicit provider migration decision.



- `ApplicationDbContext` is registered with Oracle's `MySql.EntityFrameworkCore` provider via
  `UseMySQL(connectionString)`.
- EF migration history uses `MariaDbHistoryRepository`.
- `DefaultConnection` supplies the application/Identity database.
- Hangfire storage is configured separately with `UseSqlServerStorage(hangfireConnectionString, ...)`.
- When Hangfire is enabled, missing `HangfireConnection` throws; do not assume it falls back to
  `DefaultConnection`.
- Presentation integration tests use `Testcontainers.MariaDb`; the maintained integration skill
  identifies MariaDB 11.4, `UseMySQL()`, and `MariaDbHistoryRepository`.

Evidence: `src/ThaiX.Infrastructure/DependencyInjection.cs`,
`tests/ThaiX.Presentation.IntegrationTests/Infrastructure/ThaiXWebApplicationFactory.cs`,
`.agents/skills/add-integration-test/SKILL.md`.

## MediatR pipeline

Registration order in `src/ThaiX.Application/DependencyInjection.cs`:

1. `LoggingBehavior`
2. `ValidationBehavior`
3. `TransactionBehavior`
4. `QueryCachingBehavior`
5. `CommandCacheInvalidationBehavior`
6. Request handler

Preserve order and do not bypass the pipeline.

## JWT validation and permission claims

Verified in `src/ThaiX.Presentation/Extensions/ServiceCollectionExtensions.cs`:

- Issuer, audience, lifetime, and signing key are validated; clock skew is zero.
- `actor_type` must be `user` or `system`.
- User tokens require `sub` and `email`.
- System tokens are rejected when an email claim exists.
- Non-empty JWT `scope` values are copied into `permission` claims when missing.
- Do not conflate server `permission` claims with client navigation checks that currently read
  `scope`.
- This validation code does not define token issuance lifetimes. Inspect the token issuer and its
  configuration before documenting TTLs.

## Permission constants

- `ThaiX.Domain.Common.Constants.Permissions.GetAll()` reflects public constant string fields.
- `ThaiX.Client.Constants.PermissionNames.GetAll()` is a manually maintained collection.
- When adding client permission constants, also update `PermissionNames.GetAll()` when needed for
  client policy registration.

Evidence: `src/ThaiX.Domain/Common/Constants/Permissions.cs`,
`src/ThaiX.Client/Constants/PermissionNames.cs`.
