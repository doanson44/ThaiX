using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class RiskCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Estimates the frequency of crossing a ruin threshold using reproducible fixed-fractional Monte Carlo paths.
    /// Assumes independent trades with constant win probability and fixed win/loss multiples; excludes costs.
    /// </summary>
    public static CalculatorResult RiskOfRuin(
        decimal winRatePct,
        decimal riskReward,
        decimal riskPerTradePct,
        decimal ruinThresholdPct = 50m,
        int tradeHorizon = 100,
        int simulationCount = 2000,
        int randomSeed = 2026)
    {
        RequirePct(winRatePct, "Win rate %");
        RequirePositive(riskReward, "R:R");
        RequirePositive(riskPerTradePct, "Risk/trade %");
        if (riskPerTradePct > 100)
            throw new CalculatorInputException("Risk/trade % cannot exceed 100.");
        if (ruinThresholdPct <= 0 || ruinThresholdPct >= 100)
            throw new CalculatorInputException("Ruin threshold % must be greater than 0 and below 100.");
        if (tradeHorizon is < 1 or > 1000)
            throw new CalculatorInputException("Trade horizon must be between 1 and 1000.");
        if (simulationCount is < 100 or > 10000)
            throw new CalculatorInputException("Simulation count must be between 100 and 10000.");
        if ((long)tradeHorizon * simulationCount > 2_000_000)
            throw new CalculatorInputException("Trade horizon multiplied by simulation count cannot exceed 2000000.");

        var winProbability = (double)(winRatePct / 100m);
        var riskFraction = (double)(riskPerTradePct / 100m);
        var winLogReturn = Math.Log(1d + riskFraction * (double)riskReward);
        var lossLogReturn = riskFraction >= 1d ? double.NegativeInfinity : Math.Log(1d - riskFraction);
        var ruinLogThreshold = Math.Log((double)(ruinThresholdPct / 100m));
        var random = new Random(randomSeed);
        var ruinedPaths = 0;

        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            var logEquity = 0d;
            for (var trade = 0; trade < tradeHorizon; trade++)
            {
                logEquity += random.NextDouble() < winProbability ? winLogReturn : lossLogReturn;
                if (logEquity <= ruinLogThreshold)
                {
                    ruinedPaths++;
                    break;
                }
            }
        }

        var probability = (decimal)ruinedPaths / simulationCount;
        var probabilityPct = probability * 100m;
        var (lowerBound, upperBound) = WilsonInterval(ruinedPaths, simulationCount);
        var edge = winRatePct / 100m - (1m - winRatePct / 100m) / riskReward;

        return new CalculatorResult(
            [
                Line("Calc.Result.Edge", edge),
                Line("Calc.Result.RiskOfRuinPct", probabilityPct),
                Line("Calc.Result.RiskOfRuinConfidenceLow", lowerBound),
                Line("Calc.Result.RiskOfRuinConfidenceHigh", upperBound),
                Line("Calc.Result.RuinThresholdPct", ruinThresholdPct),
                Line("Calc.Result.TradeHorizon", tradeHorizon),
                Line("Calc.Result.SimulationCount", simulationCount),
                Line("Calc.Result.RandomSeed", randomSeed)
            ],
            "Monte Carlo: win equity × (1 + risk% × R:R); loss equity × (1 − risk%); ruin when equity <= threshold% of initial; independent fixed-probability trades; excludes fees, slippage, and changing odds.",
            "Calc.Interp.RiskOfRuin",
            [F(probabilityPct), F(lowerBound), F(upperBound)]);
    }

    private static (decimal Lower, decimal Upper) WilsonInterval(int successes, int trials)
    {
        const double z = 1.959963984540054;
        var n = (double)trials;
        var p = successes / n;
        var zSquared = z * z;
        var denominator = 1d + zSquared / n;
        var center = (p + zSquared / (2d * n)) / denominator;
        var margin = z * Math.Sqrt((p * (1d - p) / n) + (zSquared / (4d * n * n))) / denominator;
        return ((decimal)Math.Max(0d, center - margin) * 100m,
            (decimal)Math.Min(1d, center + margin) * 100m);
    }

    public static CalculatorResult MaxDrawdownFromSeries(IReadOnlyList<decimal> equity)
    {
        if (equity.Count < 2) throw new CalculatorInputException("Provide at least two equity points.");
        if (equity.Any(value => value < 0))
            throw new CalculatorInputException("Equity series values cannot be negative.");

        decimal peak = equity[0];
        decimal maxDd = 0;
        decimal maxDdPct = 0;
        var peakIndex = 0;
        var maxPeakIndex = 0;
        var maxTroughIndex = 0;
        var recoveryIndex = 0;

        for (var index = 0; index < equity.Count; index++)
        {
            var value = equity[index];
            if (value >= peak && index > maxTroughIndex && maxTroughIndex > maxPeakIndex && recoveryIndex == 0)
                recoveryIndex = index;

            if (value > peak)
            {
                peak = value;
                peakIndex = index;
            }

            var drawdown = peak - value;
            var drawdownPct = peak == 0 ? 0 : drawdown / peak * 100m;
            if (drawdown > maxDd)
                maxDd = drawdown;

            if (drawdownPct > maxDdPct)
            {
                maxDdPct = drawdownPct;
                maxPeakIndex = peakIndex;
                maxTroughIndex = index;
                recoveryIndex = 0;
            }
        }

        var troughToRecovery = maxDd == 0
            ? "0"
            : recoveryIndex > 0
                ? F(recoveryIndex - maxTroughIndex)
                : "N/A";

        return new CalculatorResult(
            [
                Line("Calc.Result.MaxDrawdown", maxDd),
                Line("Calc.Result.MaxDrawdownPct", maxDdPct),
                Line("Calc.Result.PeakToTroughPeriods", maxTroughIndex - maxPeakIndex),
                new("Calc.Result.TroughToRecoveryPeriods", troughToRecovery)
            ],
            "MDD = max(peak − equity); MDD% = MDD / peak × 100; recovery duration is measured in input periods",
            "Calc.Interp.MaxDrawdown",
            [F(maxDdPct)]);
    }

    public static CalculatorResult MaxDrawdownFromPeakTrough(decimal peak, decimal trough)
    {
        RequirePositive(peak, "Peak");
        if (trough < 0) throw new CalculatorInputException("Trough cannot be negative.");
        if (trough > peak) throw new CalculatorInputException("Trough cannot exceed peak.");
        var dd = peak - trough;
        var ddPct = dd / peak * 100m;
        return new CalculatorResult(
            [
                Line("Calc.Result.MaxDrawdown", dd),
                Line("Calc.Result.MaxDrawdownPct", ddPct)
            ],
            "MDD = peak − trough; MDD% = MDD / peak × 100",
            "Calc.Interp.MaxDrawdown",
            [F(ddPct)]);
    }

    public static CalculatorResult LossRecovery(decimal lossPct)
    {
        RequirePositive(lossPct, "Loss %");
        if (lossPct >= 100) throw new CalculatorInputException("Loss % must be below 100.");
        var remaining = 1m - lossPct / 100m;
        var recovery = (1m / remaining - 1m) * 100m;
        return new CalculatorResult(
            [Line("Calc.Result.RecoveryPct", recovery)],
            "recovery% = 1/(1 − loss%) − 1",
            "Calc.Interp.LossRecovery",
            [F(recovery)]);
    }

    public static CalculatorResult Expectancy(decimal winRatePct, decimal avgWin, decimal avgLoss)
    {
        RequirePct(winRatePct, "Win rate %");
        RequirePositive(avgWin, "Average win");
        RequirePositive(avgLoss, "Average loss");
        var p = winRatePct / 100m;
        var exp = p * avgWin - (1m - p) * avgLoss;
        var expectancyR = exp / avgLoss;
        return new CalculatorResult(
            [
                Line("Calc.Result.Expectancy", exp),
                Line("Calc.Result.ExpectancyR", expectancyR)
            ],
            "E = winRate×avgWin − (1−winRate)×avgLoss; E_R = E / avgLoss",
            "Calc.Interp.Expectancy",
            [F(exp)]);
    }

    public static CalculatorResult Kelly(decimal winRatePct, decimal payoffRatio, decimal maxKellyPct = 25m)
    {
        RequirePct(winRatePct, "Win rate %");
        RequirePositive(payoffRatio, "Payoff ratio");
        RequirePct(maxKellyPct, "Kelly cap %");
        var p = winRatePct / 100m;
        var q = 1m - p;
        var b = payoffRatio;
        var full = (b * p - q) / b;
        if (full < 0) full = 0;
        var fullPct = full * 100m;
        var cappedPct = Math.Min(fullPct, maxKellyPct);
        return new CalculatorResult(
            [
                Line("Calc.Result.KellyFull", fullPct),
                Line("Calc.Result.KellyHalf", fullPct / 2m),
                Line("Calc.Result.KellyQuarter", fullPct / 4m),
                Line("Calc.Result.KellyCapped", cappedPct)
            ],
            "f* = (b×p − q)/b; fractional Kelly = f* × fraction; capped Kelly = min(full Kelly, cap)",
            "Calc.Interp.Kelly",
            [F(fullPct)]);
    }

    public static CalculatorResult ConsecutiveLoss(decimal equity, decimal riskPct, int losses)
    {
        RequirePositive(equity, "Equity");
        RequirePositive(riskPct, "Risk %");
        if (losses < 0) throw new CalculatorInputException("Losses cannot be negative.");
        if (riskPct >= 100) throw new CalculatorInputException("Risk % must be below 100.");
        var factor = 1m - riskPct / 100m;
        var remaining = equity * (decimal)Math.Pow((double)factor, losses);
        var ddPct = equity == 0 ? 0 : (equity - remaining) / equity * 100m;
        return new CalculatorResult(
            [
                Line("Calc.Result.RemainingEquity", remaining),
                Line("Calc.Result.MaxDrawdownPct", ddPct)
            ],
            "remaining = equity × (1 − risk%)^N",
            "Calc.Interp.ConsecutiveLoss",
            [F(remaining), F(ddPct)]);
    }

    private static CalcLine Line(string key, decimal value) => new(key, F(value));
    private static string F(decimal v) => v.ToString("0.########", Inv);

    private static void RequirePositive(decimal v, string name)
    {
        if (v <= 0) throw new CalculatorInputException($"{name} must be greater than zero.");
    }

    private static void RequirePct(decimal v, string name)
    {
        if (v < 0 || v > 100) throw new CalculatorInputException($"{name} must be between 0 and 100.");
    }
}
