using ThaiX.Client.Services.Tools.Calculators;
using ThaiX.Client.Services.Tools.Transforms;

namespace ThaiX.Client.Services.Tools;

public static class ToolSampleCatalog
{
    public static bool ShouldSeedSample(ToolDefinition tool, ToolWorkspaceState state)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(state);

        return tool.Kind == ToolKind.Calculator
            ? MatchesCalculatorDefaults(tool.Slug, state)
            : IsTextToolEmpty(state);
    }

    public static ToolWorkspaceState Create(string slug) =>
        slug.ToLowerInvariant() switch
        {
            "json-formatter" => FromSingleInput(
                """
                {"name":"ThaiX","features":["tools","samples"],"active":true,"meta":{"version":1,"owner":"demo"}}
                """,
                output: JsonToolTransforms.PrettyPrint(
                    """
                    {"name":"ThaiX","features":["tools","samples"],"active":true,"meta":{"version":1,"owner":"demo"}}
                    """)),

            "json-compare" => new ToolWorkspaceState
            {
                Input =
                """
                {
                  "id": 101,
                  "name": "BTCUSDT",
                  "enabled": true,
                  "limits": {
                    "maxLeverage": 25,
                    "tickSize": 0.1
                  }
                }
                """,
                Input2 =
                """
                {
                  "id": 101,
                  "name": "BTCUSDT",
                  "enabled": false,
                  "limits": {
                    "maxLeverage": 20,
                    "tickSize": 0.1
                  }
                }
                """,
                Output = JsonToolTransforms.Compare(
                    """
                    {
                      "id": 101,
                      "name": "BTCUSDT",
                      "enabled": true,
                      "limits": {
                        "maxLeverage": 25,
                        "tickSize": 0.1
                      }
                    }
                    """,
                    """
                    {
                      "id": 101,
                      "name": "BTCUSDT",
                      "enabled": false,
                      "limits": {
                        "maxLeverage": 20,
                        "tickSize": 0.1
                      }
                    }
                    """)
            },

            "yaml-validator" => FromSingleInput(
                """
                environment: development
                service:
                  name: ThaiX.Tools
                  replicas: 2
                  enabled: true
                """),

            "yaml-json" => FromSingleInput(
                """
                product:
                  code: THX
                  markets:
                    - VN
                    - US
                  active: true
                """,
                output: ConvertToolTransforms.YamlToJson(
                    """
                    product:
                      code: THX
                      markets:
                        - VN
                        - US
                      active: true
                    """)),

            "xml-formatter" => FromSingleInput(
                """<order id="1001"><symbol>BTCUSDT</symbol><price>64500.5</price><side>BUY</side></order>""",
                output: ConvertToolTransforms.FormatXml(
                    """<order id="1001"><symbol>BTCUSDT</symbol><price>64500.5</price><side>BUY</side></order>""")),

            "csv-json" => FromSingleInput(
                """
                symbol,price,volume
                BTCUSDT,64500.5,1280
                ETHUSDT,3520.2,8540
                """,
                output: ConvertToolTransforms.CsvToJson(
                    """
                    symbol,price,volume
                    BTCUSDT,64500.5,1280
                    ETHUSDT,3520.2,8540
                    """)),

            "json-to-csharp" => FromSingleInput(
                """
                {
                  "id": 7,
                  "name": "ThaiX",
                  "isEnabled": true,
                  "markets": ["VN", "US"],
                  "settings": {
                    "refreshSeconds": 30,
                    "theme": "dark"
                  }
                }
                """,
                output: ConvertToolTransforms.JsonToCsharp(
                    """
                    {
                      "id": 7,
                      "name": "ThaiX",
                      "isEnabled": true,
                      "markets": ["VN", "US"],
                      "settings": {
                        "refreshSeconds": 30,
                        "theme": "dark"
                      }
                    }
                    """)),

            "json-to-typescript" => FromSingleInput(
                """
                {
                  "id": 7,
                  "name": "ThaiX",
                  "isEnabled": true,
                  "markets": ["VN", "US"],
                  "settings": {
                    "refreshSeconds": 30,
                    "theme": "dark"
                  }
                }
                """,
                output: ConvertToolTransforms.JsonToTypescript(
                    """
                    {
                      "id": 7,
                      "name": "ThaiX",
                      "isEnabled": true,
                      "markets": ["VN", "US"],
                      "settings": {
                        "refreshSeconds": 30,
                        "theme": "dark"
                      }
                    }
                    """)),

            "sql-formatter" => FromSingleInput(
                "select id,name,price from instruments where market = 'crypto' and is_active = 1 order by price desc",
                output: TextToolTransforms.FormatSql(
                    "select id,name,price from instruments where market = 'crypto' and is_active = 1 order by price desc")),

            "html-formatter" => FromSingleInput(
                """<section class="hero"><h1>ThaiX</h1><p>Tools sample</p><button>Run</button></section>""",
                output:
                """
                <section class="hero">
                  <h1>ThaiX</h1>
                  <p>Tools sample</p>
                  <button>Run</button>
                </section>
                """),

            "css-formatter" => FromSingleInput(
                ".hero{display:flex;gap:12px;padding:16px;background:#111827;color:#fff}.hero button{border:none;border-radius:999px}",
                output:
                """
                .hero {
                  display: flex;
                  gap: 12px;
                  padding: 16px;
                  background: #111827;
                  color: #fff;
                }

                .hero button {
                  border: none;
                  border-radius: 999px;
                }
                """),

            "js-formatter" => FromSingleInput(
                "const prices=[64500,64620,64210];const avg=prices.reduce((sum,x)=>sum+x,0)/prices.length;console.log(avg);",
                output:
                """
                const prices = [64500, 64620, 64210];
                const avg = prices.reduce((sum, x) => sum + x, 0) / prices.length;

                console.log(avg);
                """),

            "markdown-preview" => FromSingleInput(
                """
                # ThaiX Tool Sample

                This preview supports **Markdown**, tables, and Mermaid.

                ```csharp
                var pnl = exitPrice - entryPrice;
                ```

                ```mermaid
                flowchart LR
                    A[Input] --> B[Transform]
                    B --> C[Preview]
                ```
                """),

            "html-markdown" => FromSingleInput(
                """
                <h1>ThaiX Tool Sample</h1>
                <p>Convert HTML content into a markdown-friendly draft.</p>
                <ul><li>Input</li><li>Transform</li><li>Output</li></ul>
                """,
                output: TextToolTransforms.HtmlToMarkdownRough(
                    """
                    <h1>ThaiX Tool Sample</h1>
                    <p>Convert HTML content into a markdown-friendly draft.</p>
                    <ul><li>Input</li><li>Transform</li><li>Output</li></ul>
                    """)),

            "base64" => FromSingleInput(
                "ThaiX tools sample",
                output: EncodeToolTransforms.Base64Encode("ThaiX tools sample")),

            "url-codec" => FromSingleInput(
                "https://ThaiX.local/tools?q=btc usdt&tab=preview",
                output: EncodeToolTransforms.UrlEncode("https://ThaiX.local/tools?q=btc usdt&tab=preview")),

            "sha256" => FromSingleInput(
                "ThaiX tools sample",
                output: EncodeToolTransforms.Sha256Hex("ThaiX tools sample")),

            "jwt-decoder" => FromSingleInput(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRoYWlYIERlbW8iLCJpYXQiOjE1MTYyMzkwMjJ9.signature",
                output: EncodeToolTransforms.DecodeJwt(
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRoYWlYIERlbW8iLCJpYXQiOjE1MTYyMzkwMjJ9.signature")),

            "password-generator" => new ToolWorkspaceState
            {
                Number = 20,
                Flag = true,
                Output = GeneratorToolTransforms.GeneratePassword(20, true)
            },

            "password-strength" => FromSingleInput(
                "ThaiX-Demo#2026",
                output: GeneratorToolTransforms.PasswordStrength("ThaiX-Demo#2026")),

            "uuid" => new ToolWorkspaceState
            {
                Flag = true,
                Output = EncodeToolTransforms.NewUuid(v7: true)
            },

            "cron" => new ToolWorkspaceState
            {
                Option = "0 9 * * 1-5",
                Output = GeneratorToolTransforms.DescribeCron("0 9 * * 1-5")
            },

            "qr" => new ToolWorkspaceState
            {
                Input = "https://ThaiX.local/tools/sample",
                Output = "QR generated."
            },

            "curl" => new ToolWorkspaceState
            {
                Option = "POST",
                Input = "https://api.ThaiX.local/v1/orders",
                Input2 =
                """
                Authorization: Bearer demo-token
                Content-Type: application/json
                X-Trace-Id: sample-001
                """,
                Output = GeneratorToolTransforms.BuildCurl(
                    "POST",
                    "https://api.ThaiX.local/v1/orders",
                    """
                    Authorization: Bearer demo-token
                    Content-Type: application/json
                    X-Trace-Id: sample-001
                    """,
                    "")
            },

            "gitignore" => new ToolWorkspaceState
            {
                Option = "dotnet",
                Output = GeneratorToolTransforms.GitignorePreset("dotnet")
            },

            "conventional-commit" => new ToolWorkspaceState
            {
                Option = "feat/tools",
                Input = "prefill sample data for tool pages",
                Input2 = "Adds ready-to-run examples so each tool is useful on first open.",
                Output = GeneratorToolTransforms.ConventionalCommit(
                    "feat",
                    "tools",
                    "prefill sample data for tool pages",
                    false,
                    "Adds ready-to-run examples so each tool is useful on first open.")
            },

            "regex" => new ToolWorkspaceState
            {
                Input = "BTCUSDT broke 65000 and ETHUSDT held 3500.",
                Input2 = @"[A-Z]{3,10}USDT",
                Option = "SYMBOL",
                Output = TextToolTransforms.RegexTest(
                    @"[A-Z]{3,10}USDT",
                    "BTCUSDT broke 65000 and ETHUSDT held 3500.",
                    null)
            },

            "unix-timestamp" => FromSingleInput(
                "2026-08-01T09:30:00Z",
                output: EncodeToolTransforms.UnixConvert("2026-08-01T09:30:00Z", toUnix: true)),

            "diff" => new ToolWorkspaceState
            {
                Input =
                """
                line one
                line two
                line four
                """,
                Input2 =
                """
                line one
                line three
                line four
                """,
                Output = TextToolTransforms.Diff(
                    """
                    line one
                    line two
                    line four
                    """,
                    """
                    line one
                    line three
                    line four
                    """)
            },

            "position-size" => CalcState("position-size", ("account", "10000"), ("riskPct", "1"), ("entry", "50000"), ("stopLoss", "49000")),
            "risk-reward" => CalcState("risk-reward", ("side", "long"), ("entry", "50000"), ("stopLoss", "49000"), ("takeProfit", "53000")),
            "futures-pnl" => CalcState("futures-pnl", ("side", "long"), ("entry", "50000"), ("exit", "52500"), ("size", "0.2"), ("leverage", "10"), ("feeMode", "taker")),
            "liquidation" => CalcState("liquidation", ("side", "long"), ("entry", "50000"), ("leverage", "10"), ("mmrPct", "0.5")),
            "average-entry" => CalcState("average-entry", ("legs", "50000,0.1;48000,0.15;46000,0.2")),
            "dca" => CalcState("dca", ("legs", "50000,1000;45000,1000;40000,1500")),
            "break-even" => CalcState("break-even", ("side", "long"), ("entry", "50000"), ("feeMode", "taker")),
            "trading-fee" => CalcState("trading-fee", ("notional", "15000"), ("feeMode", "taker")),
            "funding" => CalcState("funding", ("side", "long"), ("notional", "20000"), ("ratePct", "0.01"), ("periods", "3")),
            "leverage" => CalcState("leverage", ("notional", "25000"), ("margin", "2500")),
            "stop-loss" => CalcState("stop-loss", ("side", "long"), ("account", "10000"), ("riskPct", "1"), ("size", "0.2"), ("entry", "50000")),
            "take-profit" => CalcState("take-profit", ("side", "long"), ("entry", "50000"), ("size", "0.2"), ("stopLoss", "49000"), ("targetRr", "2"), ("targetPct", "")),
            "risk-of-ruin" => CalcState("risk-of-ruin", ("winRatePct", "55"), ("riskReward", "1.5"), ("riskPerTradePct", "1")),
            "max-drawdown" => CalcState("max-drawdown", ("peak", "10000"), ("trough", "7800"), ("equitySeries", "10000,10200,9800,11000,9300,9500")),
            "loss-recovery" => CalcState("loss-recovery", ("lossPct", "20")),
            "expectancy" => CalcState("expectancy", ("winRatePct", "55"), ("avgWin", "150"), ("avgLoss", "100")),
            "kelly-criterion" => CalcState("kelly-criterion", ("winRatePct", "55"), ("payoffRatio", "1.5")),
            "consecutive-loss" => CalcState("consecutive-loss", ("equity", "10000"), ("riskPct", "2"), ("losses", "5")),
            "market-cap" => CalcState("market-cap", ("price", "2.35"), ("supply", "100000000")),
            "market-cap-compare" => CalcState("market-cap-compare", ("supply", "21000000"), ("targetMcap", "2000000000000")),
            "ath-drawdown" => CalcState("ath-drawdown", ("ath", "69000"), ("current", "43000")),

            _ => new ToolWorkspaceState()
        };

    private static ToolWorkspaceState FromSingleInput(string input, string? output = null) =>
        new()
        {
            Input = input,
            Output = output ?? string.Empty
        };

    private static ToolWorkspaceState CalcState(string slug, params (string Key, string Value)[] overrides)
    {
        var fields = CalculatorFieldCatalog.DefaultFields(slug);
        foreach (var (key, value) in overrides)
            fields[key] = value;

        return new ToolWorkspaceState
        {
            Fields = fields
        };
    }

    private static bool IsTextToolEmpty(ToolWorkspaceState state) =>
        string.IsNullOrWhiteSpace(state.Input)
        && string.IsNullOrWhiteSpace(state.Input2)
        && string.IsNullOrWhiteSpace(state.Output)
        && string.IsNullOrWhiteSpace(state.Option)
        && !state.Flag
        && state.Number == 16
        && (state.Fields is null || state.Fields.Count == 0 || state.Fields.Values.All(string.IsNullOrWhiteSpace));

    private static bool MatchesCalculatorDefaults(string slug, ToolWorkspaceState state)
    {
        state.Fields ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var defaults = CalculatorFieldCatalog.DefaultFields(slug);

        foreach (var field in CalculatorFieldCatalog.FieldsFor(slug))
        {
            var current = state.Fields.TryGetValue(field.Key, out var value) ? value ?? string.Empty : string.Empty;
            var baseline = defaults.TryGetValue(field.Key, out var sample) ? sample ?? string.Empty : string.Empty;
            if (!string.Equals(current, baseline, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return state.Fields
            .Where(x => !defaults.ContainsKey(x.Key))
            .All(x => string.IsNullOrWhiteSpace(x.Value));
    }
}
