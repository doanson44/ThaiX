using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class CalculatorRunner
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static CalculatorResult Run(string slug, IReadOnlyDictionary<string, string> fields) =>
        slug.ToLowerInvariant() switch
        {
            "position-size" => TradingCalculators.PositionSize(
                D(fields, "account"), D(fields, "riskPct"), D(fields, "entry"), D(fields, "stopLoss"), Side(fields),
                D(fields, "feePct", 0m), D(fields, "slippagePct", 0m)),
            "risk-reward" => TradingCalculators.RiskReward(
                D(fields, "entry"), D(fields, "stopLoss"), D(fields, "takeProfit"), Side(fields),
                D(fields, "feePct", 0m), D(fields, "slippagePct", 0m)),
            "futures-pnl" => TradingCalculators.FuturesPnl(
                D(fields, "entry"), D(fields, "exit"), D(fields, "size"), D(fields, "leverage"), Side(fields),
                FeePct(fields, "openFeePct"), FeePct(fields, "closeFeePct"), D(fields, "slippagePct", 0m),
                D(fields, "contractSize", 1m), D(fields, "fundingRatePct", 0m), Int(fields, "fundingPeriods", 0)),
            "liquidation" => TradingCalculators.Liquidation(
                D(fields, "entry"), D(fields, "leverage"), Side(fields), D(fields, "mmrPct")),
            "average-entry" => TradingCalculators.AverageEntry(
                ParsePairs(fields, "legs"), D(fields, "feePct", 0m)),
            "dca" => TradingCalculators.Dca(
                ParsePairs(fields, "legs"), D(fields, "feePct", 0m), OptionalD(fields, "currentPrice")),
            "break-even" => TradingCalculators.BreakEven(
                D(fields, "entry"), Side(fields), FeePct(fields, "openFeePct"), FeePct(fields, "closeFeePct")),
            "trading-fee" => TradingCalculators.TradingFee(D(fields, "notional"), FeePct(fields, "feePct")),
            "funding" => TradingCalculators.Funding(
                D(fields, "notional"), D(fields, "ratePct"), Int(fields, "periods"), Side(fields)),
            "leverage" => TradingCalculators.Leverage(D(fields, "notional"), D(fields, "margin")),
            "stop-loss" => TradingCalculators.StopLoss(
                D(fields, "account"), D(fields, "riskPct"), D(fields, "size"), D(fields, "entry"), Side(fields),
                D(fields, "feePct", 0m), D(fields, "slippagePct", 0m)),
            "take-profit" => TradingCalculators.TakeProfit(
                D(fields, "entry"), Side(fields), D(fields, "size"),
                OptionalD(fields, "stopLoss"), OptionalD(fields, "targetRr"), OptionalD(fields, "targetPct"),
                D(fields, "feePct", 0m), D(fields, "slippagePct", 0m)),
            "risk-of-ruin" => RiskCalculators.RiskOfRuin(
                D(fields, "winRatePct"),
                D(fields, "riskReward"),
                D(fields, "riskPerTradePct"),
                D(fields, "ruinThresholdPct", 50m),
                Int(fields, "tradeHorizon"),
                Int(fields, "simulationCount"),
                Int(fields, "randomSeed")),
            "max-drawdown" => MaxDrawdown(fields),
            "loss-recovery" => RiskCalculators.LossRecovery(LossScenarios(fields)),
            "expectancy" => RiskCalculators.Expectancy(
                D(fields, "winRatePct"), D(fields, "avgWin"), D(fields, "avgLoss")),
            "kelly-criterion" => RiskCalculators.Kelly(
                D(fields, "winRatePct"), D(fields, "payoffRatio"), D(fields, "maxKellyPct", 25m)),
            "consecutive-loss" => RiskCalculators.ConsecutiveLoss(
                D(fields, "equity"), D(fields, "riskPct"), Int(fields, "losses")),
            "market-cap" => CryptoCalculators.MarketCap(
                D(fields, "price"),
                D(fields, "supply"),
                OptionalD(fields, "totalSupply"),
                OptionalD(fields, "maxSupply")),
            "market-cap-compare" => CryptoCalculators.MarketCapCompare(
                D(fields, "supply"), TargetMarketCaps(fields)),
            "ath-drawdown" => CryptoCalculators.AthDrawdown(D(fields, "ath"), D(fields, "current")),
            _ => throw new CalculatorInputException($"Unknown calculator '{slug}'.")
        };

    private static CalculatorResult MaxDrawdown(IReadOnlyDictionary<string, string> fields)
    {
        if (fields.TryGetValue("equitySeries", out var series) && !string.IsNullOrWhiteSpace(series))
        {
            var points = series.Split([',', ';', '\n', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => decimal.Parse(s.Trim(), Inv))
                .ToList();
            return RiskCalculators.MaxDrawdownFromSeries(points);
        }

        return RiskCalculators.MaxDrawdownFromPeakTrough(D(fields, "peak"), D(fields, "trough"));
    }

    private static List<(decimal, decimal)> ParsePairs(
        IReadOnlyDictionary<string, string> fields, string key)
    {
        // Format: price,qty;price,qty  OR price,amount;...
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
            throw new CalculatorInputException("Add at least one row (price, qty/amount).");

        var list = new List<(decimal, decimal)>();
        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var bits = part.Split(',', StringSplitOptions.TrimEntries);
            if (bits.Length != 2) throw new CalculatorInputException($"Invalid row '{part}'. Use price,value;");
            list.Add((decimal.Parse(bits[0], Inv), decimal.Parse(bits[1], Inv)));
        }

        return list;
    }

    private static IReadOnlyList<decimal> TargetMarketCaps(IReadOnlyDictionary<string, string> fields)
    {
        if (!fields.TryGetValue("targetMcaps", out var raw) || string.IsNullOrWhiteSpace(raw))
            return [D(fields, "targetMcap")];

        return ParseDecimalList(raw, "target market cap", 10);
    }

    private static IReadOnlyList<decimal> LossScenarios(IReadOnlyDictionary<string, string> fields)
    {
        if (!fields.TryGetValue("lossScenarios", out var raw) || string.IsNullOrWhiteSpace(raw))
            return [D(fields, "lossPct")];

        return ParseDecimalList(raw, "loss percentage", 10);
    }

    private static IReadOnlyList<decimal> ParseDecimalList(string raw, string label, int maxCount)
    {
        var values = raw.Split([',', ';', '\r', '\n', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length is < 1 || values.Length > maxCount)
            throw new CalculatorInputException($"Provide between 1 and {maxCount} {label} scenarios.");

        var parsed = new List<decimal>(values.Length);
        foreach (var value in values)
        {
            if (!decimal.TryParse(value, NumberStyles.Number, Inv, out var number)
                && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out number))
                throw new CalculatorInputException($"Invalid {label}: {value}.");
            parsed.Add(number);
        }

        return parsed;
    }

    private static TradeSide Side(IReadOnlyDictionary<string, string> fields) =>
        string.Equals(Get(fields, "side"), "short", StringComparison.OrdinalIgnoreCase)
            ? TradeSide.Short
            : TradeSide.Long;

    public static decimal ResolveFeePct(IReadOnlyDictionary<string, string> fields, string customKey = "feePct")
    {
        var mode = Get(fields, "feeMode").ToLowerInvariant();
        return mode switch
        {
            "maker" => D(fields, "makerPct", 0.02m),
            "custom" => D(fields, customKey),
            _ => D(fields, "takerPct", 0.04m) // taker default
        };
    }

    private static decimal FeePct(IReadOnlyDictionary<string, string> fields, string key)
    {
        var mode = Get(fields, "feeMode").ToLowerInvariant();
        if (mode == "custom")
            return D(fields, key);
        return ResolveFeePct(fields, key);
    }

    private static decimal D(IReadOnlyDictionary<string, string> fields, string key, decimal? fallback = null)
    {
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            if (fallback is decimal f) return f;
            throw new CalculatorInputException($"Missing value: {key}");
        }

        if (!decimal.TryParse(raw.Trim(), NumberStyles.Number, Inv, out var v)
            && !decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out v))
            throw new CalculatorInputException($"Invalid number for {key}.");
        return v;
    }

    private static int Int(IReadOnlyDictionary<string, string> fields, string key, int? fallback = null)
    {
        if ((!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw)) && fallback.HasValue)
            return fallback.Value;

        var value = D(fields, key);
        if (value != decimal.Truncate(value) || value < int.MinValue || value > int.MaxValue)
            throw new CalculatorInputException($"{key} must be a whole number.");

        return decimal.ToInt32(value);
    }

    private static decimal? OptionalD(IReadOnlyDictionary<string, string> fields, string key)
    {
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw)) return null;
        return D(fields, key);
    }

    private static string Get(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var v) ? v ?? "" : "";
}
