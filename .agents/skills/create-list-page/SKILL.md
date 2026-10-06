---
name: create-list-page
description: "Generate a Blazor WASM admin list page with RadzenDataGrid, server-side paging, filtering, responsive layout, and DebouncedSearchBox for mobile. Use when creating a new admin data grid page."
argument-hint: "Entity name, e.g. 'Product'"
---

# Create List Page

Generate a Blazor WASM admin list page following ThaiX conventions.

## Procedure

### 1. Create Page Component
- File: `Pages/{Area}/{Entity}List.razor`
- Add `@page` directive with route
- Add `@layout MainLayout` (default)

### 2. RadzenDataGrid Setup
```razor
<RadzenDataGrid TItem="{Entity}Dto"
    Data="@_items"
    Count="@_totalCount"
    LoadData="@LoadDataAsync"
    AllowFiltering="true"
    FilterMode="FilterMode.Advanced"
    AllowSorting="true"
    AllowPaging="true"
    ShowPagingSummary="true"
    PageSizeOptions="@_pageSizeOptions"
    PageSize="@_pageSize"
    IsLoading="@_isLoading">
```

### 3. Server-Side Paging
- Implement `LoadDataAsync(LoadDataArgs args)` method
- Call API service with paging/filter/sort parameters
- Update `_items` and `_totalCount`
- Set `_isLoading` flag during load

### 4. Edit Panel (Responsive)
- Desktop (>=1200px): Side panel alongside grid
- Tablet (768-1199px): `RadzenDialog`
- Mobile (<768px): Full-width `RadzenDialog`
- Row click opens edit

### 4.1 Mobile Search (AsyncSearchSelect or DebouncedSearchBox)
- For mobile list/card views, prefer `AsyncSearchSelect` from `ThaiX.Client.Shared`; fallback to `DebouncedSearchBox` if only simple text search is needed.
- Bind `Value` and `ValueChanged`:
  - `AsyncSearchSelect`: `_selectedItemValue`, `OnSelectedItemChanged`.
  - `DebouncedSearchBox`: `_mobileSearchTerm`, `OnMobileSearchChanged`.
- Use `SearchPlaceholder` and `Paging` in `AsyncSearchSelect` for large datasets.
- Keep page data sync:
  - `Selected` filter updates `FilteredMobileItems`.
  - `All` reset when empty.

AsyncSearchSelect example:
```razor
<AsyncSearchSelect TItem="EntityDto"
                   Value="@_selectedEntityId"
                   ValueChanged="OnSelectedEntityChanged"
                   LoadDataAsync="LoadEntitiesAsync"
                   GetText="@(x => x.Name)"
                   GetValue="@(x => x.Id)"
                   Placeholder="@L[ResourceKeys.Common.Select]"
                   SearchPlaceholder="@L[ResourceKeys.Common.Search]"
                   LoadingText="@L[ResourceKeys.Common.Loading]"
                   EmptyText="@L[ResourceKeys.Common.NoDataFound]"
                   MoreHint="@L[ResourceKeys.MasterData.SelectScrollMore]"
                   AllowClear="true"
                   Class="w-100" />
```
```csharp
private string? _selectedEntityId;
private List<EntityDto> _filteredItems = [];

private Task OnSelectedEntityChanged(string? value)
{
    _selectedEntityId = value;
    _filteredItems = string.IsNullOrWhiteSpace(_selectedEntityId)
        ? _items
        : _items.Where(x => x.Id == _selectedEntityId).ToList();
    return Task.CompletedTask;
}

private Task<(IReadOnlyList<EntityDto>, int)> LoadEntitiesAsync(int page, int pageSize, string? search, CancellationToken cancellationToken)
{
    // Implement server-side filtering/paging, or local filter on _items.
    // Return items + total count.
}
```

DebouncedSearchBox fallback example:
```razor
<DebouncedSearchBox Value="@_mobileSearchTerm"
                    ValueChanged="OnMobileSearchChanged"
                    Placeholder="@L[ResourceKeys.Common.Search]"
                    Name="EntitySearchMobile"
                    CssClass="w-100 mb-3"
                    DebounceMilliseconds="300" />
```
```csharp
private string _mobileSearchTerm = string.Empty;

private IEnumerable<EntityDto> FilteredMobileItems =>
    string.IsNullOrWhiteSpace(_mobileSearchTerm)
        ? _items
        : _items.Where(x => x.Name.Contains(_mobileSearchTerm, StringComparison.OrdinalIgnoreCase));

private Task OnMobileSearchChanged(string value)
{
    _mobileSearchTerm = value;
    return Task.CompletedTask;
}
```

### 4.2 Mobile Client-Side Sorting & Virtualize (Large Lists)
- For mobile list views containing hundreds of items, iterating via `@foreach` will cause performance lag.
- Use the `<Virtualize>` component from `Microsoft.AspNetCore.Components.Web.Virtualization` to render only visible elements.
- Use `<ClientSortBar>` to provide dropdown sorting capabilities based on `SortOption<T>`.

Example implementation:
```razor
@if (Viewport.IsMobile && !_isLoading && _filtered.Any())
{
    <RadzenStack Gap="1rem">
        <ClientSortBar TItem="EntityDto"
                       Options="@SortOptions"
                       SortKey="@_sortKey"
                       SortKeyChanged="OnSortKeyChanged"
                       Ascending="@_sortAscending"
                       AscendingChanged="OnSortDirectionChanged"
                       Label="@L[RK.Common.SortBy]"
                       Placeholder="@L[RK.Common.SortBy]"
                       AscendingLabel="@L[RK.Common.Ascending]"
                       DescendingLabel="@L[RK.Common.Descending]" />
        <Virtualize Items="@_filtered" Context="item" OverscanCount="5">
            <div class="mb-3">
                <RadzenCard Class="p-3 m-0">
                    <RadzenText TextStyle="TextStyle.Body2"><strong>@item.Name</strong></RadzenText>
                </RadzenCard>
            </div>
        </Virtualize>
    </RadzenStack>
}
```
In `@code`:
```csharp
private string? _sortKey;
private bool _sortAscending = true;

private IReadOnlyList<SortOption<EntityDto>> SortOptions =>
[
    new("name", L[RK.Feature.ColName], x => x.Name),
    new("date", L[RK.Feature.ColDate], x => x.CreatedDate)
];

private void OnSortKeyChanged(string? key) { _sortKey = key; ApplyFilter(); }
private void OnSortDirectionChanged(bool asc) { _sortAscending = asc; ApplyFilter(); }

private void ApplyFilter()
{
    IEnumerable<EntityDto> result = _items;
    
    // 1. Filter
    if (!string.IsNullOrWhiteSpace(_mobileSearchTerm))
        result = result.Where(x => x.Name.Contains(_mobileSearchTerm, StringComparison.OrdinalIgnoreCase));
        
    // 2. Sort
    if (!string.IsNullOrEmpty(_sortKey))
    {
        var opt = SortOptions.FirstOrDefault(o => o.Key == _sortKey);
        if (opt is not null)
            result = _sortAscending ? result.OrderBy(opt.KeySelector) : result.OrderByDescending(opt.KeySelector);
    }
    
    _filtered = result.ToList();
}
```

### 4.3 Full DTO Columns + Mobile Card Details
- Desktop grid must show all DTO fields via `RadzenDataGridColumn`.
- For each field in DTO, add:
  - `RadzenDataGridColumn TItem="EntityDto" Property="FieldName" Title="@L[ResourceKeys.{Feature}.FieldName]"`
  - If needed, use visible binding for large field sets: `Visible="@(Viewport.IsDesktop || Viewport.IsTablet)"`.
- In mobile card mode, render same fields using `RadzenCard`:
  - `@L[ResourceKeys.{Feature}.FieldName]: @item.FieldName` (or formatted string for numerics/dates)
-    Format recommendations:
  - currency/price: `@item.Price.ToString("N2")`
  - percent: `@item.ChangePct.ToString("P2")`
  - date: `@item.LastUpdated` or `DateTime.Parse(item.LastUpdated).ToString("g")`
- Keep `NoDataFound` state in card format.
- If data set is large, keep grid mode as default and mobile cards only for screen < 768.

### 5. Delete Confirmation
- Use Radzen `DialogService.Confirm()` -- not browser `confirm()`
- Call delete API, reload grid on success
- `NotifySuccess()` / `NotifyError()` for feedback

### 6. Localization
- `@inject IStringLocalizer<Resource> L`
- Use `ResourceKeys` constants: `L[ResourceKeys.Entity.Title]`
- Never use raw string literals

### 7. Loading State
- `_isLoading` flag for grid loading
- Disable actions during loading

### 8. Breadcrumb (MANDATORY)

Breadcrumbs are built **automatically** by `MainLayout.BuildBreadcrumbFromRoute()` based on URL segments. You MUST register every new URL segment so it resolves to a localized label instead of the raw slug.

**Checklist — do ALL steps for every new page:**

#### 8a. Add to `RouteSegments.cs`
For each new URL segment (even intermediate ones like `chainbroker`):
```csharp
// src/ThaiX.Client/Constants/RouteSegments.cs
public const string ChainBroker = "chainbroker";
public const string ChainBrokerFunds = "funds";
```

#### 8b. Add to `ResourceKeys.Breadcrumb` if it's an intermediate segment
If the segment is a parent (e.g. `chainbroker` in `/market/chainbroker/funds`), add a key under `ResourceKeys.Breadcrumb`:
```csharp
// src/ThaiX.Client/Constants/ResourceKeys.cs
public static class Breadcrumb
{
    public const string Account = "Breadcrumb.Account";
    public const string ChainBroker = "Breadcrumb.ChainBroker"; // add new intermediates here
}
```
Leaf segments (funds, projects, unlocks) reuse their page `Title` key — no new Breadcrumb key needed.

#### 8c. Add resx entries for new Breadcrumb keys
```xml
<!-- Localization.SharedResource.resx -->
<data name="Breadcrumb.ChainBroker" xml:space="preserve"><value>ChainBroker</value></data>
<!-- Localization.SharedResource.vi.resx -->
<data name="Breadcrumb.ChainBroker" xml:space="preserve"><value>ChainBroker</value></data>
```

#### 8d. Update `MainLayout.GetSegmentLabel()` in `Layout/MainLayout.razor`
Register the segment in the correct parent `switch`:

```csharp
// In the parent == RouteSegments.Market block:
RouteSegments.ChainBroker => L[ResourceKeys.Breadcrumb.ChainBroker],

// Add a new block for the new intermediate:
if (parent == RouteSegments.ChainBroker)
{
    return segment switch
    {
        RouteSegments.ChainBrokerFunds => L[ResourceKeys.ChainBrokerFunds.Title],
        RouteSegments.ChainBrokerProjects => L[ResourceKeys.ChainBrokerProjects.Title],
        RouteSegments.ChainBrokerUnlocks => L[ResourceKeys.ChainBrokerUnlocks.Title],
        _ => SegmentToTitle(segment)
    };
}
```

**Pattern summary for a route like `/area/group/page`:**
| Segment | Where to handle |
|---------|----------------|
| `area` (index 0) | Top-level `switch` in `GetSegmentLabel` |
| `group` (parent == `area`) | `parent == RouteSegments.Area` block |
| `page` (parent == `group`) | new `parent == RouteSegments.Group` block |

Do NOT skip any of the 4 steps above. If omitted, the breadcrumb will show raw slugs like "chainbroker" instead of localized labels.

### 9. NavMenu permission guard (MANDATORY)

Every page that requires a permission MUST be hidden in the NavMenu when the user lacks that permission.
Use `auth.User.HasClaim("scope", PermissionNames.XxxRead)` — the same permission constant used in `[Authorize(Policy = ...)]`.

**Pattern — single page:**
```razor
@if (auth.User.HasClaim("scope", PermissionNames.ChainBrokerDataRead))
{
    <RadzenPanelMenuItem Text="@L[ResourceKeys.ChainBrokerFunds.Title]"
                         Path="market/chainbroker/funds"
                         Icon="account_balance_wallet" />
}
```

**Pattern — grouped pages under a shared permission:**
```razor
@if (auth.User.HasClaim("scope", PermissionNames.ChainBrokerDataRead))
{
    <RadzenPanelMenuItem Text="@L[ResourceKeys.ChainBrokerFunds.Title]" Path="market/chainbroker/funds" Icon="account_balance_wallet" />
    <RadzenPanelMenuItem Text="@L[ResourceKeys.ChainBrokerProjects.Title]" Path="market/chainbroker/projects" Icon="rocket_launch" />
    <RadzenPanelMenuItem Text="@L[ResourceKeys.ChainBrokerUnlocks.Title]" Path="market/chainbroker/unlocks" Icon="lock_open" />
}
```

Rules:
- If all children of a parent `RadzenPanelMenuItem` are permission-guarded, also guard the parent.
- The `PermissionNames` constant used here MUST be registered in `PermissionNames.GetAll()` (client-side policy list used for Blazor policy registration in `Program.cs`).
- Keep guard permission aligned with the page's `[Authorize(Policy = ...)]` attribute.

**Admin seed is automatic.** `DbInitializer` calls `Permissions.GetAdminPermissions()` (reflection-based) on every startup and adds any missing permission claims to the admin user. Adding the constant to `Domain/Common/Constants/Permissions.cs` is all that is needed — no manual seed SQL or code changes required.
