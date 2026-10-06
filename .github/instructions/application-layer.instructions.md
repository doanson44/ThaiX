---
description: "Use when writing CQRS commands, queries, handlers, validators, DTOs, or mapping in the Application layer. Covers MediatR pipeline, FluentValidation, caching, and projection rules."
applyTo: "src/ThaiX.Application/**"
---

# Application Layer Rules

## Interface Contracts

All commands and queries use marker interfaces from `ThaiX.Application.Common.Interfaces.Messaging`:

- **`IAppCommand<T>`** — for operations that modify the database (create, update, delete, upsert, import, toggle status, etc.). Use `IAppCommand<Unit>` for void returns.
- **`IAppQuery<T>`** — for read-only operations that only fetch/retrieve data (get, list, search, export, etc.). Never mutates state.

Rule: if the handler calls `SaveChangesAsync`, it MUST be `IAppCommand`. If it only reads, it MUST be `IAppQuery`.

> Exception: Auth flows (login, client credentials, bot command execution) that don't directly write to `IApplicationDbContext` may use `IAppQuery<T>` even when in `Commands/` folders.

## CQRS Commands (Write)

- Sealed record implementing `IAppCommand<T>` or `IAppCommand<Unit>`.
- Import via `using ThaiX.Application.Common.Interfaces.Messaging;`.
- Folder: `Features/{Area}/Commands/{Name}/`.
- Three files: `{Name}Command.cs`, `{Name}CommandHandler.cs`, `{Name}CommandValidator.cs`.
- Handler is a sealed class with constructor injection.
- Commands that don't need to return data use `IAppCommand<Unit>`. Handlers must return `Unit.Value`.
- For `IAppCommand<Unit>`: keep `using MediatR;` for the `Unit` type.
- For `IAppCommand<T>` where T is not `Unit`: drop `using MediatR;` from the command file.
- Use `[InvalidateCache(CacheGroups.X)]` attribute for cache invalidation. Multiple attributes supported.

## CQRS Queries (Read)

- Sealed record implementing `IAppQuery<T>`.
- Import via `using ThaiX.Application.Common.Interfaces.Messaging;`. Do NOT import `MediatR` in query files.
- Folder: `Features/{Area}/Queries/{Name}/`.
- Two files minimum: `{Name}Query.cs`, `{Name}QueryHandler.cs`.
- Return DTOs, never domain entities.
- Implement `ICacheableQuery` for cached queries (CacheKey, Expiration, CacheGroup, IsVersionedList).

## MediatR Pipeline Order

1. LoggingBehavior
2. ValidationBehavior (FluentValidation)
3. QueryCachingBehavior (queries only, cache-aside pattern)
4. CommandCacheInvalidationBehavior (commands only, reads `[InvalidateCache]` attributes)

## No Repository Pattern

- No Repository pattern layered on top of EF Core.
- Handlers access `IApplicationDbContext` directly.

## Query Handler Execution Order (MANDATORY)

1. Build predicate using `PredicateExtensions.True<T>()` with `.And()/.Or()/.Not()`.
2. Start query with `.AsNoTracking()` immediately after DbSet access.
3. Apply `.Where(predicate)`.
4. Apply sorting `.OrderBy()/.ThenBy()`.
5. Apply projection `.Select(e => new Dto { ... })`.
6. Materialize: `.ToPagedListAsync(request, ct)` or `.ToListAsync(ct)` or `.FirstOrDefaultAsync(ct)`.

## AsNoTracking Requirements

- ALWAYS use `.AsNoTracking()` on EF queries that materialize entities.
- `.AsNoTracking()` MUST be placed immediately after `DbSet` access, before any `.Where()`.
- Exception: Queries that project directly to DTOs via `.Select()` do NOT need `.AsNoTracking()`.
- NEVER use `.AsNoTracking()` in Command handlers.

## Data Access Hard Rules

- ALWAYS use `Expression<Func<T, bool>>`, never `Func<T, bool>`.
- NEVER call `.Compile()` on expressions.
- NEVER use `.AsEnumerable()` before filtering/sorting.
- NEVER call `.ToList()` then filter in memory.
- NEVER implement manual Skip/Take/Count -- use `ToPagedListAsync()`.
- ALL filtering/sorting/projection MUST be EF Core translatable to SQL.

## Validation

- FluentValidation only. No data annotations. No manual validation in handlers.
- Validators co-located with commands/queries.
- Use `ErrorCodes` constants for `.WithErrorCode()`.
- Runs in MediatR pipeline before handler execution.

## Mapping

- Explicit mapping via LINQ projection (`Select`) or DTO constructors. NO AutoMapper.
- Direction: Domain/EF -> DTO only.
- No business logic in mappings. No DTO -> Domain mapping (use factory methods).

## Caching

- `ICacheableQuery` on queries: CacheKey, Expiration, CacheGroup. Set `IsVersionedList` for versioned list keys.
- `[InvalidateCache(CacheGroups.X)]` on commands. One attribute per group; multiple allowed.
- Handlers must NOT contain cache logic -- pipeline behaviors handle it.
- Use `CacheKeys.*` and `CacheGroups.*` constants only. Never hardcode.
- Do not cache null responses or mutations.

## Async Locking

- NEVER use raw `SemaphoreSlim` or `lock` for async critical sections.
- Use `AsyncLock` from `ThaiX.Application.Common.Helpers` for all async mutual exclusion.
- Always pass `CancellationToken` to `LockAsync()`.
- Use `using` pattern to guarantee release.
- Key naming convention: `"domain:entity:operation"` (e.g. `"sync:chainbroker:projects"`).

```csharp
using var _ = await AsyncLock.GetLockByKey("sync:chainbroker:projects").LockAsync(cancellationToken);
```
