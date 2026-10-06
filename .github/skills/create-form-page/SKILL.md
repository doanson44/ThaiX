---
name: create-form-page
description: "Generate a Blazor create/edit form with RadzenTemplateForm, responsive layout, validation, and double-submit prevention. Use when creating admin create or edit forms."
argument-hint: "Entity name and mode: create, edit, or both"
---

# Create Form Page

Generate create/edit form following ThaiX responsive conventions.

## Procedure

### 1. Form Component
- Use `RadzenTemplateForm<{Entity}Model>` with validators
- Wrap in `RadzenCard`

### 2. Responsive Layout
```razor
<RadzenRow Gap="1rem">
    <RadzenColumn Size="12" SizeMD="6" SizeLG="4">
        <RadzenFormField Text="@L[ResourceKeys.Entity.FieldName]">
            <RadzenTextBox @bind-Value="@_model.FieldName" />
        </RadzenFormField>
    </RadzenColumn>
</RadzenRow>
```
- Forms stack vertically on mobile (Size="12")
- Multi-column on tablet/desktop (SizeMD, SizeLG)
- If the form includes mobile list/select sections (lookup cards, pickers), use `DebouncedSearchBox` for client-side filtering.

### 2.1 Mobile Search (When Applicable)
- Use `DebouncedSearchBox` from `ThaiX.Client.Components` for mobile lookup/list filtering.
- Bind to `_mobileSearchTerm` + `OnMobileSearchChanged` handler.
- Use `DebounceMilliseconds="300"`.
- Render mobile content from filtered collection (example: `FilteredMobileItems`).
- Reuse localization keys: `ResourceKeys.Common.Search`, `ResourceKeys.Common.NoDataFound`.

### 3. Submission
- Submit button with `IsBusy="@_isSubmitting"` and `Disabled="@_isSubmitting"`
- `_isSubmitting` flag prevents double submission
- Call API service, handle success/error
- `NotifySuccess()` on success, `NotifyError()` on failure

### 4. Validation Display
- FluentValidation errors from API displayed inline
- Use `RadzenRequiredValidator`, `RadzenLengthValidator` for client-side

### 5. Edit Mode
- Load existing data in `OnParametersSetAsync`
- `_isLoading` flag while fetching
- Map API response to form model

### 6. Localization
- All labels via `L[ResourceKeys.Entity.FieldName]`
- All buttons via `L[ResourceKeys.Common.Save]` etc.

### 7. Navigation and Routing
- If the form is a dedicated page route, add/update `@page` route.
- Update `Layout/NavMenu.razor` when the page should be visible in navigation.
- Update breadcrumb mapping: `RouteSegments`, `ResourceKeys`, and `MainLayout.GetSegmentLabel()` when introducing new segments.

### 8. AsyncSearchSelect for Dropdowns
Use `AsyncSearchSelect` (from `ThaiX.Client.Shared`) instead of `RadzenDropDown` when a list has >20 items or benefits from keyword filtering.

**In-memory pattern** (use when items are already loaded locally, e.g. from an API response):
```razor
<div>
    <label class="rz-label mb-1">@L[ResourceKeys.Feature.FieldName]</label>
    <AsyncSearchSelect TItem="string"
                       Value="@_selected"
                       ValueChanged="OnSelectedChanged"
                       LoadDataAsync="LoadItemsAsync"
                       GetText="@(c => c)"
                       GetValue="@(c => c)"
                       Placeholder="@L[ResourceKeys.Feature.SelectPlaceholder]"
                       SearchPlaceholder="@L[ResourceKeys.Common.Search]"
                       LoadingText="@L[ResourceKeys.Common.Loading]"
                       EmptyText="@L[ResourceKeys.Common.NoDataFound]"
                       MoreHint="@L[ResourceKeys.MasterData.SelectScrollMore]"
                       Class="w-100" />
</div>
```
```csharp
private Task<(IReadOnlyList<string>, int)> LoadItemsAsync(
    int page, int pageSize, string? search, CancellationToken _)
{
    var filtered = string.IsNullOrWhiteSpace(search)
        ? _allItems
        : _allItems
            .Where(c => c.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

    IReadOnlyList<string> paged = filtered
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Task.FromResult((paged, filtered.Count));
}

private void OnSelectedChanged(string? value)
{
    _selected = value ?? string.Empty;
    // trigger side-effects here
}
```
- `AsyncSearchSelect` is NOT a valid child of `RadzenFormField` -- use a `<div>` + `<label class="rz-label">` wrapper.
- For separate From/To dropdowns backed by the same list, create two separate method references (`LoadFromAsync` / `LoadToAsync`) both delegating to the same filter helper.
