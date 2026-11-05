# ThaiX Tools Quality Audit

## Scope

Reviewed the 50 entries in `ToolCatalog` on `feat/tools-last-used`:

| Category | Count |
|---|---:|
| Developer tools | 29 |
| Trading calculators | 12 |
| Risk calculators | 6 |
| Crypto calculators | 3 |
| **Total** | **50** |

This is a source-level audit of the Blazor client implementation. It is not a penetration test,
exchange-specific financial model validation, benchmark, or runtime verification.

## Changes in this branch

| Area | Change | Coverage |
|---|---|---|
| Calculator sample initialization | Seed calculator examples only for a new workspace; distinguish an empty new state from initialized calculator fields | New workspace and initialized workspace tests |
| CSV to JSON | Parse quoted fields across line breaks, escaped quotes, CRLF/LF, and UTF-8 BOM; reject unterminated quoted fields, empty headers, duplicate headers, and excess columns | Quoted comma, escaped quote, multiline, BOM, malformed quote, duplicate header tests |
| Password generator | Use `RandomNumberGenerator.GetInt32` to avoid modulo bias; ensure each selected character class appears at least once; cryptographically shuffle the result | Length and selected character class tests |
| Trading calculators | Bound risk percentages, constrain maintenance margin, reject ambiguous simultaneous take-profit modes, validate stop-loss direction for R:R mode, and prevent invalid short target prices | Boundary and mutually-exclusive-mode tests |
| Risk calculators | Reject non-positive equity-series points; document risk-of-ruin as a heuristic rather than a calibrated probability | Equity-series validation test |
| cURL generator | Validate the HTTP method and shell-quote the URL, headers, and body to prevent argument injection through single quotes | Invalid method and quoted URL tests |\n| JSON code generators | Sanitize generated type/property identifiers and quote non-identifier TypeScript property names | Special property-name tests for C# and TypeScript output |
| HTML to Markdown | Preserve common heading, paragraph, link, emphasis, inline-code, and list structure instead of stripping all tags | Structural conversion test |

## Findings and follow-up backlog

### Developer tools

| Tool / area | Finding | Recommended next step |
|---|---|---|
| SQL Formatter | Keyword formatting now scans quoted literals, bracket/backtick identifiers, and line/block comments before placing keyword breaks; it is still not a dialect-aware parser | Add dialect-specific formatting and nested-query indentation after selecting supported SQL dialects |
| HTML to Markdown | The converter now preserves common headings, links, emphasis, inline code, paragraphs, and lists; regex conversion is not a full HTML5 parser and nested structures can still be lossy | Adopt an already-approved HTML parser if one exists in the dependency set, or keep the tool labelled as a best-effort converter |
| Cron | The current output is a textual preview, not expression validation or a next-run calculation | Add a parser and next-occurrence preview with an explicit scheduler dialect |
| URL Codec | Absolute HTTP(S) URLs are now normalized with delimiters preserved and spaces escaped; non-URL input remains component-encoded | Add explicit query-component mode and safe component-wise decoding for reserved escape sequences |
| Password Strength | The result now explicitly states that its score is heuristic and is not entropy or breach screening | Add common-password screening only with a vetted local dataset |
| JSON to C# / TypeScript | Property and type names derived from arbitrary JSON keys can collide or produce language keywords/invalid identifiers | Sanitize identifiers, escape reserved words, detect collisions, and cover arrays/nulls/nested schemas |
| JWT Decoder | The decoder displays decoded claims and already states that signatures are not verified | Keep the unverified warning prominent; do not describe decoded claims as authenticated |
| JSON Formatter / Validator | Large documents and parse failures need better line/column context | Surface structured parser diagnostics and test large payload behavior |
| JSON Compare / Diff | Current text diff output is linear and not path-oriented | Add JSON-path summaries and side-by-side line context |
| YAML / XML | Parser diagnostics should retain line/column; XML formatting should define whitespace preservation semantics | Add malformed-input diagnostics and whitespace-preservation tests |
| Base64 / URL / UUID / SHA-256 | Tool modes and output formats should be explicit | Add Base64URL, UUID format selection, and hash input/file modes only where user workflows justify them |
| QR | QR output is SVG; error-correction, quiet-zone, and size controls are not exposed | Expose documented encoder settings and add SVG output tests |
| cURL | Shell quoting and method validation are now covered; method/header/body UX still needs broader cases | Add tests for multiline headers, invalid header lines, empty body, and special shell characters |
| Conventional Commit | Builder does not fully validate Conventional Commits 1.0.0 | Validate type, scope, subject length, breaking footer, and body formatting |
| Regex | Timeout is present; flags and replacement ergonomics are limited | Expose supported regex flags and add timeout/invalid-pattern tests |
| Unix Timestamp | Seconds/milliseconds are inferred from magnitude | Add an explicit unit selector and timezone display mode |
| Text Diff | Current result is inline | Add side-by-side view and whitespace-ignore option |

### Trading calculators

| Calculator | Follow-up |
|---|---|
| Position Size | Add explicit Long/Short direction, stop-loss-side validation, fees/slippage, and risk after costs |
| Risk/Reward | Include fee-adjusted net R:R and break-even win rate; validate level direction |
| Futures PnL | Add contract size, isolated/cross margin distinction, funding, slippage, and exchange-specific fee presets |
| Liquidation | Current formula is an isolated estimate. Add tiered maintenance margin and exchange-specific fee/funding assumptions before presenting it as exchange liquidation |
| Average Entry | Include per-leg fees and net cost basis |
| DCA | Add schedule planning and current market-value scenario |
| Break-even | Add contract mode and fee assumptions |
| Trading Fee | Add exchange/tier presets and fee-token discounts only with maintained source data |
| Funding | Negative funding rates are supported by the signed calculation; add clear payer/receiver labels and interval semantics |
| Leverage | Add adverse-price / liquidation context and distinguish notional leverage from account leverage |
| Stop Loss | Include direction, fees, and slippage in risk amount |
| Take Profit | Target percent and R:R are now mutually exclusive; add fee-adjusted net profit and break-even comparison |

### Risk calculators

| Calculator | Follow-up |
|---|---|
| Risk of Ruin | Current result is a simplified heuristic, not a statistically calibrated ruin probability. A defensible model needs ruin threshold, finite horizon, return distribution, and cost assumptions; use Monte Carlo only with reproducible seeds and documented assumptions |
| Max Drawdown | Add recovery duration, underwater curve, and peak-to-recovery tracking |
| Loss Recovery | Add scenario table for multiple loss sizes and recovery targets |
| Expectancy | Add R-multiple mode, sample size, and uncertainty interval |
| Kelly Criterion | Add configurable full/half/quarter/capped sizing and warn that input estimates are unstable |
| Consecutive Loss | Add equity curve and risk-per-trade comparison |

### Crypto calculators

| Calculator | Follow-up |
|---|---|
| Market Cap | Distinguish circulating, total, and maximum supply; show FDV separately |
| Market Cap Compare | Add multiple target market-cap scenarios and explicit circulating-vs-fully-diluted assumptions |
| ATH Drawdown | Add recovery ROI and optional historical drawdown context |
| New tool candidate | Token valuation simulator with unlock schedule, dilution, and supply scenarios |

### Shared UX, persistence, and security

- Localize every dynamic tool label, action, validation message, and result description through the existing
  resource keys and both shared resource files. Dynamic strings in `ToolHost.razor` remain a known gap.
- Standardize numeric validation, decimal precision, overflow handling, and error presentation across all
  calculator entry points.
- Add versioned workspace-state migration and distinguish first-run samples from user-owned drafts.
- Add explicit save state, draft size limits, and import/export before expanding cloud persistence behavior.
- Add Favorites, a clear-history action, keyboard shortcuts, fullscreen workspace, and mobile-specific
  layouts after validating the current responsive baseline.
- Review the Markdown preview JavaScript sanitization and all raw HTML/SVG rendering paths. This audit
  did not establish an exploitable XSS issue; sanitizer behavior needs a dedicated source review and tests.

## Test cases added

The `tests/ThaiX.Client.UnitTests/ToolQualityUnitTests.cs` suite covers:

1. CSV quoted commas and escaped quotes.
2. CSV multiline fields and BOM handling.
3. CSV unterminated quotes and duplicate-header rejection.
4. SQL keywords inside string literals, quoted identifiers, and line/block comments.
5. HTML headings, links, emphasis, inline code, and list conversion.
6. Absolute URL encoding preserves URL separators.
7. JSON-to-C# and JSON-to-TypeScript generators handle special property names.
8. cURL URL shell escaping and HTTP method validation.
9. Password length, selected character-class inclusion, and heuristic disclaimer.
10. Calculator sample seeding for new versus initialized workspaces.
11. Position-size risk percentage upper bound.
12. Liquidation maintenance-margin upper bound.
13. Mutually exclusive take-profit target modes.
14. Positive equity-series requirement for max drawdown.
15. Signed funding cashflow with a negative rate.

## Verification status

The changes were authored through the GitHub repository integration. A local .NET SDK/build
environment was not available in this workflow, so no `dotnet build` or `dotnet test` result is
claimed. Run the following from a checkout of this branch before merge:

```bash
dotnet build ThaiX.slnx
dotnet test tests/ThaiX.Client.UnitTests/ThaiX.Client.UnitTests.csproj --no-restore
dotnet test ThaiX.slnx --no-restore
```

The CSV parser, regex-based HTML conversion, SQL formatter, generated model code, and financial formulas should receive
additional runtime review before production release. Financial outputs are estimates and must not be
represented as exchange guarantees or investment advice.
