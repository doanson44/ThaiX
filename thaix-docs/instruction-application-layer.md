# Application Layer Rules

## CQRS

- Commands are sealed records implementing `IRequest<T>` or `IRequest`; return void or operation
  identifiers, never Domain entities.
- Queries return DTOs only. Use `IAppQuery<T>` for read-only queries where the current messaging
  contract requires it; otherwise follow the established feature pattern.
- Keep handlers in Application and access EF through `IApplicationDbContext`; do not add a
  repository layer over EF Core.
- Keep validators colocated and use FluentValidation and `ErrorCodes`; do not duplicate validation
  manually in handlers.
- Pass `CancellationToken` through asynchronous calls.

## MediatR pipeline (order is significant)

Verified in `src/ThaiX.Application/DependencyInjection.cs`:

1. `LoggingBehavior`
2. `ValidationBehavior`
3. `TransactionBehavior`
4. `QueryCachingBehavior`
5. `CommandCacheInvalidationBehavior`
6. Request handler

Do not reorder or bypass these behaviors.

## EF query shape

For entity-materializing read queries:

1. Build predicates with the existing `PredicateExtensions` helpers where appropriate.
2. Start from the DbSet and apply `.AsNoTracking()`.
3. Apply `.Where()`, ordering, then `.Select()` projection.
4. Materialize with the existing `ToPagedListAsync()`, `ToListAsync()`, or
   `FirstOrDefaultAsync()` helper as appropriate.

A direct DTO/scalar projection does not need `AsNoTracking()` because no entity instance is
materialized. Never use `Func<T,bool>`, `.Compile()`, `.AsEnumerable()` before filtering, or
materialize a large result before filtering/sorting. Do not hand-roll pagination.

## Validation and mapping

- FluentValidation only; use `ErrorCodes` with `.WithErrorCode()`.
- Map Domain/EF to DTO using explicit LINQ projection or constructors.
- No AutoMapper and no DTO-to-Domain mapping; use aggregate factories/methods.

## Caching

- Cache queries only through `ICacheableQuery` and the established cache pipeline.
- Use `CacheKeys.*` and `CacheGroups.*`.
- Invalidate related cache groups on commands with `[InvalidateCache(...)]`.
- Do not put cache logic in handlers, cache mutations, or use domain events for cache invalidation.

## Async locking

Use the existing `AsyncLock` helper for asynchronous mutual exclusion. Do not use raw
`lock` or `SemaphoreSlim` for async critical sections. Pass cancellation tokens and dispose the
lock handle with `using`.
