---
name: create-list-page
description: "Generate responsive Blazor WASM admin list pages with RadzenDataGrid, server-side paging, filtering, and mobile search UX. Use when creating or reworking data-grid listing pages."
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

### 4.2 Full DTO Columns + Mobile Card Details
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

### 8. Breadcrumb
- Update `RouteSegments` if new route segment
- Update `ResourceKeys` if new label needed
- Update `MainLayout.GetSegmentLabel()` if needed
- Update `Layout/NavMenu.razor` to expose the new page in navigation (use localized `ResourceKeys` label)
