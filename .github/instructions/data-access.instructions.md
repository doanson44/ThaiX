---
description: "Use when writing EF Core configurations, DbContext changes, migrations, interceptors, or infrastructure data access code. Covers EF Core conventions and Hangfire/Outbox patterns."
applyTo: "src/ThaiX.Infrastructure/**"
---

# Infrastructure Data Access Rules

## EF Core

- All data access through `ApplicationDbContext` implementing `IApplicationDbContext`.
- Entity configurations in `Persistence/Configurations/`.
- Interceptors handle audit fields and domain event persistence.

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

## Background Processing

- Hangfire only. No `Task.Run` or `BackgroundService` for background work.

## Outbox Pattern

1. Domain raises event via `BaseEntity.AddDomainEvent(IDomainEvent)`.
2. `OutboxInterceptor` persists events to `OutboxMessages` table on `SaveChangesAsync`.
3. Hangfire recurring job processes every 30 seconds.
4. Events published via MediatR to domain event handlers.

## Identity

- ASP.NET Core Identity in Infrastructure only.
- `ApplicationUser : IdentityUser<Guid>` -- no business fields.
- Identity and Domain models NEVER merged.

## Caching Implementation

- `ICacheService` interface defined in Application; `MemoryCacheService` implemented here.
- Swappable to Redis by adding new implementation only.

## Prohibited

- No caching commands or mutations.
- No domain events for cache invalidation.
- No direct `HttpClient` in Application or Presentation.
- No hardcoded external API URLs.
