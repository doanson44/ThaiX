---
name: new-module
description: "Create a full ThaiX business module across Domain, Application, Infrastructure, Presentation, Client, and tests. Use when the feature does not already exist."
argument-hint: "Module name, e.g. 'Invoice' or 'Inventory'"
---

# New Module Flow

Create a complete module across all architectural layers with verification between steps.

Do not pause for confirmation after each layer. Complete the end-to-end workflow when the scope is clear. Ask only when a material business rule, public contract, destructive database operation, or architectural decision is genuinely ambiguous. Never run `dotnet ef database update` unless explicitly requested.

## Procedure

### Step 1 -- Design
- Inspect the current EF model, entity configurations, migrations, and relevant domain/application code; do not assume a separate schema document exists.
- Identify existing aggregates, entity fields, relationships, permissions, cache groups, and API contracts before proposing additions.
- Output: entity diagram + field list
- Ask for a decision only when a material business rule, public contract, or architectural choice is genuinely ambiguous; otherwise proceed with the smallest consistent implementation.

### Step 2 -- Domain Layer
- Create entity in `Domain/Aggregates/{Module}/`
- `BaseAuditableEntity`, private setters, factory `Create()`, aggregate methods
- Add domain events if needed
- Add permission constants to `Permissions.cs`

### Step 3 -- Application Layer
- Create Commands (Create, Update, Delete) + Handlers + Validators
- Create Queries (List, Detail) + Handlers
- Add `[InvalidateCache]` on commands, `ICacheableQuery` on queries
- Add `CacheKeys` and `CacheGroups` constants
- Add DTOs (sealed records)
- Update `IApplicationDbContext` with new DbSet

### Step 4 -- Infrastructure Layer
- Create EF Core entity configuration
- Add DbSet to `ApplicationDbContext`
- Register any new services in `DependencyInjection.cs`

### Step 5 -- Presentation Layer
- Create Minimal API endpoint class
- Register endpoints in `MapApiEndpoints()`
- Authorization with `Permissions.*` constants

### Step 6 -- Client (Blazor WASM)
- Create API service class
- Create list page with `RadzenDataGrid` (server-side paging)
- For mobile list/card view, preserve the same server-side paging/filtering/sorting contract; use `DebouncedSearchBox` (300ms) to submit search terms to the API.
- Create create/edit form (side panel or dialog)
- Add route and navigation
- Update `Layout/NavMenu.razor` to include new module pages with localized labels
- Update breadcrumb: `RouteSegments`, `ResourceKeys`, `GetSegmentLabel()`

### Step 7 -- Tests
- Unit tests for command handlers
- Unit tests for query handlers
- Run `dotnet test tests/ThaiX.Application.UnitTests`

### Step 8 -- Documentation
- Update maintained schema/domain documentation when it exists. Do not reference or create `knowledge-docs/` files unless that documentation structure has been added to the repository.
- Run relevant builds and tests, report actual results, and follow the repository Git workflow when a commit or PR is requested.
