# Verified Repository Facts

This file records implementation facts verified against the current ThaiX source. Re-check the source before changing these facts; source code remains authoritative if this document becomes stale.

## .NET Toolchain and Target Framework

- The repository pins .NET SDK `11.0.100` through root `global.json`.
- Current application and test projects target `net10.0`; with the SDK default language-version mapping, they compile as C# 14, not C# 15.
- Install the .NET 10 runtime alongside the .NET 11 SDK when executing `net10.0` test applications outside environments that already provide it.
- Do not change project TFMs or claim a .NET 11 framework migration solely because the SDK was upgraded.

## Database Providers

- The current `ApplicationDbContext` registration in `src/ThaiX.Infrastructure/DependencyInjection.cs` uses Oracle's `MySql.EntityFrameworkCore` provider through `UseMySQL(connectionString)`.
- The current EF history repository is `MariaDbHistoryRepository`.
- `DefaultConnection` is the application/Identity database connection.
- `HangfireConnection` is used by Hangfire storage through `UseSqlServerStorage()`; it does not fall back to `DefaultConnection`.
- When Hangfire is enabled, missing `HangfireConnection` causes startup/configuration failure.
- The current presentation integration-test factory uses Testcontainers.MariaDb with MariaDB 11.4, `UseMySQL()`, and `MariaDbHistoryRepository`.
- Do not copy SQL Server/Testcontainers.MsSql setup into current ThaiX integration tests. If a provider migration is requested, update the runtime registration, test factory, packages, migration behavior, and these instructions together.

## MediatR Pipeline

The registration order in `src/ThaiX.Application/DependencyInjection.cs` is significant:

1. `LoggingBehavior`
2. `ValidationBehavior`
3. `TransactionBehavior`
4. `QueryCachingBehavior`
5. `CommandCacheInvalidationBehavior`
6. Request handler

Preserve this order. Do not bypass pipeline behaviors from endpoints or handlers.

## Permission Claims

- Domain permission identifiers are defined by `ThaiX.Domain.Common.Constants.Permissions`.
- Identity user permission claims use `ClaimTypeConstants.Permission` (claim type `permission`).
- The JWT validation event in `src/ThaiX.Presentation/Extensions/ServiceCollectionExtensions.cs` copies each non-empty JWT `scope` value into a corresponding `permission` claim when that permission claim is not already present.
- The Blazor client currently checks `scope` claims in navigation guards and registers policies against the scope claim type. Keep the token's `scope` contract and server-side `permission` claim contract distinct; do not rename either without tracing token issuance, validation, authorization handlers, client policy registration, and tests.
- Endpoints must continue to use `Permissions.*` constants with `.RequireAuthorization(...)`; client navigation guards are not a security boundary.

## Source of Truth

When implementation and documentation disagree, inspect the relevant source and tests. Update this file and the affected skill together when a verified implementation fact changes. Never infer behavior from a similarly named class or from an old prompt.
