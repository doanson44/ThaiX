# ThaiX Feature Traceability

Use this document to find likely implementation paths, then inspect source and tests end-to-end. A directory mapping does not prove that a page uses a similarly named endpoint or service.

## Feature-to-layer matrix
| Feature | Client | Presentation endpoint | Application | Domain |
|---|---|---|---|---|
| Portfolios | Pages/Portfolio/Portfolios.razor, PortfolioDetail.razor | PortfolioEndpoints.cs | Features/Portfolios/ | Aggregates/Portfolios/ |
| Stock positions | Portfolio detail; trace components | StockPositionEndpoints.cs | Features/AssetPositions/ | Aggregates/AssetPositions/ |
| Crypto positions | Portfolio detail; trace components | CryptoPositionEndpoints.cs | Features/AssetPositions/ | Aggregates/AssetPositions/ |
| Saving positions | Trace Client component | SavingPositionEndpoints.cs | Features/AssetPositions/ | Aggregates/AssetPositions/ |
| Price alerts | Pages/Portfolio/PriceAlerts.razor | PriceAlertEndpoints.cs | Features/PriceAlerts/ | Aggregates/PriceAlerts/ |
| Market scanner | Pages/Portfolio/MarketScannerRules.razor | MarketScannerEndpoints.cs | Features/MarketScanner/ | Aggregates/MarketScanner/ |
| Trading suggestions | Trace Client components | TradingEndpoints.cs | Features/Trading/ | Aggregates/TradingSuggestions/ |
| Expense tracker | Pages/Portfolio/ExpenseTracker.razor | ExpenseTrackerEndpoints.cs | Features/ExpenseTracker/ | Aggregates/ExpenseTracker/ |
| Contacts | Pages/Crm/Contacts.razor | ContactEndpoints.cs | Features/Contacts/ | Aggregates/Contacts/, ContactImport/ |
| Notes | Pages/Crm/Notes.razor | NoteEndpoints.cs | Features/Notes/ | Aggregates/Notes/ |
| Notifications | Pages/Notification/NotificationSender.razor | NotificationEndpoints.cs | Features/Notifications/ | Aggregates/Notifications/ |
| Notification schedules | Pages/Notification/NotificationSchedules.razor | NotificationScheduleEndpoints.cs | Features/Notifications/Scheduling/ | Aggregates/Notifications/ |
| Bot commands | Admin tester / Slack / Telegram | BotCommandEndpoints.cs | Features/BotCommands/ | Trace event/aggregate ownership |
| Market data/research | Pages/MarketData/, Pages/MarketResearch/ | ExternalDataEndpoints.cs | Features/ExternalData/ | Aggregates/MarketData/ and related areas |
| TCBS Top 10 | Pages/MarketData/TcbsTop10List.razor | ExternalDataEndpoints.cs | Features/TcbsTop10/ | Aggregates/TcbsTop10/ |
| Lottery | Pages/MarketData/Power655Analysis.razor | LotteryEndpoints.cs | Features/Lottery/ | Aggregates/Lottery/ |
| Blog | Pages/Admin/Blog/, Pages/Public/Blog/ | Blog post/category/tag/AI endpoints | Features/Blog/ | Aggregates/Blog/ |
| Public HTML | Pages/Admin/PublicHtmlPages.razor | PublicHtmlEndpoints.cs | Features/PublicHtml/ | Aggregates/PublicHtml/ |
| Resume | Pages/Administration/ResumeEditor.razor, Pages/Public/Resume.razor | ResumeEndpoints.cs, ResumeAiEndpoints.cs | Features/Resumes/ | Aggregates/Resumes/ |
| JSON Bins | Pages/Admin/JsonBins/, Pages/Public/JsonBinShareView.razor | JsonBinEndpoints.cs, JsonBinPublicEndpoints.cs | Features/JsonBins/ | Aggregates/JsonBin/ |
| Authentication | Pages/Auth/ | AuthenticationEndpoints.cs, account/token endpoints | Features/Authentication/ | Identity boundary is Infrastructure |
| Users | Pages/Administration/Users.razor | UserEndpoints.cs, AccountEndpoints.cs | Features/Users/, Features/Identity/ | Identity boundary is Infrastructure |
| API clients | Pages/Security/ApiClients.razor | ApiClientEndpoints.cs | Trace related handlers | Aggregates/ApiClient/ |
| Credential accounts | Pages/Security/CredentialAccounts.razor | CredentialAccountEndpoints.cs | Features/CredentialAccounts/ | Aggregates/CredentialAccounts/ |
| Master data | Pages/MasterData/ | MasterDataEndpoints.cs | Features/MasterData/ | Aggregates/MasterData/ |
| Games | Pages/Games/ | GameEndpoints.cs | Features/Games/ | Trace aggregate ownership |
| Files | Trace Client usage | FileEndpoints.cs | Features/Files/ | Trace storage/domain ownership |
| System logs | Pages/Administration/SystemLogs.razor | SystemLogEndpoints.cs | Features/SystemLogs/ | Inspect storage boundary |
| Developer tools | Pages/Administration/AiTester.razor, CmsxProxyTester.razor, CommandTester.razor | AI/CMSX/Slack/Telegram endpoints | Features/DeveloperTools/, Features/BotCommands/ | Per-feature |

## Verified API route examples

### Portfolios
Source: src/ThaiX.Presentation/Endpoints/PortfolioEndpoints.cs
Base route: /api/portfolios
- GET / - paged list for current user; PortfolioRead.
- GET /{id:guid} - detail; PortfolioRead.
- POST / - create; PortfolioWrite.
- PUT /{id:guid} - update; PortfolioWrite.
- DELETE /{id:guid} - delete; PortfolioDelete.
- GET /import/template - CSV template; PortfolioRead.
- POST /import - CSV import; PortfolioWrite.
- GET /export - CSV export; PortfolioRead.

### Stock positions
Source: StockPositionEndpoints.cs; base route /api/stock-positions.
- GET / - list by portfolio/closed state.
- GET /{id}/transactions - paged transaction history.
- POST / - create.
- POST /{id}/transactions - add transaction.
- PUT /{id}/targets - update target and stop-loss.
- DELETE /{id} - delete.
Permission family: PositionRead, PositionWrite, PositionDelete.

### Crypto positions
Source: CryptoPositionEndpoints.cs; base route /api/crypto-positions. Route shape and permission family match stock positions, with crypto-specific request types.

### Saving positions
Source: SavingPositionEndpoints.cs; base route /api/saving-positions.
- GET / - list by portfolio/status.
- POST / - create.
- PUT /{id} - update.
- POST /{id}/withdraw - withdraw.
- DELETE /{id} - delete.

### Price alerts
Source: PriceAlertEndpoints.cs; base route /api/price-alerts.
- GET / - paged list with search/enabled filters.
- POST / - create.
- PUT /{id} - update.
- DELETE /{id} - soft-delete.
Permission family: PriceAlertRead, PriceAlertWrite, PriceAlertDelete.

### Notes
Source: NoteEndpoints.cs; base route /api/notes.
- GET / and GET /{id} - list/detail.
- POST /, PUT /{id}, DELETE /{id} - CRUD.
- POST /{id}/pin - toggle pin.
- POST /{id}/archive - toggle archive.
Permission family: NoteRead, NoteWrite, NoteDelete.

### Notifications
Source: NotificationEndpoints.cs; base route /api/notifications.
- POST /send - send.
- GET /{id} - lifecycle/delivery status.
- GET /preferences/{userId} - read preferences.
- PUT /preferences - upsert preference.
The inspected endpoints require SystemAdmin.

### Notification schedules
Source: NotificationScheduleEndpoints.cs; base route /api/notification-schedules.
- GET / and GET /{id} - list/detail.
- POST /, PUT /{id}, DELETE /{id} - management.
- POST /{id}/activate, /pause, /resume, /disable, /run-now.
- GET /{id}/executions, /upcoming, /{id}/preview.
The group calls RequireAuthorization(); inspect policy setup and tests for the effective policy.

### Market scanner
Source: MarketScannerEndpoints.cs.
- /api/market-scanner/rules - list/create/update/delete.
- /api/market-scanner/exchange-settings - get/update.
- Permissions: MarketScannerRuleRead, MarketScannerRuleWrite, MarketScannerRuleDelete.

### Trading
Source: TradingEndpoints.cs; base route /api/trading.
- GET /suggestion
- POST /verdict
- GET /suggestion-history
- POST /suggestion-history/evaluate
- GET /suggestion-history/export
- POST /weekly-suggestion/run
- POST /weekly-suggestion/run-spot
Permission: TradingView.

### Contacts and external data
- ContactEndpoints.cs maps /api/contacts including nested contact operations, CSV import/export and async import status. Read the file for the full route contract.
- ExternalDataEndpoints.cs maps groups under /api/external-data/. Read the file for exact routes and anonymous/permission requirements.

## Endpoint registration
src/ThaiX.Presentation/Extensions/WebApplicationExtensions.cs contains MapApiEndpoints(). It explicitly registers endpoint mapping methods. When adding an endpoint, register it there.
Preserve Permissions.* constants, correlation IDs and standard response envelopes where the endpoint family uses them. File streaming, no-content schedule operations and public endpoints may use different response shapes.

## Client source layout
Pages live under src/ThaiX.Client/Pages/. Feature areas include Account, Admin, Administration, Auth, Crm, Games, MarketData, MarketResearch, MasterData, Notification, Portfolio, Public, Security and Tools.

Client service areas under src/ThaiX.Client/Services/:
Ai, Api, ApiClients, AssetPositions, Auth, Blog, Breadcrumb, Caching, Contacts, CredentialAccounts, ExpenseTracker, ExternalData, Files, Games, Http, JsonBins, Lottery, MarketData, MarketScanner, MasterData, Notes, Notifications, Portfolios, PriceAlerts, PublicHtml, Resumes, Slack, Tools, Ui and Users.

Shared components are under src/ThaiX.Client/Components/. Examples include AsyncSearchSelect, DebouncedSearchBox, ClientSortBar, ResponsiveIconButton and shared UI controls. Reuse existing services/components before creating another abstraction.

## External API providers
Located in src/ThaiX.Infrastructure/ExternalApis/Providers/:
BinanceApiProvider, BybitApiProvider, CafeFApiProvider, ChainBrokerApiProvider, CoinGeckoApiProvider, DragonCapitalApiProvider, IWealthClubApiProvider, InvestingApiProvider, KetQuaDienToanApiProvider, MexcApiProvider, SacombankApiProvider, TwentyFourHMoneyApiProvider, VnDirectApiProvider, VnExpressApiProvider and YahooFinanceApiProvider.

Provider presence does not prove a page uses it. Trace Application handler, IExternalDataService, provider registration and transport.

## Trace a feature end-to-end
1. Locate Client page and reusable components.
2. Locate Client service/API helper and request/response DTOs.
3. Locate Presentation endpoint, route, authorization and response mapping.
4. Follow MediatR request to handler and validator.
5. Inspect Application interfaces, DTO projection and cache contracts.
6. Inspect Domain aggregate, value objects, events and invariants.
7. Inspect Infrastructure EF configuration, provider/service, persistence and jobs.
8. Read unit and integration tests.
9. Check localization, server/client permission constants, navigation guards and cache invalidation.
10. Update this map when routes, permissions, ownership or dependencies change.
