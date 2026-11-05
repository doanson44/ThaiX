# ThaiX Tools Quality Audit

## Scope

Reviewed the 50 entries in `ToolCatalog` on `feat/tools-last-used`.

| Category | Count |
|---|---:|
| Developer tools | 29 |
| Trading calculators | 12 |
| Risk calculators | 6 |
| Crypto calculators | 3 |
| **Total** | **50** |

This is a source-level audit with focused unit and browser test coverage. It is not a penetration test,
an exchange-specific financial model validation, or a performance benchmark.

## Implemented improvements

### Developer tools

| Area | Implemented behavior |
|---|---|
| JSON Formatter / Validator | Invalid JSON diagnostics include localized line and byte-column positions. Large-input formatting has a regression test. |
| JSON Compare | Compares canonical JSON and reports JSON paths, missing-versus-null values, and quoted paths for special property names. |
| JSON to C# / TypeScript | Sanitizes identifiers, quotes invalid TypeScript property names, resolves nested type/property collisions, rejects duplicate object keys recursively, and uses safe collection types for heterogeneous arrays. |
| CSV to JSON | Parses quoted fields, escaped quotes, multiline records, CRLF/LF/CR, and UTF-8 BOM. Rejects empty/duplicate headers, malformed quote placement, unterminated quotes, and excess columns. |
| YAML / XML | YAML parser diagnostics retain source positions; XML formatting preserves significant text whitespace. |
| SQL Formatter | Scans strings, quoted identifiers, bracket/backtick identifiers, and line/block comments before formatting keywords. Remains a lightweight formatter, not a dialect parser. |
| HTML to Markdown | Preserves common headings, links, emphasis, inline code, paragraphs, and lists. Remains a best-effort regex converter, not a full HTML5 parser. |
| Markdown preview security | Disables raw HTML parsing in Markdig and sanitizes generated HTML with HtmlSanitizer. Mermaid uses strict security mode. |
| JWT decoder | Shows a localized UNVERIFIED warning and explicit signature-present/missing state. Decoding does not imply signature verification. |
| Cron | Validates five fields, wildcards, steps, lists, ranges, numeric values, named months, and named weekdays. Computes upcoming UTC occurrences over a 400-year Gregorian cycle. |
| URL Codec | Preserves absolute HTTP(S) URL delimiters and offers separate component encode/decode operations. |
| Unix Timestamp | Seconds and milliseconds are explicit actions; output is labeled UTC. |
| Base64 / UUID | Supports Base64URL and UUID D/N/B/P/X formats. |
| Regex | Supports Ignore Case, Multiline, Singleline, and replacement mode with execution timeout; tooltips explain anchor and dot behavior. |
| Text Diff | Supports side-by-side output and configurable whitespace-ignore behavior. |
| cURL | Validates RFC HTTP method tokens, URL control characters, header names/values, rejects NUL in request bodies, and shell-quotes URL, headers, and body. |
| Password generator | Uses `RandomNumberGenerator.GetInt32`, includes every selected character class, and cryptographically shuffles output. |
| Password strength | Explicitly labels its score as heuristic; it is not entropy measurement or breach screening. |
| QR | Exposes four error-correction levels and a configurable quiet zone (0-16 modules). |
| Conventional Commit | Validates type syntax, scope, subject length, and breaking-change description. |

### Trading calculators

| Calculator | Implemented behavior and boundary |
|---|---|
| Position Size | Long/Short direction, stop-loss-side validation, per-side fees and slippage, risk-after-costs quantity. |
| Risk/Reward | Fee/slippage-adjusted net R:R and break-even win rate; invalid stop/target direction is rejected. |
| Futures PnL | Linear contract size, per-side slippage/fees, configurable funding rate and whole funding-period count. Uses average open/close notional for funding. |
| Liquidation | Linear isolated estimate with leverage/MMR validation. Explicitly excludes exchange tiers, fees, funding, and cross margin. |
| Average Entry | Per-leg fee-adjusted cost basis. |
| DCA | Fees, average cost, and optional current-market-value/unrealized-PnL scenario. |
| Break-even | Round-trip open/close fee model. |
| Trading Fee | Notional-based fee calculation; exchange presets are intentionally not hardcoded. |
| Funding | Signed funding cashflow with localized payer/receiver labels. |
| Leverage | Effective leverage and approximate adverse move to consume initial margin. |
| Stop Loss | Fee/slippage-adjusted stop price and risk breakdown. |
| Take Profit | Mutually exclusive target modes, net profit, optional net R:R and break-even comparison. |

### Risk and crypto calculators

- Risk of Ruin uses deterministic fixed-fractional Monte Carlo paths, configurable ruin threshold/horizon/simulation count/seed, and a Wilson confidence interval. Assumptions and excluded costs are stated.
- Max Drawdown rejects negative equity, permits zero equity, handles an all-zero series, and reports independent absolute/percentage peak-to-trough and recovery periods.
- Loss Recovery supports multiple independent scenarios.
- Expectancy includes R-multiple output.
- Kelly includes full/half/quarter and configurable capped allocation.
- Consecutive Loss reports the remaining-equity path.
- Market Cap distinguishes circulating market cap, total-supply valuation, and maximum-supply FDV.
- Market Cap Compare supports multiple target scenarios.
- ATH Drawdown includes recovery percentage.

### Shared UX, persistence, localization, and CI

- Last Used is deduplicated, filters unknown slugs, and is capped at 10 tools.
- Favorites and clear-history controls are persisted in browser storage.
- Workspace import/export validates tool slug, version, and size.
- Workspace envelopes are versioned; legacy raw workspace JSON is wrapped as the current envelope.
- Save status is shown for pending/saved/failed operations; local fallback is retained when cloud save fails.
- Clearing a draft persists an explicitly empty workspace so first-run samples do not reappear after reload.
- Tool host supports Ctrl/Command+Enter to run the primary action, fullscreen mode, and Escape to exit fullscreen.
- Regex, diff, calculator formulas, editor statistics/status, validation status, and JSON parse positions have English/Vietnamese resources.
- Raw exception messages are not displayed in tool notifications.
- Responsive smoke coverage checks 375px, 768px, and 1280px viewports.
- CI builds the solution, runs application unit tests, runs the dedicated client unit-test project, and includes DevTools browser smoke tests in the client visual workflow.

## Remaining items requiring product or domain decisions

These are deliberately not implemented without the missing product constraints or maintained reference data.

| Area | Why it remains open | Required decision/input |
|---|---|---|
| SQL Formatter | A correct dialect-aware parser/formatter differs between SQL Server, MariaDB/MySQL, PostgreSQL, etc. | Select supported SQL dialects and formatting rules. |
| HTML to Markdown | Regex conversion cannot fully preserve arbitrary malformed HTML, nested tables, and browser parsing behavior. | Approve an HTML5 parser dependency and define table/list conversion rules. |
| Cron | Current semantics are five-field UTC cron; Quartz has different fields and special tokens, and local timezone evaluation must handle DST gaps/overlaps. | Specify scheduler dialect and timezone/DST policy. |
| Password Strength | Common-password screening requires a maintained, licensed dataset and update process. | Approve a local dataset and refresh cadence. |
| JWT verification | Verification is a different workflow from decoding and requires algorithm/key material. | Define supported algorithms, key input UX, and safe key handling. |
| SHA-256 file hashing | Current tool hashes text. File hashing requires file-picker UX and a maximum file size. | Confirm file hashing is required and set size limits. |
| QR fixed version/print sizing | Current output exposes error correction and quiet zone; fixed version affects capacity and print requirements. | Define target print dimensions and maximum payload expectations. |
| Exchange-specific futures/liquidation | Current Futures PnL is linear; liquidation is isolated linear. Exchange tiers, inverse contracts, maintenance brackets, funding mark prices, and fee schedules differ. | Select exchange/product types and use maintained official exchange specifications. |
| Trading fee presets | Presets/discounts change by exchange, tier, region, and fee-token policy. | Select exchanges and data ownership/update cadence. |
| DCA schedule | Current DCA is based on entered purchase legs, not a calendar schedule. | Define cadence, timezone, and date-range semantics. |
| Max Drawdown underwater curve | Input is an ordered equity series without timestamps. | Add dated equity observations and define sampling interval. |
| Expectancy confidence interval | Current inputs are aggregate win rate/average win/average loss; no sample variance is available. | Accept trade-level observations or add sample size and variance inputs. |
| Token valuation simulator | This is a new tool beyond the current 50; unlock/dilution assumptions need a product model. | Confirm scope and define vesting/unlock input model. |

## Test coverage

- `tests/ThaiX.Client.UnitTests/ToolQualityUnitTests.cs`: 121 `[Fact]` tests and 3 `[Theory]` methods with 11 inline cases (132 test cases).
- `tests/ThaiX.Client.VisualTests/DevToolsSmokeTests.cs`: 9 `[Fact]` tests and 1 `[Theory]` with 3 viewport cases (12 browser test cases).

Coverage includes CSV/JSON conversion, heterogeneous root/nested JSON arrays, duplicate keys, large JSON formatting, SQL literals/comments/CR line endings, Markdown sanitization, URL/Base64URL/UUID, Cron aliases, regex flags, diff whitespace, cURL token/shell/control-character handling, password generation, calculator boundaries and funding, drawdown recovery, responsive widths, clear-draft persistence, keyboard shortcuts, and fullscreen exit.

## Verification status

The GitHub integration can update and inspect repository files, but this workflow does not provide a local .NET SDK.
No build or test result is claimed unless it is reported by a successful CI run. The branch-specific workflows
are configured for pull requests to the protected branches; they do not run on every feature-branch push.

Run locally before merge:

```bash
dotnet build ThaiX.slnx
dotnet test tests/ThaiX.Client.UnitTests/ThaiX.Client.UnitTests.csproj --no-restore
dotnet test tests/ThaiX.Client.VisualTests/ThaiX.Client.VisualTests.csproj --no-restore --filter FullyQualifiedName~DevToolsSmokeTests
dotnet test ThaiX.slnx --no-restore
```

Financial outputs remain estimates, not exchange guarantees or investment advice. The CSV parser, regex-based
HTML conversion, SQL formatter, cron dialect semantics, and financial models still require runtime review.

## Primary references

- Markdig `DisableHtml`: https://github.com/xoofx/markdig/blob/main/src/Markdig/MarkdownExtensions.cs
- HtmlSanitizer usage and default URI schemes: https://github.com/mganss/HtmlSanitizer
- Mermaid `securityLevel`: https://mermaid.js.org/config/usage#securitylevel
- QR Code Generator 3.2.1: https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/3.2.1
