# ThaiX Source Index

## Repository entry points
| Path | Purpose |
|---|---|
| README.md | Product, setup, architecture and public discovery |
| AGENTS.md | Mandatory conventions and agent workflow |
| global.json | SDK pin |
| Directory.Packages.props | Central package versions |
| ThaiX.slnx | Solution projects |
| .agents/rules/ | Cross-cutting rules |
| .agents/skills/ | Task-specific workflows |
| thaix-docs/ | Runtime facts and documentation audit |
| docs/ai-feature-map/ | Maintained AI navigation map |

## Project entry points
| Project | Key paths |
|---|---|
| Domain | src/ThaiX.Domain/Aggregates/, Common/ |
| Application | src/ThaiX.Application/Features/, Common/ |
| Application DI/pipeline | src/ThaiX.Application/DependencyInjection.cs |
| Infrastructure | src/ThaiX.Infrastructure/ |
| Infrastructure DI | src/ThaiX.Infrastructure/DependencyInjection.cs |
| Persistence | src/ThaiX.Infrastructure/Persistence/ |
| External providers | src/ThaiX.Infrastructure/ExternalApis/Providers/ |
| Background jobs | src/ThaiX.Infrastructure/BackgroundJobs/ |
| Presentation host | src/ThaiX.Presentation/Program.cs |
| API endpoints | src/ThaiX.Presentation/Endpoints/ |
| Endpoint/middleware mapping | src/ThaiX.Presentation/Extensions/WebApplicationExtensions.cs |
| Client pages | src/ThaiX.Client/Pages/ |
| Client components | src/ThaiX.Client/Components/ |
| Client services | src/ThaiX.Client/Services/ |

## Application feature folders
AssetPositions, Authentication, Blog, BotCommands, Contacts, CredentialAccounts,
DeveloperTools, ExpenseTracker, ExternalData, Files, Games, Identity, JsonBins,
Lottery, MarketScanner, MasterData, Notes, Notifications, Portfolios, PriceAlerts,
PublicHtml, Resumes, SystemLogs, TcbsTop10, Trading, Users.

## Domain aggregate folders
ApiClient, AssetPositions, Blog, ChainBroker, ContactImport, Contacts, CredentialAccounts,
ExpenseTracker, JsonBin, Lottery, MarketData, MarketScanner, MasterData, Notes,
Notifications, Outbox, Portfolios, PriceAlerts, PublicHtml, Resumes, TcbsTop10,
TradingSuggestions.

Application feature folders and Domain aggregate folders are not one-to-one. Identity is an Infrastructure boundary; do not model ApplicationUser as a Domain aggregate.

## Presentation endpoint classes
AccountEndpoints.cs
AgentDiscoveryEndpoints.cs
AgentDiscoveryMetadataEndpoints.cs
AiEndpoints.cs
ApiClientEndpoints.cs
AuthenticationEndpoints.cs
BlogAiEndpoints.cs
BlogCategoryEndpoints.cs
BlogPostEndpoints.cs
BlogTagEndpoints.cs
BotCommandEndpoints.cs
CmsxProxyTesterEndpoints.cs
ContactEndpoints.cs
CredentialAccountEndpoints.cs
CryptoPositionEndpoints.cs
ExpenseTrackerEndpoints.cs
ExternalDataEndpoints.cs
FileEndpoints.cs
GameEndpoints.cs
HealthEndpoints.cs
JsonBinEndpoints.cs
JsonBinPublicEndpoints.cs
LotteryEndpoints.cs
MarketScannerEndpoints.cs
MasterDataEndpoints.cs
NoteEndpoints.cs
NotificationEndpoints.cs
NotificationScheduleEndpoints.cs
PortfolioEndpoints.cs
PriceAlertEndpoints.cs
PublicConfigurationEndpoints.cs
PublicHtmlEndpoints.cs
ResumeAiEndpoints.cs
ResumeEndpoints.cs
SavingPositionEndpoints.cs
SlackEndpoints.cs
StockPositionEndpoints.cs
SystemLogEndpoints.cs
TelegramEndpoints.cs
TokenEndpoints.cs
TradingEndpoints.cs
UserEndpoints.cs

These mapping methods are registered in WebApplicationExtensions.MapApiEndpoints(). Public configuration/discovery mapping is also invoked separately from Program startup.

## External API providers
Located in src/ThaiX.Infrastructure/ExternalApis/Providers/:
BinanceApiProvider, BybitApiProvider, CafeFApiProvider, ChainBrokerApiProvider,
CoinGeckoApiProvider, DragonCapitalApiProvider, IWealthClubApiProvider,
InvestingApiProvider, KetQuaDienToanApiProvider, MexcApiProvider, SacombankApiProvider,
TwentyFourHMoneyApiProvider, VnDirectApiProvider, VnExpressApiProvider, YahooFinanceApiProvider.

Other integration infrastructure lives under ExternalApis/Core, DTOs, Options, Parsing, Playwright, Resilience and Transport.

## Client pages
Current page areas: Account, Admin, Administration, Auth, Crm, Games, MarketData,
MarketResearch, MasterData, Notification, Portfolio, Public, Security and Tools.

## Client service areas
Ai, Api, ApiClients, AssetPositions, Auth, Blog, Breadcrumb, Caching, Contacts,
CredentialAccounts, ExpenseTracker, ExternalData, Files, Games, Http, JsonBins,
Lottery, MarketData, MarketScanner, MasterData, Notes, Notifications, Portfolios,
PriceAlerts, PublicHtml, Resumes, Slack, Tools, Ui and Users.

## Constants and contracts
- Server permissions: src/ThaiX.Domain/Common/Constants/Permissions.cs.
- Client permissions: src/ThaiX.Client/Constants/PermissionNames.cs.
- Client resource keys: src/ThaiX.Client/Constants/ResourceKeys.cs.
- Client route segments: src/ThaiX.Client/Constants/RouteSegments.cs.
- Application contracts: src/ThaiX.Application/Common/Models/, Interfaces/, Constants/.
- Presentation envelopes: src/ThaiX.Presentation/Models/.
- External API options: src/ThaiX.Infrastructure/ExternalApis/Options/.

## Tests
| Project | Scope |
|---|---|
| tests/ThaiX.Application.UnitTests/ | Application handlers, validators and unit behavior |
| tests/ThaiX.Client.UnitTests/ | Client logic |
| tests/ThaiX.Client.VisualTests/ | Browser and visual UI tests |
| tests/ThaiX.Presentation.IntegrationTests/ | API integration and MariaDB-backed persistence |

Integration fixture: tests/ThaiX.Presentation.IntegrationTests/Infrastructure/ThaiXWebApplicationFactory.cs. It uses Testcontainers.MariaDb and MariaDB 11.4.

## Discovery tips
- Find endpoint class and verify its mapping is called by MapApiEndpoints().
- Follow the request type to handler and validator.
- Search tests by command/query/endpoint name, not only feature folder.
- Search Client service usages to verify which API route a page actually calls.
- For provider work, trace handler -> IExternalDataService -> provider registration -> ExternalApiService.
- For scheduled behavior, trace HangfireJobIds -> scheduler registration -> job implementation.
