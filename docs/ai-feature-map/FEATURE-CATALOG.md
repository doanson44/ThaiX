# ThaiX Feature Catalog

This catalog records feature areas present in the current repository. Verified means the folder/page/endpoint exists. Detailed business rules remain in Domain/Application implementations and tests.

## Market intelligence
### Market Data
- Client: src/ThaiX.Client/Pages/MarketData/
- Application: Features/ExternalData/, Features/TcbsTop10/, Features/Lottery/
- API: ExternalDataEndpoints.cs, LotteryEndpoints.cs
- Pages include bank deposit rates, bank interest rates, Sacombank exchange rates, Power655 analysis, TCBS Top 10 and MEXC ticker sockets.

### Market Research
- Client: src/ThaiX.Client/Pages/MarketResearch/
- Application: Features/ExternalData/
- API: ExternalDataEndpoints.cs
- Pages include Binance, Bybit, MEXC, CoinGecko, ChainBroker funds/projects/unlocks, Dragon Capital funds and VnDirect top stocks.

### TCBS Top 10 and Lottery
- TCBS: Client Pages/MarketData/TcbsTop10List.razor; Application Features/TcbsTop10; Domain Aggregates/TcbsTop10.
- Lottery: Client Pages/MarketData/Power655Analysis.razor; Application Features/Lottery; Domain Aggregates/Lottery; provider KetQuaDienToanApiProvider.

## Investment and trading
### Portfolios
- Client: Pages/Portfolio/Portfolios.razor, PortfolioDetail.razor
- Application: Features/Portfolios/
- Domain: Aggregates/Portfolios/
- API: PortfolioEndpoints.cs, /api/portfolios
- Verified operations: paged list, detail, create, update, delete, CSV import template/import/export.
- Permissions: PortfolioRead, PortfolioWrite, PortfolioDelete.

### Asset Positions
- Client: portfolio detail experience; trace child components and Client service before UI changes.
- Application: Features/AssetPositions/
- Domain: Aggregates/AssetPositions/
- API: StockPositionEndpoints.cs, CryptoPositionEndpoints.cs, SavingPositionEndpoints.cs
- Operations include stock/crypto/saving position lists, create/delete, stock/crypto transactions, target/stop-loss updates, saving position update and withdrawal.
- Permissions: PositionRead, PositionWrite, PositionDelete.

### Price Alerts
- Client: Pages/Portfolio/PriceAlerts.razor
- Application: Features/PriceAlerts/
- Domain: Aggregates/PriceAlerts/
- API: PriceAlertEndpoints.cs, /api/price-alerts
- Operations: paged list, create, update, delete.
- Permissions: PriceAlertRead, PriceAlertWrite, PriceAlertDelete.
- Background checker: PriceAlertCheckerJob; inspect for trigger/delivery behavior.

### Market Scanner
- Client: Pages/Portfolio/MarketScannerRules.razor
- Application: Features/MarketScanner/
- Domain: Aggregates/MarketScanner/
- API: MarketScannerEndpoints.cs, /api/market-scanner/
- Operations: rule list/create/update/delete and exchange settings read/update.
- Permissions: MarketScannerRuleRead, MarketScannerRuleWrite, MarketScannerRuleDelete.
- Background job: MexcMarketScannerJob.

### Trading Suggestions
- Application: Features/Trading/
- Domain: Aggregates/TradingSuggestions/
- API: TradingEndpoints.cs, /api/trading
- Operations: suggestion, AI verdict, suggestion history, performance evaluation, template export and manual weekly-job triggers.
- Permission: TradingView.
- POST /verdict calls IAiTextGenerationService directly; source comment explains it does not persist data and does not use MediatR.
- Background jobs include weekly stock and MEXC spot suggestions.

## Personal finance
### Expense Tracker
- Client: Pages/Portfolio/ExpenseTracker.razor
- Application: Features/ExpenseTracker/
- Domain: Aggregates/ExpenseTracker/
- API: ExpenseTrackerEndpoints.cs
- Application areas include budgets, categories, tags, transactions, recurring transactions, saving goals, transfers and wallets.
- Inspect aggregate methods and handlers for balance, recurrence and transfer invariants.

## CRM and notes
### Contacts
- Client: Pages/Crm/Contacts.razor
- Application: Features/Contacts/
- Domain: Aggregates/Contacts/ and Aggregates/ContactImport/
- API: ContactEndpoints.cs, /api/contacts
- Operations include profile and nested emails, phones, addresses, social links, tags, bank accounts, identity documents and custom fields; avatar upload/removal; archive; CSV import/export; async import job status.
- Endpoint describes a Google Contacts-compatible CSV template. Inspect the import handler for matching/upsert rules.

### Notes
- Client: Pages/Crm/Notes.razor
- Application: Features/Notes/
- Domain: Aggregates/Notes/
- API: NoteEndpoints.cs, /api/notes
- Operations: list/detail/create/update/delete, pin toggle and archive toggle.
- Permissions: NoteRead, NoteWrite, NoteDelete.

## Notifications and bot commands
### Notifications
- Client: Pages/Notification/NotificationSender.razor
- Application: Features/Notifications/
- Domain: Aggregates/Notifications/
- API: NotificationEndpoints.cs, /api/notifications
- Operations: send, retrieve lifecycle/delivery status, read preferences and upsert preferences.
- Endpoint docs describe Slack, Telegram or both routing; inspect Infrastructure sender services for exact behavior.
- Inspected send/preferences endpoints require SystemAdmin.

### Notification Schedules
- Client: Pages/Notification/NotificationSchedules.razor
- Application: Features/Notifications/Scheduling/
- Domain: Aggregates/Notifications/
- API: NotificationScheduleEndpoints.cs, /api/notification-schedules
- Operations: list/detail/create/update/delete, activate/pause/resume/disable, run-now, execution history, upcoming schedules and occurrence preview.
- Endpoint group uses RequireAuthorization(); inspect policy setup and tests for effective policy.

### Bot Commands
- Application: Features/BotCommands/
- API: BotCommandEndpoints.cs
- Shared abstractions include parser, catalog, registry, intent resolver, response narrator, conversational responder, user mapping and Slack/Telegram conversation history.
- Registered modules include help, crypto/stock prices and indicators, market summary, signals, portfolio, alerts, notes and TCBS.
- Read parser/catalog/module implementations for supported command syntax.

## Content and publishing
### Blog
- Client: Pages/Admin/Blog/ and Pages/Public/Blog/
- Application: Features/Blog/Categories/, Posts/, Tags/
- Domain: Aggregates/Blog/
- API: BlogCategoryEndpoints.cs, BlogPostEndpoints.cs, BlogTagEndpoints.cs, BlogAiEndpoints.cs
- Post operations include create/update, publish/unpublish, archive/restore, schedule, duplicate and delete.
- Background publisher: ScheduledPostPublisherJob.

### Public HTML Pages
- Client: Pages/Admin/PublicHtmlPages.razor
- Application: Features/PublicHtml/
- Domain: Aggregates/PublicHtml/
- API: PublicHtmlEndpoints.cs
- Operations include upload, rename, publish state, delete, view recording, list and open published content.

### Resumes
- Client: Pages/Administration/ResumeEditor.razor and Pages/Public/Resume.razor
- Application: Features/Resumes/
- Domain: Aggregates/Resumes/
- API: ResumeEndpoints.cs, ResumeAiEndpoints.cs
- Application has upsert, current-user retrieval and slug retrieval.

### JSON Bins
- Client: Pages/Admin/JsonBins/ and Pages/Public/JsonBinShareView.razor
- Application: Features/JsonBins/
- Domain: Aggregates/JsonBin/
- API: JsonBinEndpoints.cs, JsonBinPublicEndpoints.cs
- Operations include create/update/delete/expire, share-link generation/revocation, read by code/id/content, shared read and code validation.
- Background cleanup: JsonBinExpiredCleanupJob.

## Identity, security and administration
### Authentication
- Client: Pages/Auth/
- Application: Features/Authentication/
- API: AuthenticationEndpoints.cs, plus account/token endpoints.
- Pages include login, registration, forgot/reset password, email confirmation, 2FA login, external login callback, lockout and access denied.
- Commands include login, client credentials, email OTP request/verify and login settings.
- Do not infer token TTL from JWT validation configuration.

### Users and permissions
- Client: Pages/Administration/Users.razor
- Application: Features/Users/, Features/Identity/
- API: UserEndpoints.cs, AccountEndpoints.cs
- Operations include user create/update, password reset, lockout, permission assignment and permission/user queries.
- Server permissions: Domain/Common/Constants/Permissions.cs.
- Client permissions: Client/Constants/PermissionNames.cs.
- Server authorization uses permission claims; Client navigation/policy registration currently checks scope claims. JWT validation copies non-empty scope values into permission claims when missing. Trace token issuance before changing this contract.

### API Clients
- Client: Pages/Security/ApiClients.razor
- Domain: Aggregates/ApiClient/
- API: ApiClientEndpoints.cs
- Trace handlers and permission definitions before changing client credential lifecycle.

### Credential Accounts
- Client: Pages/Security/CredentialAccounts.razor
- Application: Features/CredentialAccounts/
- Domain: Aggregates/CredentialAccounts/
- API: CredentialAccountEndpoints.cs
- Operations include create/update/delete, password change/retrieval, mark/reset used and audit-log query. Treat as sensitive; inspect encryption, authorization, audit and logging.

### Admin and developer tools
- Client: Pages/Administration/ includes AI tester, CMSX proxy tester, command tester, resume editor, system logs and users.
- Application: Features/DeveloperTools/, Features/SystemLogs/
- API: AiEndpoints.cs, CmsxProxyTesterEndpoints.cs, SystemLogEndpoints.cs, Slack/Telegram endpoints.
- Access control is endpoint-specific; do not infer it from the page location.

## Master data and utilities
### Master Data
- Client: Pages/MasterData/ (banks, cities, countries, districts)
- Application: Features/MasterData/
- Domain: Aggregates/MasterData/
- API: MasterDataEndpoints.cs
- Application contains CRUD/import/query groups for banks, cities, countries and districts.

### Games
- Client: Pages/Games/ includes a games hub and individual games.
- Application: Features/Games/
- API: GameEndpoints.cs
- Trace game services, persistence and tests for exact behavior.

### Files
- Application: Features/Files/
- API: FileEndpoints.cs
- Inspect storage, authorization and upload/download validation before changes.

### System Logs
- Client: Pages/Administration/SystemLogs.razor
- Application: Features/SystemLogs/
- API: SystemLogEndpoints.cs
- Queries include file listing, preview and download.

### Tools
- Client: Pages/Tools/
- Inspect the tool host and component registrations for the actual catalog.

## Domain aggregate inventory
ApiClient, AssetPositions, Blog, ChainBroker, ContactImport, Contacts, CredentialAccounts, ExpenseTracker, JsonBin, Lottery, MarketData, MarketScanner, MasterData, Notes, Notifications, Outbox, Portfolios, PriceAlerts, PublicHtml, Resumes, TcbsTop10, TradingSuggestions.

Application feature folders and Domain aggregates are not guaranteed to be one-to-one. Do not create an aggregate merely to mirror an Application folder.
