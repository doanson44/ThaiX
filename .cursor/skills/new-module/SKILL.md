---
name: new-module
description: "Create a complete business module across Domain, Application, Infrastructure, Presentation, Client UI, and Tests with staged execution. Use when building a brand-new feature area."
argument-hint: "Module name, e.g. 'Invoice' or 'Inventory'"
---

# New Module Flow

Create a complete module across all architectural layers with verification between steps.

## Procedure

### Step 1 -- Design
- Read current schema from `knowledge-docs/03-db-schema.md`
- Propose: entity fields, relationships, permissions needed
- Output: entity diagram + field list
- **Wait for user confirmation**

### Step 2 -- Domain Layer
- Create entity in `Domain/Aggregates/{Module}/`
- `BaseAuditableEntity`, private setters, factory `Create()`, aggregate methods
- Add domain events if needed
- Add permission constants to `Permissions.cs`
- **Wait for confirmation**

### Step 3 -- Application Layer
- Create Commands (Create, Update, Delete) + Handlers + Validators
- Create Queries (List, Detail) + Handlers
- Add `[InvalidateCache]` on commands, `ICacheableQuery` on queries
- Add `CacheKeys` and `CacheGroups` constants
- Add DTOs (sealed records)
- Update `IApplicationDbContext` with new DbSet
- **Wait for confirmation**

### Step 4 -- Infrastructure Layer
- Create EF Core entity configuration
- Add DbSet to `ApplicationDbContext`
- Register any new services in `DependencyInjection.cs`
- **Wait for confirmation**

### Step 5 -- Presentation Layer
- Create Minimal API endpoint class
- Register endpoints in `MapApiEndpoints()`
- Authorization with `Permissions.*` constants
- **Wait for confirmation**

### Step 6 -- Client (Blazor WASM)
- Create API service class
- Create list page with `RadzenDataGrid` (server-side paging)
- For mobile list/card view, add `DebouncedSearchBox` (300ms) and render from filtered collection
- Create create/edit form (side panel or dialog)
- Add route and navigation
- Update `Layout/NavMenu.razor` to include new module pages with localized labels
- Update breadcrumb: `RouteSegments`, `ResourceKeys`, `GetSegmentLabel()`
- **Wait for confirmation**

### Step 7 -- Tests
- Unit tests for command handlers
- Unit tests for query handlers
- Run `dotnet test tests/ThaiX.Application.UnitTests`
- **Wait for confirmation**

### Step 8 -- Documentation
- Update `knowledge-docs/03-db-schema.md` with new tables
- Update `knowledge-docs/05-domain-glossary.md` with new terms
- Report status -- DO NOT auto-commit
