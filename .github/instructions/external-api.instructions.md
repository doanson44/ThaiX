---
description: "Use when integrating external APIs, creating API providers, or working with ExternalApiService, HttpClient resilience, retry policies, or external data features."
---

# External API Integration Rules

## Architecture

```
Endpoint -> MediatR -> Handler (Application) -> IExternalDataService -> Provider (Infrastructure) -> ExternalApiService -> Transport -> External API
```

## Built-in Features (Automatic)

- **Retry**: 3 exponential backoff attempts (2s, 4s, 8s) for 5xx, network failures, timeouts.
- **Timeout**: Configurable per-endpoint (default 30s), enforced before retry.
- **Caching**: Deterministic SHA256 keys, configurable duration.
- **Rate Limiting**: Per-host concurrent limits (default 10).
- **Logging**: Request/response tracking, duration, retry attempts, correlation IDs.

## Integration Steps

1. Add config in `appsettings.json` under `ExternalApis.Providers.[Name]`.
2. Create provider in `Infrastructure/ExternalApis/Providers/`.
3. Register provider in `DependencyInjection.cs`.
4. Create DTOs in `Application/Features/ExternalData/{Feature}/Queries/`.
5. Add interface method to `IExternalDataService`.
6. Implement in `ExternalDataService`.
7. Create query handler in Application (uses `IExternalDataService`).
8. Create thin endpoint in Presentation.

## Configuration Template

```json
{
  "ExternalApis": {
    "Providers": {
      "YourProvider": {
        "YourEndpoint": {
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

## Requirements

- Application uses `IExternalDataService` interface only (no Infrastructure deps).
- Never hardcode URLs -- always use configuration.
- Provider reads from `IOptions<ExternalApisOptions>`.
- Register `ExternalApiResilienceHandler` as Transient in DI.

## Prohibited

- No direct `HttpClient` in Application or Presentation.
- No hardcoded URLs.
- No manual retry logic.
- No manual cache key generation.
