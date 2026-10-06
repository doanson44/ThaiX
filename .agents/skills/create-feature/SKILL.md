---
name: create-feature
description: "Generate a full CRUD feature for a domain entity across all layers -- Domain, Application (Commands + Queries + Validators + Handlers), Infrastructure (EF Config), Presentation (Endpoint), and constants (Permissions, CacheKeys). Use when creating a new entity or adding CRUD operations."
argument-hint: "Entity name, e.g. 'Product' or 'Invoice'"
---

# Create Feature

Generate a complete CRUD feature for the specified entity across all architectural layers.

## Procedure

### 1. Domain Entity
- Create in `Domain/Aggregates/{Entity}/` or appropriate aggregate folder
- Inherit `BaseAuditableEntity`
- Private setters, factory `Create()` method, aggregate mutation methods
- Add domain events if needed (past-tense naming)

### 2. EF Core Configuration
- Create `{Entity}Configuration.cs` in `Infrastructure/Persistence/Configurations/`
- Configure properties, indexes, relationships
- Ensure `RowVersion` is configured
- Add soft delete query filter if using `BaseAuditableEntity`

### 3. DbContext
- Add `DbSet<{Entity}>` to `IApplicationDbContext` interface
- Add `DbSet<{Entity}>` to `ApplicationDbContext`

### 4. Permissions and Constants
- Add `{Entity}Read`, `{Entity}Write`, `{Entity}Delete` to `Domain/Common/Constants/Permissions.cs`
- Add cache key constants to `CacheKeys.cs`
- Add cache group to `CacheGroups.cs`
- Also mirror permission constants in `ThaiX.Client/Constants/PermissionNames.cs` AND add them to `PermissionNames.GetAll()` (hardcoded list used for Blazor client policy registration)

> **Admin seed is automatic.** `Permissions.GetAll()` uses reflection to enumerate all `const string` fields in `Permissions.cs`. `DbInitializer.SeedAdminUserAsync()` calls `GetAdminPermissions()` (which calls `GetAll()`) on every startup and adds any missing claims to the admin user. No manual seed code needed — just adding the constant is enough.

### 5. Create Command
- `Features/{Entity}/Commands/Create{Entity}/`
- Three files: `Create{Entity}Command.cs`, `Create{Entity}CommandHandler.cs`, `Create{Entity}CommandValidator.cs`
- Command returns `Guid` (the new entity ID)
- Add `[InvalidateCache(CacheGroups.{Entity})]`
- Validator uses FluentValidation with `ErrorCodes`

### 6. Update Command
- `Features/{Entity}/Commands/Update{Entity}/`
- Same pattern as Create
- Handler loads entity, calls aggregate methods, saves
- Add `[InvalidateCache(CacheGroups.{Entity})]`

### 7. Delete Command
- `Features/{Entity}/Commands/Delete{Entity}/`
- Soft delete via `entity.Delete()` aggregate method
- Add `[InvalidateCache(CacheGroups.{Entity})]`

### 8. List Query
- `Features/{Entity}/Queries/GetAll{Entity}s/`
- Implement `ICacheableQuery` (CacheKey, Expiration, CacheGroup, IsVersionedList=true)
- `AsNoTracking()` -> `Where()` -> `OrderBy()` -> `Select()` -> `ToPagedListAsync()`
- Return `PagedResult<{Entity}Dto>`

### 9. Detail Query
- `Features/{Entity}/Queries/Get{Entity}ById/`
- Implement `ICacheableQuery`
- `AsNoTracking()` -> `Where()` -> `Select()` -> `FirstOrDefaultAsync()`
- Return `{Entity}Dto`

### 10. Minimal API Endpoint
- Create endpoint class in `Presentation/Endpoints/`
- Thin delegation to MediatR
- `.RequireAuthorization(Permissions.{Entity}Read)` etc.
- Register in `MapApiEndpoints()`

### 11. DTOs
- Create `{Entity}Dto.cs` as sealed record in Application layer
- Map via `Select()` projection only
