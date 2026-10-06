---
description: "Use when writing Blazor WASM pages, Razor components, layouts, or CSS for the ThaiX.Client project. Covers Radzen Blazor, Bootstrap 5 dark mode, responsive design, admin CRUD patterns, and component conventions."
applyTo: "src/ThaiX.Client/**"
---

# Blazor UI Rules (ThaiX.Client)

## MANDATORY: All Views Must Satisfy These Two Requirements

These are non-negotiable. Every Blazor page and component — no exceptions.

### 1. Responsive Design (All Devices)

- **NEVER** use fixed pixel widths or fixed column counts that break on narrow screens.
- Always use `RadzenRow` + `RadzenColumn` with both `Size` (mobile-first) and `SizeMD`/`SizeLG` breakpoints.
  - e.g. `Size="12" SizeMD="6"` for 2-up on tablet+, full-width on mobile.
  - e.g. `Size="6" SizeMD="4"` for 3-up on desktop, 2-up on mobile.
- For text sizes that would overflow on mobile, use `Viewport.IsMobile` to switch `TextStyle` (e.g. `H5` on desktop → `H6` on mobile).
- Inject `IViewportService` and implement `IDisposable` to trigger `StateHasChanged` on breakpoint changes.
- Test mentally: portrait phone (375px), tablet (768px), desktop (1280px+). If any column arrangement would break, fix it.
- No `Style="width: Npx"` in markup that could force horizontal scroll on small screens.

### 2. Internationalization (All User-Facing Text)

- **EVERY** user-visible string in a component must come from `IStringLocalizer<SharedResource>` via a `ResourceKeys` constant.
- No hardcoded English labels, titles, placeholders, tooltips, error messages, or button text in `.razor` files.
- Workflow per new string:
  1. Add constant to the appropriate `ResourceKeys` nested class.
  2. Add `<data>` entry to `Localization.SharedResource.resx` (en-US).
  3. Add matching `<data>` entry to `Localization.SharedResource.vi.resx` with **proper Vietnamese diacritics** (e.g. `Không thể tải`, never `Khong the tai`). `.resx` is the only file type where Unicode is allowed — use it correctly.
  4. Use `L[ResourceKeys.Feature.ConstantName]` in markup.
- Never substitute `_("raw text")`, `@("hardcoded")`, or concatenated strings for localized labels.

---

## Theme and Dark Mode

- Bootstrap 5.3 dark mode via `data-bs-theme="dark"` on `<html>`.
- Radzen theme: `standard-dark-base.css`.
- Use CSS variables (`var(--bs-*)`, `var(--rz-*)`) instead of hardcoded colors.

## Layouts

- `MainLayout`: default admin shell (sidebar, header, breadcrumb, content).
- `AuthLayout`: standalone for auth pages.
- Auth pages (`Pages/Auth/`) must use `@layout AuthLayout`.

## Folder Organization and Routing (MANDATORY)

All Pages and Components MUST be placed in the correct menu-grouped folder. Routes follow the pattern `/<group-prefix>/<page-name>`.

### Pages Folder → Route Mapping

| Folder | Route Prefix | Menu Group |
|--------|-------------|------------|
| `Pages/Home/` | `/` | Home |
| `Pages/Account/` | `/account/` | Account |
| `Pages/Administration/` | `/admin/` | Administration |
| `Pages/Auth/` | `/auth/` | Auth |
| `Pages/Crm/` | `/crm/` | CRM |
| `Pages/MarketData/` | `/market-data/` | Market Data |
| `Pages/MarketResearch/` | `/market-research/` | Market Research |
| `Pages/MasterData/` | `/master-data/` | Master Data |
| `Pages/Notification/` | `/notifications/` | Notification |
| `Pages/Portfolio/` | `/portfolio/` | Portfolio |
| `Pages/Security/` | `/security/` | Security |

### Components Folder → Namespace Mapping

| Folder | Namespace |
|--------|-----------|
| `Components/Administration/` | `ThaiX.Client.Components.Administration` |
| `Components/Crm/` | `ThaiX.Client.Components.Crm` |
| `Components/Crm/Contacts/` | `ThaiX.Client.Components.Crm.Contacts` |
| `Components/Crm/Notes/` | `ThaiX.Client.Components.Crm.Notes` |
| `Components/MarketData/` | `ThaiX.Client.Components.MarketData` |
| `Components/MarketData/Dashboard/` | `ThaiX.Client.Components.MarketData.Dashboard` |
| `Components/MarketResearch/` | `ThaiX.Client.Components.MarketResearch` |
| `Components/MarketResearch/Market/` | `ThaiX.Client.Components.MarketResearch.Market` |
| `Components/MasterData/` | `ThaiX.Client.Components.MasterData` |
| `Components/Notification/` | `ThaiX.Client.Components.Notification` |
| `Components/Portfolio/` | `ThaiX.Client.Components.Portfolio` |
| `Components/Portfolio/Portfolios/` | `ThaiX.Client.Components.Portfolio.Portfolios` |
| `Components/Security/` | `ThaiX.Client.Components.Security` |
| `Components/Shared/` | `ThaiX.Client.Components.Shared` |

### New Page Checklist

When creating a new page:
1. Determine the correct menu group folder from the table above.
2. Place the `.razor` file in `Pages/<GroupFolder>/`.
3. Set `@page "/<route-prefix>/<page-name>"` matching the group prefix.
4. Add `@using` to `_Imports.razor` for the new Pages namespace if needed.
5. Add the page path to `RouteSegments.cs`.
6. Add the `NavMenu.razor` menu item under the correct group.
7. Add the breadcrumb label for the route segments in `MainLayout.razor` → `GetSegmentLabel()`.

### New Component Checklist

When creating a new component:
1. Determine the correct group folder from the table above.
2. Place the `.razor` file in `Components/<GroupFolder>/`.
3. Set `@namespace` matching the table if different from folder convention.
4. Add `@using` to `_Imports.razor` for the new Components namespace if needed.
5. Never place components in the `Pages` folder — that folder is only for routable pages.

## Breadcrumb

- `RadzenBreadCrumb` in `MainLayout` above content.
- Default trail from URL via `RouteSegments` and `ResourceKeys`.
- Override via `IBreadcrumbService.Set(...)`.
- New routes: update `RouteSegments`, `ResourceKeys`, `MainLayout.GetSegmentLabel()`.

## Admin CRUD Pattern (Mandatory)

- `Data Grid + Edit Panel` pattern. Primary surface: `RadzenDataGrid`.
- Every grid: `FilterMode="FilterMode.Advanced"`, `ShowPagingSummary="true"`, `PageSizeOptions`.
- Use `AllowFiltering="true"` with `LoadData` (server-side).
- Desktop (>=1200px): grid + side panel or dialog for edit.
- Tablet (768-1199px): `RadzenDialog` for edit.
- Mobile (<768px): compact list/card + full-width dialog.
- No full-page navigation for CRUD. No inline forms above/below grid.
- Row click to open edit. Delete via Radzen confirmation (not browser `confirm()`).

## UI Responsive Design Convention (Updated)

When implementing responsive layouts in Blazor:
1. **Avoid manual JS Interop**: Do NOT write custom JS window listeners per component.
2. **IViewportService**: Always use `@inject ThaiX.Client.Services.Ui.IViewportService Viewport`.
3. **Breakpoints**: Utilize `Viewport.IsMobile`, `Viewport.IsTablet`, and `Viewport.IsDesktop` for rendering logic.
4. **Lifecycle Subscription**: Remember to implement `IDisposable` and subscribe `Viewport.OnBreakpointChanged += StateHasChanged;`.
5. **Fluid Layouts**: Prefer native `<RadzenRow>` and `<RadzenColumn Size="12" SizeMD="4">` for responsive grids instead of fixed widths or manual wraps where appropriate.

## CSS: Utility-First

- Prefer Bootstrap 5 / Radzen utility classes over `Style=` and custom CSS.
- `class="w-100"` not `Style="width: 100%;"`.
- Scoped `.razor.css` only for truly component-specific styles.
- No fixed pixel widths that break on mobile.

## JavaScript (Strict)

- Prefer Blazor/Radzen/Bootstrap behavior first.
- No custom JS for CRUD, modals, confirmations, or layout.
- No `JSRuntime.InvokeVoidAsync("eval", ...)`.
- No browser `confirm()` in admin flows.

## Component Conventions

- `RadzenCard` as primary container.
- `RadzenTemplateForm` with validators for forms.
- `RadzenStack` for layout; `Gap` over manual margins.
- `IsBusy`/`Disabled` on submit buttons.
- Async flags: `_isSubmitting`, `_isLoading`, `_isSaving`.

## Notifications

- Use `NotificationExtensions`: `NotifySuccess()`, `NotifyError()`, `NotifyInfo()`, `NotifyWarning()`.
- Not `NotificationService.Notify()` directly.

## Localization

- `IStringLocalizer` with `ResourceKeys` constants from `ThaiX.Client.Constants.ResourceKeys`.
- Use `L[Auth.LoginTitle]` not `L["Auth.Login.Title"]`.
- Never use raw string literals for keys.

## Performance

- Server-side paging for large datasets.
- Lazy loading for expensive detail content.
- No full dataset loading into browser.

## AsyncSearchSelect Usage

`Shared/AsyncSearchSelect.razor` is the approved custom searchable dropdown with infinite scroll. Use it instead of `RadzenDropDown` whenever the list has more than ~20 items or needs keyword filtering.

### Two modes

**Server-backed** (MasterData lookups — Country, City, Bank):
```razor
<AsyncSearchSelect TItem="CountryListItemDto"
                   Value="@_model.CountryCode"
                   ValueChanged="OnCountryChanged"
                   LoadDataAsync="LoadCountriesAsync"
                   GetText="x => x.DisplayText"
                   GetValue="x => x.Code"
                   ... />
```
```csharp
private async Task<(IReadOnlyList<CountryListItemDto>, int)> LoadCountriesAsync(
    int page, int pageSize, string? search, CancellationToken ct) =>
    await MasterDataService.GetCountriesAsync(page, pageSize, search, ct);
```

**In-memory** (client-side list already loaded — e.g. currency codes from exchange rates):
```razor
<AsyncSearchSelect TItem="string"
                   Value="@_selected"
                   ValueChanged="OnSelectionChanged"
                   LoadDataAsync="LoadItemsAsync"
                   GetText="@(c => c)"
                   GetValue="@(c => c)"
                   ... />
```
```csharp
// Filter against a pre-loaded list; no API call needed.
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

private void OnSelectionChanged(string? value)
{
    _selected = value ?? string.Empty;
    // trigger recompute / side effect here
}
```

### Label outside RadzenFormField
`AsyncSearchSelect` cannot be a child of `RadzenFormField`. Use a plain label instead:
```razor
<div>
    <label class="rz-label mb-1">@L[ResourceKeys.Feature.FieldName]</label>
    <AsyncSearchSelect ... Class="w-100" />
</div>
```

### Required resource keys
Always supply these localization parameters:
- `SearchPlaceholder="@L[ResourceKeys.Common.Search]"`
- `LoadingText="@L[ResourceKeys.Common.Loading]"`
- `EmptyText="@L[ResourceKeys.Common.NoDataFound]"`
- `MoreHint="@L[ResourceKeys.MasterData.SelectScrollMore]"`
