using RK = ThaiX.Client.Constants.ResourceKeys;

namespace ThaiX.Client.Services.Tools.Calculators;

public enum CalcFieldKind
{
    Number = 0,
    Select = 1,
    Text = 2
}

public sealed record CalcFieldOption(string Value, string LabelKey);

public sealed record CalcFieldDef(
    string Key,
    string LabelKey,
    CalcFieldKind Kind,
    string? DefaultValue = null,
    string? PlaceholderKey = null,
    IReadOnlyList<CalcFieldOption>? Options = null,
    bool ShowWhenFeeCustom = false);

public static class CalculatorFieldCatalog
{
    private static readonly CalcFieldOption[] SideOptions =
    [
        new("long", RK.Calc.SideLong),
        new("short", RK.Calc.SideShort)
    ];

    private static readonly CalcFieldOption[] FeeModeOptions =
    [
        new("taker", RK.Calc.FeeTaker),
        new("maker", RK.Calc.FeeMaker),
        new("custom", RK.Calc.FeeCustom)
    ];

    private static readonly CalcFieldOption[] CurrencyOptions =
    [
        new("USDT", RK.Calc.CurrencyUsdt),
        new("USD", RK.Calc.CurrencyUsd),
        new("VND", RK.Calc.CurrencyVnd),
        new("BTC", RK.Calc.CurrencyBtc)
    ];

    public static IReadOnlyList<CalcFieldDef> FieldsFor(string slug) =>
        slug.ToLowerInvariant() switch
        {
            "position-size" =>
            [
                Currency(),
                Num("account", RK.Calc.Field.Account),
                Num("riskPct", RK.Calc.Field.RiskPct, "1"),
                Num("entry", RK.Calc.Field.Entry),
                Num("stopLoss", RK.Calc.Field.StopLoss)
            ],
            "risk-reward" =>
            [
                Currency(),
                Side(),
                Num("entry", RK.Calc.Field.Entry),
                Num("stopLoss", RK.Calc.Field.StopLoss),
                Num("takeProfit", RK.Calc.Field.TakeProfit)
            ],
            "futures-pnl" =>
            [
                Currency(),
                Side(),
                Num("entry", RK.Calc.Field.Entry),
                Num("exit", RK.Calc.Field.Exit),
                Num("size", RK.Calc.Field.Size),
                Num("leverage", RK.Calc.Field.Leverage, "10"),
                FeeMode(),
                Num("makerPct", RK.Calc.Field.MakerPct, "0.02", showWhenCustom: false),
                Num("takerPct", RK.Calc.Field.TakerPct, "0.04", showWhenCustom: false),
                Num("openFeePct", RK.Calc.Field.OpenFeePct, "0.04", showWhenCustom: true),
                Num("closeFeePct", RK.Calc.Field.CloseFeePct, "0.04", showWhenCustom: true)
            ],
            "liquidation" =>
            [
                Currency(),
                Side(),
                Num("entry", RK.Calc.Field.Entry),
                Num("leverage", RK.Calc.Field.Leverage, "10"),
                Num("mmrPct", RK.Calc.Field.MmrPct, "0.5")
            ],
            "average-entry" =>
            [
                Currency(),
                Text("legs", RK.Calc.Field.LegsPriceQty, RK.Calc.Placeholder.LegsPriceQty)
            ],
            "dca" =>
            [
                Currency(),
                Text("legs", RK.Calc.Field.LegsPriceAmount, RK.Calc.Placeholder.LegsPriceAmount)
            ],
            "break-even" =>
            [
                Currency(),
                Side(),
                Num("entry", RK.Calc.Field.Entry),
                FeeMode(),
                Num("makerPct", RK.Calc.Field.MakerPct, "0.02"),
                Num("takerPct", RK.Calc.Field.TakerPct, "0.04"),
                Num("openFeePct", RK.Calc.Field.OpenFeePct, "0.04", showWhenCustom: true),
                Num("closeFeePct", RK.Calc.Field.CloseFeePct, "0.04", showWhenCustom: true)
            ],
            "trading-fee" =>
            [
                Currency(),
                Num("notional", RK.Calc.Field.Notional),
                FeeMode(),
                Num("makerPct", RK.Calc.Field.MakerPct, "0.02"),
                Num("takerPct", RK.Calc.Field.TakerPct, "0.04"),
                Num("feePct", RK.Calc.Field.FeePct, "0.04", showWhenCustom: true)
            ],
            "funding" =>
            [
                Currency(),
                Side(),
                Num("notional", RK.Calc.Field.Notional),
                Num("ratePct", RK.Calc.Field.FundingRatePct, "0.01"),
                Num("periods", RK.Calc.Field.Periods, "1")
            ],
            "leverage" =>
            [
                Currency(),
                Num("notional", RK.Calc.Field.Notional),
                Num("margin", RK.Calc.Field.Margin)
            ],
            "stop-loss" =>
            [
                Currency(),
                Side(),
                Num("account", RK.Calc.Field.Account),
                Num("riskPct", RK.Calc.Field.RiskPct, "1"),
                Num("size", RK.Calc.Field.Size),
                Num("entry", RK.Calc.Field.Entry)
            ],
            "take-profit" =>
            [
                Currency(),
                Side(),
                Num("entry", RK.Calc.Field.Entry),
                Num("size", RK.Calc.Field.Size),
                Num("targetPct", RK.Calc.Field.TargetPct),
                Num("targetRr", RK.Calc.Field.TargetRr),
                Num("stopLoss", RK.Calc.Field.StopLoss)
            ],
            "risk-of-ruin" =>
            [
                Num("winRatePct", RK.Calc.Field.WinRatePct, "55"),
                Num("riskReward", RK.Calc.Field.RiskReward, "1.5"),
                Num("riskPerTradePct", RK.Calc.Field.RiskPerTradePct, "1")
            ],
            "max-drawdown" =>
            [
                Currency(),
                Num("peak", RK.Calc.Field.Peak),
                Num("trough", RK.Calc.Field.Trough),
                Text("equitySeries", RK.Calc.Field.EquitySeries, RK.Calc.Placeholder.EquitySeries)
            ],
            "loss-recovery" =>
            [
                Num("lossPct", RK.Calc.Field.LossPct, "20")
            ],
            "expectancy" =>
            [
                Currency(),
                Num("winRatePct", RK.Calc.Field.WinRatePct, "50"),
                Num("avgWin", RK.Calc.Field.AvgWin),
                Num("avgLoss", RK.Calc.Field.AvgLoss)
            ],
            "kelly-criterion" =>
            [
                Num("winRatePct", RK.Calc.Field.WinRatePct, "55"),
                Num("payoffRatio", RK.Calc.Field.PayoffRatio, "1.5")
            ],
            "consecutive-loss" =>
            [
                Currency(),
                Num("equity", RK.Calc.Field.Equity),
                Num("riskPct", RK.Calc.Field.RiskPct, "1"),
                Num("losses", RK.Calc.Field.Losses, "5")
            ],
            "market-cap" =>
            [
                Currency(),
                Num("price", RK.Calc.Field.Price),
                Num("supply", RK.Calc.Field.Supply)
            ],
            "market-cap-compare" =>
            [
                Currency(),
                Num("supply", RK.Calc.Field.Supply),
                Num("targetMcap", RK.Calc.Field.TargetMcap)
            ],
            "ath-drawdown" =>
            [
                Currency(),
                Num("ath", RK.Calc.Field.Ath),
                Num("current", RK.Calc.Field.Current)
            ],
            _ => []
        };

    public static Dictionary<string, string> DefaultFields(string slug)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in FieldsFor(slug))
        {
            if (f.DefaultValue is not null)
                dict[f.Key] = f.DefaultValue;
            else if (f.Kind == CalcFieldKind.Select && f.Options is { Count: > 0 })
                dict[f.Key] = f.Options[0].Value;
            else
                dict[f.Key] = string.Empty;
        }

        return dict;
    }

    public static void EnsureDefaults(string slug, Dictionary<string, string> fields)
    {
        foreach (var f in FieldsFor(slug))
        {
            if (fields.ContainsKey(f.Key)) continue;
            if (f.DefaultValue is not null)
                fields[f.Key] = f.DefaultValue;
            else if (f.Options is { Count: > 0 })
                fields[f.Key] = f.Options[0].Value;
            else
                fields[f.Key] = string.Empty;
        }
    }

    private static CalcFieldDef Currency() =>
        new("currency", RK.Calc.Field.Currency, CalcFieldKind.Select, "USDT", Options: CurrencyOptions);

    private static CalcFieldDef Side() =>
        new("side", RK.Calc.Field.Side, CalcFieldKind.Select, "long", Options: SideOptions);

    private static CalcFieldDef FeeMode() =>
        new("feeMode", RK.Calc.Field.FeeMode, CalcFieldKind.Select, "taker", Options: FeeModeOptions);

    private static CalcFieldDef Num(string key, string label, string? def = null, bool showWhenCustom = false) =>
        new(key, label, CalcFieldKind.Number, def, ShowWhenFeeCustom: showWhenCustom);

    private static CalcFieldDef Text(string key, string label, string placeholder) =>
        new(key, label, CalcFieldKind.Text, "", placeholder);
}
