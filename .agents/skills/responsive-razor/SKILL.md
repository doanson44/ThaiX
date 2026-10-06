---
name: responsive-razor
description: "Audit and update a Blazor Razor component to display correctly on all viewports (mobile 375px, tablet 768px, desktop 1280px+). Fixes layout anti-patterns, adds IViewportService lifecycle, converts fixed widths to responsive grids, and adapts text sizes. Use when a component looks broken or cramped on phone/tablet."
argument-hint: "Path to the .razor file to make responsive, e.g. 'Components/Dashboard/MyWidget.razor'"
---

# Responsive Razor

Audit a Blazor Razor component for viewport issues and fix every anti-pattern found.

## Non-Negotiable Output Requirements

Every component leaving this flow must pass all three checks:

1. **Mobile (375px portrait)** -- no horizontal scroll, text readable, no clipped cards.
2. **Tablet (768px)** -- 2-column layout where applicable, no wasted space.
3. **Desktop (1280px+)** -- multi-column layout, full content visible.

---

## Step 1 -- Read and Audit the File

Read the target `.razor` file in full. Then check for every anti-pattern in the list below.

### Anti-Pattern Checklist

Mark each as FOUND or CLEAN.

| # | Anti-Pattern | What to Look For |
|---|---|---|
| AP-1 | Fixed pixel widths | `Style="width: Npx"`, `style="width:Npx"`, `Width="N"` on layout containers |
| AP-2 | Missing `SizeMD`/`SizeLG` on `RadzenColumn` | `<RadzenColumn Size="N">` without `SizeMD` or `SizeLG` |
| AP-3 | Missing `IViewportService` when rendering differs per breakpoint | Conditional markup like `@if (...)` for "mobile" without `Viewport.IsMobile` |
| AP-4 | Missing lifecycle subscription | No `Viewport.OnBreakpointChanged += StateHasChanged` in `OnInitialized` |
| AP-5 | Missing lifecycle unsubscription | No `Viewport.OnBreakpointChanged -= StateHasChanged` in `Dispose` |
| AP-6 | `@implements IDisposable` missing when subscription exists | Subscription present but `IDisposable` not declared |
| AP-7 | `IViewportService.InitializeAsync()` not called | `Viewport.Width` or JS-dependent logic without `await Viewport.InitializeAsync()` in `OnAfterRenderAsync(firstRender)` |
| AP-8 | Heading `TextStyle` not adapted to mobile | `TextStyle.H3`/`H4`/`H5` unconditionally for titles that overflow on 375px wide screens |
| AP-9 | Table (`<table>`) without responsive wrapper | Bare `<table>` that will overflow on small screens |
| AP-10 | Bootstrap `col-*` classes instead of `RadzenColumn` | `class="col-md-4"` mixed with Radzen layout |
| AP-11 | Unconditional `RadzenDataGrid` on mobile | Full grid with many columns shown on `<768px` without a card/list fallback |
| AP-12 | Hardcoded gap/margin in `Style=` | `Style="margin: 16px"` or `Gap="16px"` instead of Radzen `Gap="1rem"` or Bootstrap utilities |

---

## Step 2 -- Fix Each Found Anti-Pattern

Apply only the fixes for FOUND items. Do not change CLEAN areas.

### Fix AP-1: Remove Fixed Pixel Widths

```razor
<!-- BEFORE -->
<RadzenCard Style="width: 320px;">

<!-- AFTER -->
<RadzenCard Class="w-100">
```

Never use pixel widths on layout containers. Use Bootstrap utility classes (`w-100`, `w-50`) or let Radzen grid control sizing.

---

### Fix AP-2: Add Responsive Breakpoints to RadzenColumn

**Decision table -- choose based on content type:**

| Content | Mobile (`Size`) | Tablet (`SizeMD`) | Desktop (`SizeLG`) |
|---|---|---|---|
| Single full-width section | 12 | 12 | 12 |
| Two-panel (list + detail) | 12 | 6 | 6 |
| Metric card (3-up desktop) | 12 | 6 | 4 |
| Metric card (4-up desktop) | 12 | 6 | 3 |
| Narrow stat card | 6 | 4 | 3 |

```razor
<!-- BEFORE -->
<RadzenColumn Size="4">

<!-- AFTER (3-metric row) -->
<RadzenColumn Size="12" SizeMD="6" SizeLG="4">
```

---

### Fix AP-3 + AP-4 + AP-5 + AP-6: Full IViewportService Setup

Add the injection and complete lifecycle pattern:

```razor
@inject ThaiX.Client.Services.Ui.IViewportService Viewport
@implements IDisposable
```

In `@code`:

```csharp
protected override void OnInitialized()
{
    Viewport.OnBreakpointChanged += StateHasChanged;
}

public void Dispose()
{
    Viewport.OnBreakpointChanged -= StateHasChanged;
}
```

Use `Viewport.IsMobile`, `Viewport.IsTablet`, `Viewport.IsDesktop` in markup for conditional rendering.

---

### Fix AP-7: InitializeAsync for Width-Dependent Logic

Only needed when reading `Viewport.Width` directly (not just `IsMobile`/`IsTablet`/`IsDesktop`):

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Viewport.InitializeAsync();
        StateHasChanged();
    }
}
```

---

### Fix AP-8: Adaptive TextStyle for Headings

```razor
<!-- BEFORE -->
<RadzenText TextStyle="TextStyle.H4">@L[Keys.Title]</RadzenText>

<!-- AFTER -->
<RadzenText TextStyle="@(Viewport.IsMobile ? TextStyle.H6 : TextStyle.H5)">@L[Keys.Title]</RadzenText>
```

**Safe thresholds (never exceed on mobile):**

- Widget/card titles: `H6` on mobile, `H5` on tablet+
- Page headings: `H5` on mobile, `H4` on tablet, `H3` on desktop
- Section labels: `Body2` on mobile, `Subtitle2` on desktop

---

### Fix AP-9: Responsive Table Wrapper

```razor
<!-- BEFORE -->
<table class="table">...</table>

<!-- AFTER -->
<div class="table-responsive">
    <table class="table">...</table>
</div>
```

Or, for `RadzenDataGrid`, add a card/list view for mobile:

```razor
@if (Viewport.IsMobile)
{
    @foreach (var item in _items)
    {
        <RadzenCard Class="mb-2">
            <!-- key fields only -->
        </RadzenCard>
    }
}
else
{
    <RadzenDataGrid ... />
}
```

---

### Fix AP-10: Replace Bootstrap Columns with RadzenColumn

```razor
<!-- BEFORE -->
<div class="row">
    <div class="col-md-4">...</div>
</div>

<!-- AFTER -->
<RadzenRow>
    <RadzenColumn Size="12" SizeMD="4">...</RadzenColumn>
</RadzenRow>
```

Do not mix Bootstrap column classes (`col-*`) with Radzen grid inside the same row.

---

### Fix AP-11: DataGrid with Mobile Card Fallback & Virtualize

When replacing a `RadzenDataGrid` with mobile cards, avoid rendering a large list using a simple `@foreach` as it causes extreme lag. Always wrap the list in a `<Virtualize>` component and consider adding a `<ClientSortBar>` if the grid allowed sorting.

```razor
@if (Viewport.IsMobile)
{
    <RadzenStack Gap="1rem">
        <!-- Optional: add sorting capabilities -->
        <ClientSortBar TItem="MyDto" Options="@SortOptions" ... />
        
        <!-- Use Virtualize for infinite scroll and DOM reuse -->
        <Virtualize Items="@_items" Context="item" OverscanCount="5">
            <div class="mb-3">
                <RadzenCard Class="p-3 m-0">
                    <RadzenStack Gap="0.25rem">
                        <RadzenText TextStyle="TextStyle.Body2"><strong>@item.Name</strong></RadzenText>
                        <RadzenText TextStyle="TextStyle.Caption" Class="text-muted">@item.SecondaryField</RadzenText>
                    </RadzenStack>
                </RadzenCard>
            </div>
        </Virtualize>
    </RadzenStack>
}
else
{
    <RadzenDataGrid TItem="MyDto" Data="@_items" ... />
}
```

---

### Fix AP-12: Replace Inline margin/gap with Utilities

```razor
<!-- BEFORE -->
<RadzenStack Gap="16px">
<div Style="margin-top: 16px;">

<!-- AFTER -->
<RadzenStack Gap="1rem">
<div Class="mt-3">
```

Use Bootstrap spacing utilities (`mt-1` through `mt-5`, `mb-`, `p-`, etc.) or Radzen `Gap` with rem units.

---

## Step 3 -- Pattern: Complete Responsive Widget Skeleton

Use this as the reference skeleton when rewriting or heavily modifying a widget:

```razor
@using ThaiX.Client.Localization
@using static ThaiX.Client.Constants.ResourceKeys
@inject ThaiX.Client.Services.Ui.IViewportService Viewport
@inject IStringLocalizer<SharedResource> L
@implements IDisposable

<RadzenCard>
    @if (_isLoading)
    {
        <div class="d-flex justify-content-center align-items-center p-4">
            <RadzenProgressBar Value="100" ShowValue="false"
                               Mode="ProgressBarMode.Indeterminate" Class="w-100" />
        </div>
    }
    else if (_hasError)
    {
        <RadzenAlert AlertStyle="AlertStyle.Warning" ShowIcon="true"
                     Variant="Variant.Flat" Shade="Shade.Lighter">
            @L[Feature.LoadFailed]
        </RadzenAlert>
    }
    else
    {
        <RadzenStack Gap="1rem">
            <!-- Title: smaller on mobile -->
            <RadzenText TextStyle="@(Viewport.IsMobile ? TextStyle.H6 : TextStyle.H5)"
                        TagName="TagName.H2" Class="mb-0">
                @L[Feature.Title]
            </RadzenText>

            <!-- Responsive metric row: 2-up mobile, 3-up desktop -->
            <RadzenRow>
                @foreach (var item in _items)
                {
                    <RadzenColumn Size="12" SizeMD="6" SizeLG="4">
                        <RadzenCard Class="p-3 h-100">
                            <RadzenText TextStyle="TextStyle.Body2"><strong>@item.Name</strong></RadzenText>
                            <RadzenText TextStyle="TextStyle.Caption" Class="text-muted">@item.Code</RadzenText>
                        </RadzenCard>
                    </RadzenColumn>
                }
            </RadzenRow>
        </RadzenStack>
    }
</RadzenCard>

@code {
    private bool _isLoading;
    private bool _hasError;
    private IReadOnlyList<MyItemDto> _items = [];

    protected override void OnInitialized()
    {
        Viewport.OnBreakpointChanged += StateHasChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            // fetch data here
        }
        catch
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    public void Dispose()
    {
        Viewport.OnBreakpointChanged -= StateHasChanged;
    }
}
```

---

## Step 4 -- Mental Viewport Validation

Before closing, verify each breakpoint mentally:

### 375px (Portrait Phone)
- [ ] No `RadzenColumn` renders side-by-side unless `Size < 12` is intentional for a short stat pair.
- [ ] All text fits without truncation or overflow.
- [ ] No fixed-width containers force horizontal scroll.
- [ ] Headings use mobile `TextStyle`.

### 768px (Tablet)
- [ ] Two-column grids activate (`SizeMD="6"`).
- [ ] No single column wastes the full width unnecessarily.
- [ ] Dialogs/panels are appropriately sized.

### 1280px (Desktop)
- [ ] Three- or four-column grids activate (`SizeLG="4"` or `SizeLG="3"`).
- [ ] DataGrids are shown (not the mobile card fallback).
- [ ] Adequate whitespace and padding.

---

## Step 5 -- Build Verification

After all changes, run:

```powershell
dotnet build src/ThaiX.Client/ThaiX.Client.csproj /consoleloggerparameters:NoSummary;ForceNoAlign 2>&1 | Select-String -Pattern "error CS"
```

Zero `error CS` lines required. Pre-existing warnings are acceptable.

---

## Common Pitfalls

| Pitfall | Correct Approach |
|---|---|
| Using `class=` instead of `Class=` on Radzen components | Always `Class=` (capital C) for Radzen component attributes |
| Using `@item.Price:N2` for number formatting | Use `@item.Price.ToString("N2")` or `@($"{item.Price:N2}")` |
| `Style="border: 1px solid #ccc"` | Use `Style="border: 1px solid var(--rz-border-color);"` (CSS variable) |
| Subscribing `OnBreakpointChanged` without `@implements IDisposable` | Always pair subscription with `IDisposable` |
| Auto-commit after fix | NEVER commit -- git operations are the user's responsibility |
