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
        if (equity.Count < 2)
            throw new CalculatorInputException("Provide at least two equity points.");
        if (equity.Any(value => value < 0))
            throw new CalculatorInputException("Equity series values cannot be negative.");

        var runningPeak = equity[0];
        var runningPeakIndex = 0;

        decimal maxAbsoluteDrawdown = 0m;
        var absolutePeakIndex = 0;
        var absoluteTroughIndex = 0;
        var absolutePeakValue = 0m;
        var absoluteRecoveryIndex = 0;

        decimal maxDrawdownPct = 0m;
        var percentPeakIndex = 0;
        var percentTroughIndex = 0;
        var percentPeakValue = 0m;
        var percentRecoveryIndex = 0;

        for (var index = 0; index < equity.Count; index++)
        {
            var value = equity[index];
            if (value > runningPeak)
            {
                runningPeak = value;
                runningPeakIndex = index;
            }

            var drawdown = runningPeak - value;
            var drawdownPct = runningPeak == 0m ? 0m : drawdown / runningPeak * 100m;

            if (drawdown > maxAbsoluteDrawdown)
            {
                maxAbsoluteDrawdown = drawdown;
                absolutePeakIndex = runningPeakIndex;
                absoluteTroughIndex = index;
                absolutePeakValue = runningPeak;
                absoluteRecoveryIndex = 0;
            }
            else if (maxAbsoluteDrawdown > 0m
                && absoluteRecoveryIndex == 0
                && index > absoluteTroughIndex
                && value >= absolutePeakValue)
            {
                absoluteRecoveryIndex = index;
            }

            if (drawdownPct > maxDrawdownPct)
            {
                maxDrawdownPct = drawdownPct;
                percentPeakIndex = runningPeakIndex;
                percentTroughIndex = index;
                percentPeakValue = runningPeak;
                percentRecoveryIndex = 0;
            }
            else if (maxDrawdownPct > 0m
                && percentRecoveryIndex == 0
                && index > percentTroughIndex
                && value >= percentPeakValue)
            {
                percentRecoveryIndex = index;
            }
        }

        var absoluteRecoveryPeriods = maxAbsoluteDrawdown == 0m
            ? "0"
            : absoluteRecoveryIndex > 0
                ? F(absoluteRecoveryIndex - absoluteTroughIndex)
                : "N/A";
        var percentRecoveryPeriods = maxDrawdownPct == 0m
            ? "0"
            : percentRecoveryIndex > 0
                ? F(percentRecoveryIndex - percentTroughIndex)
                : "N/A";

        return new CalculatorResult(
            [
                Line("Calc.Result.MaxDrawdown", maxAbsoluteDrawdown),
                Line("Calc.Result.MaxDrawdownPct", maxDrawdownPct),
                Line("Calc.Result.AbsolutePeakToTroughPeriods", absoluteTroughIndex - absolutePeakIndex),
                new("Calc.Result.AbsoluteTroughToRecoveryPeriods", absoluteRecoveryPeriods),
                Line("Calc.Result.PeakToTroughPeriods", percentTroughIndex - percentPeakIndex),
                new("Calc.Result.TroughToRecoveryPeriods", percentRecoveryPeriods)
            ],
            "Absolute drawdown and percentage drawdown are tracked independently; recovery is measured against each drawdown's own peak",
            "Calc.Interp.MaxDrawdown",
            [F(maxDrawdownPct)]);
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

    public static CalculatorResult LossRecovery(decimal lossPct) =>
        LossRecovery([lossPct]);

    public static CalculatorResult LossRecovery(IReadOnlyList<decimal> lossScenarios)
    {
        if (lossScenarios.Count is < 1 or > 10)
            throw new CalculatorInputException("Provide between 1 and 10 loss scenarios.");

        var lines = new List<CalcLine>();
        foreach (var lossPct in lossScenarios)
        {
            RequirePositive(lossPct, "Loss %");
            if (lossPct >= 100)
                throw new CalculatorInputException("Loss % must be below 100.");

            var remaining = 1m - lossPct / 100m;
            var recovery = (1m / remaining - 1m) * 100m;
            lines.Add(Line("Calc.Result.LossPct", lossPct));
            lines.Add(Line("Calc.Result.RecoveryPct", recovery));
        }

        return new CalculatorResult(
            lines,
            "required recovery% = 1 / (1 − loss%) − 1; each loss percentage is an independent scenario",
            "Calc.Interp.LossRecovery");
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
        if (losses is < 0 or > 100)
            throw new CalculatorInputException("Loss count must be between 0 and 100.");
        if (riskPct >= 100) throw new CalculatorInputException("Risk % must be below 100.");

        var factor = 1m - riskPct / 100m;
        var remaining = equity;
        var lines = new List<CalcLine>();
        for (var loss = 1; loss <= losses; loss++)
        {
            remaining *= factor;
            lines.Add(Line("Calc.Result.LossCount", loss));
            lines.Add(Line("Calc.Result.RemainingEquity", remaining));
        }

        var drawdownPct = (equity - remaining) / equity * 100m;
        lines.Insert(0, Line("Calc.Result.MaxDrawdownPct", drawdownPct));
        lines.Insert(0, Line("Calc.Result.RemainingEquity", remaining));

        return new CalculatorResult(
            lines,
            "remaining equity after each loss = previous equity × (1 − risk%); assumes fixed-fractional risk on current equity",
            "Calc.Interp.ConsecutiveLoss",
            [F(remaining), F(drawdownPct)]);
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
