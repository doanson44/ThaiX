# Demo endpoint catalog (ThaiX.Client)

Inventory of REST paths called from `src/ThaiX.Client/Services/**` and how demo mode answers them.

## Re-scan (avoid coverage regression)

From repo root:

```powershell
rg -o '"api/[^"]+"' src/ThaiX.Client/Services --glob '*.cs' | Sort-Object -Unique
```

Also check interpolated paths in `ExternalDataService` (`api/external-data/...`).
When a new path appears: add a typed handler in `DemoResponseFactory`, Bogus seed if it is a list, mark status here.

Status legend:

| Status | Meaning |
|---|---|
| **Covered** | Typed envelope/DTO matching client reader; primary page load + main actions work |
| **Partial** | Responds but some mutations/filters are shells only |
| **Missing** | Should be zero for HttpClient REST (MEXC WebSocket excluded) |

Force demo: `wwwroot/appsettings.json` → `Api:ForceDemo: true` (Development may set `false` to hit a live API).

| Path | Status | Demo shape / notes |
|---|---|---|
| `health` / `api/health` | Covered | plain text |
| `api/auth/*` | Covered | LoginResponse / UserInfoResponse / Success |
| `api/account/login-2fa\|login-recovery` | Covered | LoginResponse |
| `api/account/2fa-status` | Covered | TwoFactorStatusResponse |
| `api/account/enable-authenticator` | Covered | EnableAuthenticatorResponse |
| `api/account/verify-authenticator` | Covered | VerifyAuthenticatorResponse |
| `api/account/generate-recovery-codes` | Covered | GenerateRecoveryCodesResponse |
| `api/account/external-logins` | Covered | ExternalLoginsResponse |
| `api/account/personal-data` | Covered | `Dictionary<string,string>` |
| `api/account/*` other | Partial | Success shells (confirm-email, forgot/reset, disable-2fa, …) |
| `api/master-data/countries\|cities\|districts\|banks` | Covered | Paged ≥80 + CRUD; filter by `search` / `searchTerm` on code/name; cities/districts/banks also `parentCode` |
| `api/master-data/symbols/search` | Covered | `SymbolSearchResultDto` (Items + TotalCount); stock/crypto lists |
| `api/users/*` | Covered | Paged users ≥80, permissions, profile |
| `api/contacts` | Covered | Paged ≥80, search, ContactDetailDto, CSV export/import/template, suggest-users |
| `api/contacts/import-async` | Covered | Returns job `Guid`; stores `ContactImportJobDto` |
| `api/contacts/import/jobs/{id}` | Covered | `ContactImportJobDto` (+ synthetic completed if unknown) |
| `api/contacts` child resources | Partial | Success / Guid shells for emails/phones/… |
| `api/files/upload` | Covered | **Raw** `UploadedFileDto` JSON (not ApiResponse) + `DemoImageUrls` |
| `api/files/{id}` GET | Covered | `UploadedFileDto` in ApiResponse |
| `api/files/{id}` DELETE | Covered | Success |
| `api/portfolios` (+ export / import / template) | Covered | Paged ≥80, search, CRUD, CSV |
| `api/stock\|crypto\|saving-positions` | Covered | Paged ≥80, `portfolioId` + `search`/`searchTerm` filter; `{id}/transactions` paged |
| `api/notes` | Covered | Paged + search/pin/archive filters; pin/archive toggles store |
| `api/credential-accounts` | Covered | Paged; detail; password view/copy → CredentialAccountPasswordDto; audit-logs seeded |
| `api/blog/posts` | Covered | Admin list + PostDto by id |
| `api/blog/posts/public` (+ `/{slug}`) | Covered | PublishedPosts page + PostDto by slug |
| `api/blog/categories` | Covered | SuccessData list (not paged) |
| `api/blog/tags` | Covered | Paged + search |
| `api/blog/ai/*` | Covered | BlogAiTextResponseDto / SEO / Review |
| `api/notification-schedules` | Covered | List/filter; NotificationScheduleDto GET/POST; run-now / preview / executions |
| `api/notification-schedules/upcoming` | Covered | List of upcoming schedule items |
| `api/notification-schedules/{id}/resume` | Covered | Success; status → Active |
| `api/price-alerts`, `api/market-scanner` | Covered | Paged ≥80 + `search`/`searchTerm` filter |
| `api/api-clients` | Covered | List/scopes; CreateApiClientResponse; regenerate-secret; activate/deactivate |
| `api/json-bins` | Covered | Paged search; CRUD; code-exists; by-code; content; expire; share/revoke |
| `api/public/json-bins/share/{token}` | Covered | Anonymous shared bin payload |
| `api/external-data/market/commodities\|currencies\|cryptocurrencies` | Covered | nested `*ResponseDto.Data` ≥80 |
| `api/external-data/market/coingecko/coins-list` | Covered | nested list ≥80 |
| `api/external-data/market/coingecko/markets/{id}` | Covered | CoinGeckoCoinMarketResponseDto |
| `api/external-data/bank-interest-rates` | Covered | nested grid ≥80 |
| `api/external-data/sacombank/exchange-rates` | Covered | nested rates ≥80 |
| `api/external-data/market/vndirect/change-prices` | Covered | VnDirectChangePricesResponseDto.Data ≥80 |
| `api/external-data/market/vndirect/top-stocks` | Covered | VnDirectTopStocksResponseDto.Data ≥80 |
| `api/external-data/mexc/contract-ticker` | Covered | MexcContractTickersResponseDto.Data ≥80 |
| `api/external-data/mexc/spot-ticker-24hr` | Covered | MexcSpotTicker24HrResponseDto.Data ≥80 |
| `api/external-data/mexc/contract-depth\|spot-depth/{symbol}` | Covered | OrderBookDepthResponseDto |
| `api/external-data/mexc/contract-kline\|spot-klines/{symbol}` | Covered | KlineSeriesResponseDto |
| `api/external-data/mexc/funding-rate` | Covered | FundingRatesResponseDto |
| `api/external-data/binance/funding-rates` | Covered | FundingRatesResponseDto |
| `api/external-data/binance/*-ticker-24hr` | Covered | ExchangeTicker24HrResponseDto |
| `api/external-data/binance/*-depth/{symbol}` | Covered | OrderBookDepthResponseDto |
| `api/external-data/bybit/linear-tickers\|spot-tickers` | Covered | ExchangeTicker24HrResponseDto |
| `api/external-data/market/price-history/{symbol}` | Covered | StockPriceHistoryResponseDto |
| `api/external-data/market/watchlist-price/{symbol}` | Covered | WatchlistPriceResponseDto |
| `api/external-data/market/vndirect/events\|ratios\|recommendations\|stock-prices\|technical-signals` | Covered | Typed VnDirect*ResponseDto |
| `api/external-data/market/24hmoney/transactions/{symbol}` | Covered | TwentyFourHMoneyTransactionsResponseDto (live list) |
| `api/external-data/lottery/power-655` | Covered | Power655ResultsResponseDto |
| `api/external-data/yahoo-finance/vix` | Covered | VixResponseDto |
| `api/external-data/yahoo-finance/vix/verdict` | Covered | VixVerdictDto (POST) |
| `api/external-data/tcbs-top10/portfolios` | Covered | TcbsTop10PortfoliosResult (≥80) |
| `api/external-data/tcbs-top10/sync` | Covered | SuccessData sync object |
| `api/external-data/dragon-capital/fund-portfolio/{code}` | Covered | DragonCapitalFundPortfolioDto |
| `api/external-data/market/24hmoney/transactions/{symbol}/history` | Covered | TwentyFourHMoneyTransactionHistoryResponseDto |
| `api/external-data/chainbroker/db/funds\|projects\|unlocks` | Covered | Paged ≥80 |
| `api/external-data/chainbroker/sync*` | Covered | SuccessData sync object |
| `api/trading/suggestion` | Covered | TradeSuggestionDto |
| `api/trading/verdict` | Covered | TradeVerdictDto |
| `api/trading/suggestion-history` | Covered | WeeklySuggestionHistoryResultDto (≥80 reports) |
| `api/trading/suggestion-history/evaluate` | Covered | EvaluateWeeklySuggestionPerformanceResultDto |
| `api/trading/suggestion-history/export` | Covered | CSV bytes + Content-Disposition |
| `api/trading/weekly-suggestion/run\|run-spot` | Covered | SuccessData started |
| `api/lottery/power-655/analysis` | Covered | Power655AnalysisResponse (draws ≥80) |
| `api/lottery/power-655/sync\|predict` | Covered | Success |
| `api/resumes/mine`, `public/{slug}` | Covered | ResumeDto |
| `api/resumes/ai/*` | Covered | ResumeAiTextResponse / Report |
| `api/ai/generate` | Covered | GenerateAiTextResponse |
| `api/bot-commands/execute` | Covered | BotCommandExecutionDto |
| `api/slack\|telegram/commands/channels` | Covered | ChannelInfoDto list ≥80 |
| `api/slack/commands/execute` | Covered | BotCommandExecutionDto |
| `api/slack/commands/executions/{id}` | Covered | BotAsyncExecutionStatusDto |
| `api/notifications/send` | Covered | Success (no data) |
| `api/notifications/{id}` | Covered | NotificationDetailDto |
| `api/notifications/preferences/{userId}` | Covered | List of UserNotificationPreferenceDto |
| `api/notifications/preferences` PUT | Covered | Upsert into session store |

**Skipped (not HttpClient product surface):** `api/account/external-login-callback` (browser redirect), `api/telegram/webhook` (server inbound).

**Missing:** none for HttpClient REST paths (MEXC WebSocket pages excluded — not HTTP).
