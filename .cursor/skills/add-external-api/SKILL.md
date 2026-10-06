---
name: add-external-api
description: "Integrate a third-party API end-to-end with provider configuration, service contracts, DTOs, query handlers, and endpoints. Use when adding or expanding external API integrations."
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

### 2. Configuration
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

### 3. Provider Class
- Create `Infrastructure/ExternalApis/Providers/{Provider}ApiProvider.cs`
- Inject `ExternalApiService` and `IOptions<ExternalApisOptions>`
- Build `ApiRequest` from endpoint config
- Use `_apiService.ExecuteAsync<T>()` for all calls

### 4. Service Interface
- Add method to `IExternalDataService` in Application layer
- Implement in `ExternalDataService` in Infrastructure

### 5. DTOs
- Create in `Application/Features/ExternalData/{Feature}/Queries/`
- Map external API response to application DTOs

### 6. Query Handler
- Create in Application layer
- Inject `IExternalDataService`
- Implement `ICacheableQuery` if appropriate

### 7. Endpoint
- Create thin endpoint in Presentation
- MediatR delegation only
- Add authorization: `.RequireAuthorization(Permissions.X)`

### 8. Register
- Register provider in `Infrastructure/DependencyInjection.cs`
- `services.AddScoped<{Provider}ApiProvider>()`

## Requirements
- Application uses `IExternalDataService` interface only
- Never hardcode URLs
- Provider reads from `IOptions<ExternalApisOptions>`
- Register `ExternalApiResilienceHandler` as Transient
