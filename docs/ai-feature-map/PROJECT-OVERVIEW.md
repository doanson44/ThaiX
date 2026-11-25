# ThaiX Project Overview

## Product
ThaiX is a personal finance and market-intelligence platform. It aggregates crypto, equity, fund, banking and related market data; manages portfolios and asset positions; supports trading suggestions and price alerts; and provides personal finance, CRM, notification, publishing and administrative capabilities.

The repository also contains utility and engagement features such as JSON Bins, lottery analysis, games, resumes and developer tools. Do not assume every module belongs to the investment domain.

## Solution topology
- src/ThaiX.Domain: aggregates, entities, value objects and domain events.
- src/ThaiX.Application: CQRS requests, handlers, validators, DTOs and interfaces.
- src/ThaiX.Infrastructure: EF Core, providers, Identity, Hangfire and persistence.
- src/ThaiX.Presentation: ASP.NET Core host, Minimal API, middleware and API contracts.
- src/ThaiX.Client: Blazor WebAssembly UI.
- tests: four test projects listed in SOURCE-INDEX.md.

Dependency direction:
Domain <- Application <- Infrastructure <- Presentation
Client -> Presentation API

Domain has no outward project dependencies. Application depends on Domain. Infrastructure depends on Application and Domain. Presentation depends on Application and Infrastructure. Client consumes API contracts; do not expose EF or Domain entities to Client.

## Runtime baseline
Verified against global.json, Directory.Packages.props, project files and thaix-docs/VERIFIED-RUNTIME-FACTS.md:
- Root SDK pin: .NET SDK 11.0.100, rollForward latestMinor.
- Application and test projects target net11.0; default language mapping is C# 15.
- ASP.NET Core and EF Core packages are on the 11.0.0 line.
- Oracle MySql.EntityFrameworkCore is 11.0.0-preview (prerelease).
- Integration tests use Testcontainers.MariaDb and MariaDB 11.4.

## Database provider split
| Concern | Configuration | Provider |
|---|---|---|
| Application and Identity database | DefaultConnection | Oracle MySQL EF provider via UseMySQL; MariaDbHistoryRepository |
| Hangfire storage | HangfireConnection | SQL Server via UseSqlServerStorage |
| Presentation integration fixture | Testcontainers.MariaDb | MariaDB 11.4 |

When Hangfire is enabled, missing HangfireConnection causes configuration/startup failure; it does not fall back to DefaultConnection.

Provider risk: the Oracle EF Core 11 provider is prerelease. Restore/build success is not proof of production compatibility. Validate migrations, generated SQL, query translation, transactions and concurrency against the actual MariaDB/MySQL server. Do not apply migrations without explicit authorization.

## Request lifecycle
Typical command:
Client page/component -> Client API service -> Minimal API endpoint -> MediatR command -> Application handler/validator -> IApplicationDbContext or Application interface -> Infrastructure/EF Core -> database -> API contract -> Client UI.

Typical query:
Client page/component -> Client API service -> Minimal API endpoint -> MediatR query -> Application handler -> DTO projection -> Infrastructure or external provider -> API contract -> Client view.

These are navigation patterns, not guarantees for every endpoint. Inspect specialized direct passthroughs, public content, AI generation, file streaming and Hangfire triggers individually.

## MediatR pipeline
Verified in src/ThaiX.Application/DependencyInjection.cs. Order is significant:
1. LoggingBehavior
2. ValidationBehavior
3. TransactionBehavior
4. QueryCachingBehavior
5. CommandCacheInvalidationBehavior
6. Request handler

Do not bypass or reorder these behaviors.

## Host startup and endpoint registration
- src/ThaiX.Presentation/Program.cs configures services, registers Application and Infrastructure, initializes the database, configures middleware, maps API endpoints and the SPA fallback.
- src/ThaiX.Presentation/Extensions/WebApplicationExtensions.cs owns middleware configuration, MapApiEndpoints(), Hangfire recurring-job scheduling and host setup.
- MapApiEndpoints() explicitly registers endpoint mapping methods. A new endpoint class must be registered there.
- Hangfire recurring jobs are scheduled after application startup from configuration.

## Cross-cutting systems
| Concern | Primary source location | Guidance |
|---|---|---|
| Domain model | src/ThaiX.Domain/Aggregates/ | Aggregate invariants and domain events |
| CQRS and validation | src/ThaiX.Application/Features/ | Commands, queries, handlers, validators and DTOs |
| Persistence | src/ThaiX.Infrastructure/Persistence/ | DbContext, EF configurations, migrations and interceptors |
| External data | src/ThaiX.Infrastructure/ExternalApis/ | Providers, options, transport, parsing and resilience |
| Identity | src/ThaiX.Infrastructure/Identity/ | ASP.NET Core Identity boundary |
| Authorization | Domain/Common/Constants/Permissions.cs and Client constants | Server checks are authoritative |
| Background work | src/ThaiX.Infrastructure/BackgroundJobs/ | Hangfire only; no Task.Run or BackgroundService |
| Outbox | Domain Outbox aggregate and Infrastructure interceptors/processor | Preserve event -> Outbox -> Hangfire -> MediatR handler |
| Caching | Application query contracts/behaviors and Infrastructure | Cache queries; invalidate through command attributes |
| Localization | Presentation and Client resources | English/Vietnamese resources at boundary layers |
| Logging | Serilog and Presentation middleware | Preserve correlation; never log secrets/tokens/PII |

## External API path
Presentation endpoint -> Application query/handler -> IExternalDataService -> Infrastructure provider -> ExternalApiService -> HTTP transport/resilience/cache/concurrency/logging -> external API.

Use IOptions<ExternalApisOptions> and ExternalApiService.ExecuteAsync<T>(). Probe the upstream response before defining transport records. Do not guess payload shape or hardcode provider URLs.

## Background jobs and Outbox
Source-registered job families include:
- Outbox processing and old Outbox message cleanup.
- ChainBroker funds, projects and unlocks synchronization.
- iWealthClub Top 10 synchronization.
- Power655 synchronization and prediction.
- 24hMoney transaction synchronization.
- Weekly stock and MEXC spot suggestion jobs.
- MEXC market scanner.
- Price alert checker.
- Scheduled blog post publisher.
- Due notification schedule polling.
- Expired JSON Bin cleanup.
- Email OTP challenge cleanup.

Inspect HangfireJobIds, job implementations and configuration before changing cadence or behavior.

## Agent discovery surfaces
The Presentation host maps agent/crawler discovery endpoints. The root README documents /robots.txt, /sitemap.xml, /api.md, /auth.md and /.well-known/api-catalog. The homepage supports Accept: text/markdown and RFC 8288 Link headers.

These surfaces do not imply MCP, WebMCP, OAuth/OIDC discovery, protected-resource metadata or agent registration support.
