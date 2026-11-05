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

This is a source-level audit of the Blazor client implementation. It is not a penetration test,
exchange-specific financial model validation, benchmark, or runtime verification.

## Implemented improvements

| Area | Change |
|---|---|
| Calculator sample initialization | Calculator samples seed only when there is no persisted workspace envelope. Empty calculator field bags no longer suppress first-run samples. |
| CSV to JSON | Parses quoted fields, escaped quotes, multiline records, CRLF/LF/CR, and UTF-8 BOM. Rejects empty/duplicate headers, malformed quote placement, unterminated quotes, and excess columns. |
| JSON to C# / TypeScript | Sanitizes generated identifiers, quotes invalid TypeScript property names, avoids property-name collisions after normalization, and assigns unique nested type names. |
| Password generator | Uses `RandomNumberGenerator.GetInt32` rather than modulo reduction, guarantees each selected character class, and cryptographically shuffles the output. |
| Password strength | Explicitly labels the score as heuristic; it is not entropy measurement or breach screening. |
| cURL generator | Validates method tokens, URL control characters, header names and values; shell-quotes URL, headers, and body. |
| SQL formatter | Scans strings, quoted identifiers, bracket/backtick identifiers, and line/block comments before formatting keywords. It remains a lightweight formatter, not a dialect parser. |
| HTML to Markdown | Preserves common headings, links, emphasis, inline code, paragraphs, and lists. It remains a best-effort regex converter, not a full HTML5 parser. |
| Markdown preview security | Disables raw HTML parsing in Markdig and sanitizes generated HTML with HtmlSanitizer. Mermaid rendering now uses `securityLevel: strict`; diagram source detection does not depend on sanitized class attributes. |\n| JWT decoder | Puts a localized UNVERIFIED warning before decoded header/payload and keeps the signature-present/missing state explicit. Decoding never claims signature verification. |
| Cron | Validates five numeric fields and supports wildcards, steps, lists, and ranges. Shows the next five occurrences in UTC, searching a 400-year Gregorian cycle so rare schedules such as leap day are handled. Day-of-week accepts 0 or 7 for Sunday. Named months/days and Quartz extensions are not supported. |
| URL Codec | Preserves delimiters for absolute HTTP(S) URLs while escaping spaces, and exposes separate component encode/decode actions. |
| Unix Timestamp | Seconds and milliseconds are explicit actions; input magnitude is no longer used to guess the unit. Date-only values are interpreted as UTC and output is labeled UTC. |
| Base64 | Adds unpadded Base64URL encode/decode actions alongside standard Base64. |
| UUID | Adds D, N, B, P, and X output formats for UUID v4/v7. |
| Regex | Adds Ignore Case and replacement input options, localized result labels, and retains the existing execution timeout. |
| QR | Exposes all four error-correction levels and a configurable quiet zone (0-16 modules); default quiet zone is four modules. |\n| Text Diff | Adds a side-by-side aligned text output mode using the existing DiffPlex dependency. |
| Conventional Commit | Validates type syntax, scope, subject, 72-character header limit, and breaking-change description. Custom lowercase types are allowed. |
| Trading calculators | Adds Long/Short direction and stop-loss-side validation to Position Size; models per-side Futures PnL slippage; reports gross Risk/Reward break-even win rate; validates fee percentages, maintenance margin, leverage minimum, mutually exclusive Take Profit modes, and whole-number period/loss counts. |
| Risk calculators | Rejects negative equity while treating zero equity as a 100% drawdown; reports peak-to-trough and recovery periods; adds Expectancy in R multiples and a configurable Kelly cap; labels Risk of Ruin as a heuristic estimate, not a calibrated probability. |
| Crypto calculators | Market Cap distinguishes circulating supply market cap, total-supply valuation, and maximum-supply FDV, with supply-order validation. |
| Localization | Localizes ToolHost action labels, input labels, and generator options in English and Vietnamese. |

## Remaining backlog

### Developer tools

| Tool / area | Remaining work |
|---|---|
| SQL Formatter | Add dialect-aware parsing and nested-query indentation if multiple SQL dialects are required. |
| HTML to Markdown | Replace regex conversion with an approved HTML5 parser if nested tables, ordered lists, and malformed markup must be preserved. |
| Cron | Add named month/day aliases, timezone selection, and Quartz dialect only if those scheduler formats are required. |
| URL Codec | Add explicit query-component mode and component-wise decoding; current full-URL mode normalizes absolute HTTP(S) URLs. |
| Password Strength | Add common-password screening only with a vetted, maintained local dataset; do not present the current heuristic as entropy. |
| JSON generators | Handle heterogeneous arrays and all duplicate JSON object-key cases; validate generated output by compiling representative fixtures. |
| JWT Decoder | Keep decoded claims clearly marked unverified. Signature verification requires an explicit algorithm/key input workflow and must not be implied by decoding. |
| JSON Formatter / Validator | Add structured line/column diagnostics and large-payload tests. |
| JSON Compare / Diff | JSON Compare now reports JSON paths, missing-vs-null values, and quoted paths for special property names. Text Diff has a side-by-side output mode. |
| YAML / XML | YAML parser source positions and significant XML text whitespace are now covered by tests; add more malformed-input and whitespace-preservation cases. |
| SHA-256 | Add file hashing only if a browser file-input workflow is required. |
| QR | Add fixed version/size controls only if print-layout requirements justify them. |
| cURL | Add broader multiline-header, control-character, and special-shell-character tests. |
| Regex | Ignore Case and replacement input are now available. Add multiline/singleline/dotall flags and richer replacement ergonomics. |
| Text Diff | Add side-by-side output and whitespace-ignore mode. |

### Trading calculators

| Calculator | Remaining work |
|---|---|
| Position Size | Add fee/slippage assumptions and report risk after costs. |
| Risk/Reward | Add fee-adjusted net R:R and break-even win rate. |
| Futures PnL | Add contract size, funding, and explicit exchange/contract models. Per-side slippage is now configurable. |
| Liquidation | Add tiered maintenance margin and exchange-specific fee/funding assumptions before presenting it as exchange liquidation. |
| Average Entry | Include per-leg fees and net cost basis. |
| DCA | Add schedule planning and current market-value scenarios. |
| Break-even | Add contract mode and additional fee assumptions. |
| Trading Fee | Add exchange/tier presets and fee-token discounts only with maintained source data. |
| Funding | Add explicit payer/receiver labels and interval semantics. |
| Leverage | Add adverse-price and liquidation context. |
| Stop Loss | Include fees and slippage in the risk amount. |
| Take Profit | Add fee-adjusted net profit and break-even comparison. |

### Risk calculators

| Calculator | Remaining work |
|---|---|
| Risk of Ruin | A statistically defensible model needs a ruin threshold, finite horizon, return distribution, and cost assumptions. Monte Carlo requires reproducible seeds and documented assumptions. |
| Max Drawdown | Recovery duration is now measured in input periods. Add an underwater curve and actual time-based recovery only when dated equity observations are available. |
| Loss Recovery | Add multi-scenario recovery tables. |
| Expectancy | R-multiple output is now included. Add sample size and uncertainty intervals only when trade-level variance is available. |
| Kelly Criterion | Add configurable caps and stronger warnings about unstable input estimates. |
| Consecutive Loss | Add equity curve and risk-per-trade comparison. |

### Crypto calculators

| Calculator | Remaining work |
|---|---|
| Market Cap | Circulating, total, and maximum supply valuation are now distinct. |
| Market Cap Compare | Add multiple target-market-cap scenarios and explicit circulating-vs-fully-diluted assumptions. |
| ATH Drawdown | Add recovery ROI and optional historical drawdown context. |
| New tool candidate | Token valuation simulator with unlock schedule, dilution, and supply scenarios. |

### Shared UX, persistence, and localization

- Add Favorites, clear-history controls, workspace import/export, save status, keyboard shortcuts, and fullscreen workspace.
- Validate mobile layouts at 375/768/1280px and add visual coverage for the new tool controls.
- Add versioned workspace-state migration before changing the persisted envelope schema.
- Standardize calculator precision, overflow handling, and localized validation errors.
- Dynamic ToolHost action/input labels are localized; transform exception messages and some remaining static labels still need localization.
- Review draft size limits and cloud/local save behavior before expanding persistence features.

## Test coverage

The `tests/ThaiX.Client.UnitTests/ToolQualityUnitTests.cs` suite contains 76 `[Fact]` tests and
three parameterized `[Theory]` methods with 11 inline cases (87 test cases total). Coverage includes:

- CSV quoting, multiline fields, BOM, malformed quotes, and duplicate headers.
- SQL literals/comments, HTML conversion, Markdown raw-HTML/unsafe-link payloads, and JWT unverified warnings.
- C#/TypeScript identifier sanitization and nested type collisions.
- URL normalization, Base64URL, Unix seconds/milliseconds, and UUID formats.
- cURL shell quoting, method/header validation, Cron parsing, Conventional Commit validation, QR settings, password generation, and last-used history normalization.
- Calculator sample initialization, Position Size direction, Futures slippage, Risk/Reward break-even, trading boundaries, Risk of Ruin wording, Max Drawdown recovery, Expectancy R, Kelly caps, Funding signs, Market Cap/FDV supply rules, YAML positions, and XML whitespace.

## Verification status

The repository was updated through the GitHub integration. The execution environment has no .NET SDK
(`dotnet --info` returns `command not found`), so no build or test result is claimed. No passing
GitHub Actions status was available for the current branch head during this work.

Run these commands from a checkout before merge:

```bash
dotnet build ThaiX.slnx
dotnet test tests/ThaiX.Client.UnitTests/ThaiX.Client.UnitTests.csproj --no-restore
dotnet test ThaiX.slnx --no-restore
```

The CSV parser, regex-based HTML conversion, SQL formatter, generated model code, cron dialect
semantics, and financial formulas still require runtime review. Financial outputs are estimates and
must not be represented as exchange guarantees or investment advice.

## Primary references

- Markdig `DisableHtml`: https://github.com/xoofx/markdig/blob/main/src/Markdig/MarkdownExtensions.cs
- HtmlSanitizer usage and default URI schemes: https://github.com/mganss/HtmlSanitizer
- Mermaid `securityLevel` behavior: https://mermaid.js.org/config/usage#securitylevel
- QR Code Generator 3.2.1 error-correction and SVG API: https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/3.2.1
