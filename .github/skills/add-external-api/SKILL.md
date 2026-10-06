---
name: add-external-api
description: "Integrate a new external API provider with configuration, provider class, service interface, DTOs, query handler, and endpoint. Use when adding a new third-party API integration."
argument-hint: "Provider name, e.g. 'Stripe' or 'SendGrid'"
---

# Add External API

Integrate a new external API provider following the ExternalApiService pattern.

## Procedure

### 1. Gather Requirements
- API URL, authentication method, response format
- Endpoints needed
- Caching requirements
- Review existing providers in `Infrastructure/ExternalApis/Providers/` for patterns

### 2. Probe the API (MANDATORY before writing any code)
Call each endpoint directly to inspect the actual response structure.
Use `fetch_webpage` or `run_in_terminal` with `curl`/`Invoke-RestMethod` for each URL.

```powershell
# Example: probe without auth
Invoke-RestMethod "https://api.example.com/endpoint" | ConvertTo-Json -Depth 6

# Example: probe with Bearer token
Invoke-RestMethod "https://api.example.com/endpoint" -Headers @{ Authorization = "Bearer $token" } | ConvertTo-Json -Depth 6

# Example: probe with pagination
Invoke-RestMethod "https://api.example.com/endpoint?page=1" | ConvertTo-Json -Depth 4
```

**Extract from the real response:**
- Envelope shape: is data wrapped in `{ status, data: { list: { results, count, next, total_pages } } }` or flat array?
- Field names and casing: `snake_case`, `camelCase`, `PascalCase`?
- Nullable fields: which fields can be null/missing?
- Types: numbers as strings or actual numbers?
- Pagination fields: `next`, `total_pages`, `page_number`, `count`
- Any auth headers or query params required?

**Only after probing** define the internal DTOs (`InternalXxx`) in the handler to match the real shape exactly.
If the API is unreachable from the dev machine, fall back to the OpenAPI/YAML spec in `docs/KiotaClient/{Provider}/`.

### 3. Configuration
Add to `appsettings.json`:
```json
{
  "ExternalApis": {
    "Providers": {
      "{ProviderName}": {
        "{EndpointName}": {
          "Url": "https://api.example.com/endpoint",
          "UseCache": true,
          "CacheDurationSeconds": 300,
          "UseProxy": false,
          "TimeoutSeconds": 30
        }
      }
    }
  }
}
```

### 4. Provider Class
- Create `Infrastructure/ExternalApis/Providers/{Provider}ApiProvider.cs`
- Inject `ExternalApiService` and `IOptions<ExternalApisOptions>`
- Build `ApiRequest` from endpoint config
- Use `_apiService.ExecuteAsync<T>()` for all calls

### 5. Service Interface
- Add method to `IExternalDataService` in Application layer
- Implement in `ExternalDataService` in Infrastructure

### 6. DTOs
- Create in `Application/Features/ExternalData/{Feature}/Queries/`
- Map external API response to application DTOs
- Internal DTOs (`InternalXxx`) must match the **actual** response shape observed in step 2
- Use `[JsonPropertyName]` for every snake_case field

### 7. Query Handler
- Create in Application layer
- Inject `IExternalDataService`
- Implement `ICacheableQuery` if appropriate

### 8. Endpoint
- Create thin endpoint in Presentation
- MediatR delegation only
- Add authorization: `.RequireAuthorization(Permissions.X)`

### 9. Register
- Register provider in `Infrastructure/DependencyInjection.cs`
- `services.AddScoped<{Provider}ApiProvider>()`

## Requirements
- **Always probe the live API** (step 2) before writing DTOs — never guess field shapes from docs alone
- Application uses `IExternalDataService` interface only
- Never hardcode URLs
- Provider reads from `IOptions<ExternalApisOptions>`
- Register `ExternalApiResilienceHandler` as Transient
