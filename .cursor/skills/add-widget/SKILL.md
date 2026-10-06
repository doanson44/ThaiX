---
name: add-widget
description: "Build a dashboard or market widget end-to-end across backend query flow, client DTO/service, localization keys, and Blazor component UI. Use when creating new data widgets."
argument-hint: "Widget name and data source, e.g. 'VIX from Yahoo Finance' or 'Gold Price from CafeF'"
---

# Add Widget (End-to-End)

Full workflow for creating a new data widget: from backend API query to a production-ready,
localized, self-fetching Blazor component.

## Non-Negotiable Rules (Apply to Every Widget)

These are **blockers**. A widget is not done until both are fully satisfied.

### Responsive Design
- Use `RadzenRow` + `RadzenColumn` with both `Size` (mobile) and `SizeMD`/`SizeLG` breakpoints everywhere.
  - 3-column metric rows: `Size="6" SizeMD="4"` (2-up mobile, 3-up desktop).
  - Full-width then 2-up: `Size="12" SizeMD="6"`.
- Switch `TextStyle` based on `Viewport.IsMobile` for headings that would overflow on phone screens.
- Inject `IViewportService`, subscribe `OnBreakpointChanged += StateHasChanged` in `OnInitialized`, unsubscribe in `Dispose`.
- Never use fixed pixel widths (`Style="width: Npx"`).
- Validate mentally for 375px / 768px / 1280px before marking done.

### Internationalization
- Zero hardcoded strings in the `.razor` file. Every user-visible label uses `L[ResourceKeys.X.Y]`.
- Per-string workflow: add `ResourceKeys` constant → en resx entry → vi resx entry (proper Vietnamese diacritics) → use in markup.
- Both `.resx` files must stay synchronized (same keys, same count).

---

## Phase 1 — Backend (Application + Infrastructure + Presentation)

### 1.1 Query + Response DTO

Create in `Application/Features/ExternalData/{Feature}/Queries/{QueryName}/`:

```csharp
// {QueryName}Query.cs
public sealed record {QueryName}Query : IRequest<{QueryName}Response>;

public sealed record {QueryName}Response
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    // All domain fields here
}
```

- Keep `Success` and `Message` on every response for error propagation.
- Use `decimal?` for numeric values that may be missing.

### 1.2 Query Handler

```csharp
// {QueryName}QueryHandler.cs
public sealed class {QueryName}QueryHandler
    : IRequestHandler<{QueryName}Query, {QueryName}Response>
{
    private readonly IExternalDataService _externalDataService;

    public {QueryName}QueryHandler(IExternalDataService externalDataService)
        => _externalDataService = externalDataService;

    public async Task<{QueryName}Response> Handle(
        {QueryName}Query request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .Get{Source}Async<InternalResponse>(cancellationToken);

        if (apiResponse is null)
            return new {QueryName}Response { Success = false, Message = "No data." };

        // Project from internal → response (manual, no AutoMapper)
        return new {QueryName}Response { Success = true, ... };
    }

    // Internal deserialization records (private, in same file)
    private sealed record InternalResponse { ... }
}
```

Rules:
- Internal deserialization types stay in the same file as the handler (sealed records, private).
- Never leak internal types out of the handler file.
- `AsNoTracking()` and `Select()` projection apply to EF queries; external API responses are mapped manually.

### 1.3 Infrastructure Service (if new endpoint)

If the external API method doesn't exist yet, add to `IExternalDataService` (Application layer):

```csharp
Task<T?> Get{Source}Async<T>(CancellationToken cancellationToken = default);
```

Implement in `Infrastructure/Services/ExternalDataService.cs`:

```csharp
public Task<T?> Get{Source}Async<T>(CancellationToken cancellationToken = default)
    => _{provider}ApiProvider.Get{Method}Async<T>(cancellationToken);
```

### 1.4 Presentation Endpoint

Add to `Presentation/Endpoints/ExternalDataEndpoints.cs`:

```csharp
// GET /api/external-data/{route}
group.MapGet("{route}", async (IMediator mediator, HttpContext ctx, CancellationToken ct) =>
{
    var result = await mediator.Send(new {QueryName}Query(), ct);
    var response = ApiResponse<{QueryName}Response>.SuccessResult(result);
    response.Metadata.CorrelationId = ctx.GetCorrelationId();
    return Results.Ok(response);
})
.RequireAuthorization(Permissions.MasterDataRead)
.WithName("{QueryName}")
.WithTags("ExternalData");
```

---

## Phase 2 — Client Data Layer

### 2.1 Client DTO

Add to `ThaiX.Client/Models/ExternalData/ExternalDataContracts.cs`:

```csharp
/// <summary>
/// {Feature} response DTO for client consumption.
/// </summary>
public sealed record {Feature}ResponseDto
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    // Mirror all fields from server response
}
```

- One DTO per feature. Never reuse server-side types in the client.
- `decimal?` for nullable numerics, `string?` for nullable strings.

### 2.2 Client Service Interface

Add to `IExternalDataService.cs`:

```csharp
Task<ApiResponse<{Feature}ResponseDto>> Get{Feature}Async(CancellationToken cancellationToken = default);
```

### 2.3 Client Service Implementation

Add to `ExternalDataService.cs`:

```csharp
private const string {Feature}Endpoint = "{route-segment}";

public async Task<ApiResponse<{Feature}ResponseDto>> Get{Feature}Async(CancellationToken cancellationToken = default)
{
    var path = $"{ExternalDataBase}/{Feature}Endpoint}";
    var response = await _httpClient.GetAsync(path, cancellationToken);

    if (!response.IsSuccessStatusCode)
        throw new ApiException("EXTERNAL_API_ERROR", $"API call failed: {response.StatusCode}", response.StatusCode);

    var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<{Feature}ResponseDto>>(cancellationToken: cancellationToken);
    if (envelope is null)
        throw new ApiException("CLIENT_INVALID_RESPONSE", "Invalid response from external API.", HttpStatusCode.InternalServerError);

    return envelope;
}
```

---

## Phase 3 — Localization

### 3.1 ResourceKeys

Add a nested class inside `ResourceKeys` in `Constants/ResourceKeys.cs`:

```csharp
public static class {Feature}
{
    public const string Title      = "{Feature}.Title";
    public const string LoadFailed = "{Feature}.LoadFailed";
    // One constant per label used in the widget
}
```

### 3.2 English resx

Add to `Resources/Localization.SharedResource.resx`:

```xml
<data name="{Feature}.Title" xml:space="preserve"><value>...</value></data>
<data name="{Feature}.LoadFailed" xml:space="preserve"><value>Failed to load {Feature} data.</value></data>
```

### 3.3 Vietnamese resx

Add matching keys to `Resources/Localization.SharedResource.vi.resx`.

Rules:
- **Vietnamese values MUST use proper Unicode diacritics** — `.resx` is explicitly exempt from the ASCII-only policy, and Vietnamese without diacritics is a bug, not a convention.
- Keep keys identical across both files.

---

## Phase 4 — Widget Component

### 4.1 File Location

- Dashboard/analysis widgets: `Components/Dashboard/{Feature}Widget.razor`
- Market data widgets: `Components/Market/{Feature}Widget.razor`

### 4.2 Standard Widget Template

```razor
@using ThaiX.Client.Models.ExternalData
@using ThaiX.Client.Services.ExternalData
@using ThaiX.Client.Localization
@using Microsoft.Extensions.Localization
@using static ThaiX.Client.Constants.ResourceKeys
@inject IExternalDataService ExternalDataService
@inject ThaiX.Client.Services.Ui.IViewportService Viewport
@inject IStringLocalizer<SharedResource> L
@implements IDisposable

<RadzenCard>
    @if (_isLoading)
    {
        <div class="d-flex justify-content-center align-items-center p-4">
            <RadzenProgressBar Value="100" ShowValue="false" Mode="ProgressBarMode.Indeterminate" Class="w-100" />
        </div>
    }
    else if (_data is null || !_data.Success)
    {
        <RadzenAlert AlertStyle="AlertStyle.Warning" ShowIcon="true" Variant="Variant.Flat" Shade="Shade.Lighter">
            @(_data?.Message ?? L[{Feature}.LoadFailed])
        </RadzenAlert>
    }
    else
    {
        <RadzenStack Gap="1rem">
            @* Header *@
            <RadzenStack Orientation="Orientation.Horizontal"
                         AlignItems="AlignItems.Center"
                         JustifyContent="JustifyContent.SpaceBetween">
                <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H2" Class="mb-0">@L[{Feature}.Title]</RadzenText>
                @* right-side value / timestamp *@
            </RadzenStack>

            @* Content zones here *@
        </RadzenStack>
    }
</RadzenCard>

@code {
    private {Feature}ResponseDto? _data;
    private bool _isLoading;

    protected override void OnInitialized()
    {
        Viewport.OnBreakpointChanged += StateHasChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            var response = await ExternalDataService.Get{Feature}Async();
            if (response is { Data: not null })
                _data = response.Data;
        }
        catch
        {
            // _data stays null; widget shows error state
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

### 4.3 Layout Zones (Dashboard Style)

Use 3 zones for analytical widgets:

| Zone | Content | Component |
|---|---|---|
| Header | Title + key value (right) | `RadzenStack` horizontal space-between |
| Core Signal | Badge + bold action text | `div` with `bg-*-opacity-10` + `RadzenBadge` |
| Metrics Grid | Supporting numbers | `RadzenRow` / `RadzenColumn` |
| Macro Cards | Text categories | small `RadzenCard` in 2-col grid |
| Footer | Low-priority stats | `RadzenText TextStyle.Caption` |

### 4.4 Color Conventions for State Signals

Use Bootstrap `bg-opacity-10` backgrounds + Radzen `BadgeStyle`:

```razor
@* Determine color class in @code — no inline logic in markup *@
private string SignalBgClass => _data?.State switch
{
    "Positive" => "bg-success bg-opacity-10",
    "Negative" => "bg-danger bg-opacity-10",
    "Neutral"  => "bg-info bg-opacity-10",
    "Warning"  => "bg-warning bg-opacity-10",
    _          => "bg-secondary bg-opacity-10"
};

private BadgeStyle SignalBadgeStyle => _data?.State switch
{
    "Positive" => BadgeStyle.Success,
    "Negative" => BadgeStyle.Danger,
    "Neutral"  => BadgeStyle.Info,
    "Warning"  => BadgeStyle.Warning,
    _          => BadgeStyle.Light
};
```

### 4.5 Momentum / Percentage Formatting

Use a private static helper, never inline string interpolation with ternary:

```csharp
private static string FormatMomentum(decimal? value)
{
    if (!value.HasValue) return "-";
    decimal pct = value.Value * 100;
    return $"{(pct >= 0 ? "+" : "")}{pct:N2}%";
}
```

In markup:
```razor
<span class="@(value >= 0 ? "text-success" : "text-danger")">@FormatMomentum(value)</span>
```

### 4.6 Self-Fetch vs Parameter

- **Prefer self-fetch** (inject + call in `OnInitializedAsync`) — simpler usage, no caller plumbing.
- Use `[Parameter] public Data` only when the parent page already owns the data and must share it across multiple components.
- Never mix both patterns in the same widget.

---

## Phase 5 — Usage

Using a self-fetch widget on any page:

```razor
<{Feature}Widget />
```

No `@code` block or data loading needed in the parent.

---

## Checklist

- [ ] Phase 1: Query, Response DTO, Handler (Application)
- [ ] Phase 1: Service method on IExternalDataService + implementation (Infrastructure)
- [ ] Phase 1: Endpoint in Presentation
- [ ] Phase 2: Client DTO in ExternalDataContracts.cs
- [ ] Phase 2: IExternalDataService + ExternalDataService (Client)
- [ ] Phase 3: ResourceKeys constants
- [ ] Phase 3: en resx entries
- [ ] Phase 3: vi resx entries (proper Vietnamese diacritics, not ASCII approximations)
- [ ] Phase 4: Widget component (self-fetch, loading, error, localized)
- [ ] **Responsive**: every column uses `Size` + `SizeMD`/`SizeLG` breakpoints
- [ ] **Responsive**: headings sized with `Viewport.IsMobile` guard
- [ ] **Responsive**: `IViewportService` injected, `IDisposable` implemented, subscription in `OnInitialized`/`Dispose`
- [ ] **i18n**: zero hardcoded strings in markup — all via `L[ResourceKeys.*]`
- [ ] **i18n**: en resx and vi resx have matching keys; vi values use proper Vietnamese diacritics
- [ ] Build: `dotnet build ThaiX.Client.csproj` — zero errors
- [ ] Build: `dotnet build ThaiX.Application.csproj` — zero errors

---

## Anti-Patterns (Avoid)

- DO NOT load widget data in the parent page (unless shared across multiple components)
- DO NOT use `[Parameter]` for data that the widget can fetch itself
- DO NOT add inline `Style=` for colors — use Bootstrap utility classes
- DO NOT use raw string literals for labels — always `L[ResourceKeys.X]`
- DO NOT put business/regime logic in the widget — Application layer only
- DO NOT manually compute EMA, RSI, etc. — use `Skender.Stock.Indicators`
- DO NOT add ASCII-violating characters (emoji, smart quotes, Unicode arrows) outside `.resx` files
- DO NOT write Vietnamese without diacritics in `.vi.resx` — `Khong` instead of `Không` is a bug
- DO NOT use hardcoded English labels in markup — every user-visible string must use `L[ResourceKeys.*]`
- DO NOT use `Size="4"` alone for 3-column rows — always add `SizeMD="4"` with a mobile-safe `Size` (e.g. `6`)
- DO NOT omit `IViewportService` subscription — widgets without it will not re-render on resize
- DO NOT skip vi resx entries — both `.resx` files must always be in sync
