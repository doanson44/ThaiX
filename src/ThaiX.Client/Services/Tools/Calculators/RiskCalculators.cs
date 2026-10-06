using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class RiskCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Simplified fixed-fractional risk-of-ruin estimate:
    /// RoR ≈ ((1 − edge)/(1 + edge))^units where edge from win rate &amp; R:R, units = 1/risk%.
    /// </summary>
    public static CalculatorResult RiskOfRuin(decimal winRatePct, decimal riskReward, decimal riskPerTradePct)
    {
        RequirePct(winRatePct, "Win rate %");
        RequirePositive(riskReward, "R:R");
        RequirePositive(riskPerTradePct, "Risk/trade %");
        if (riskPerTradePct > 100) throw new CalculatorInputException("Risk/trade % cannot exceed 100.");

        var p = winRatePct / 100m;
        var q = 1m - p;
        var b = riskReward;
        // Edge ≈ p − q/b (normalized)
        var edge = p - q / b;
        decimal ror;
        if (edge <= 0)
            ror = 100m;
        else
        {
            var ratio = (1m - edge) / (1m + edge);
            if (ratio < 0) ratio = 0;
            var units = 100m / riskPerTradePct;
            ror = (decimal)(Math.Pow((double)ratio, (double)units) * 100d);
            if (ror > 100) ror = 100;
            if (ror < 0) ror = 0;
        }

        return new CalculatorResult(
            [
                Line("Calc.Result.Edge", edge),
                Line("Calc.Result.RiskOfRuinPct", ror)
            ],
            "edge ≈ winRate − (1−winRate)/R:R; RoR ≈ ((1−edge)/(1+edge))^(100/risk%) × 100 (simplified)",
            "Calc.Interp.RiskOfRuin",
            [F(ror)]);
    }

    public static CalculatorResult MaxDrawdownFromSeries(IReadOnlyList<decimal> equity)
    {
        if (equity.Count < 2) throw new CalculatorInputException("Provide at least two equity points.");
        decimal peak = equity[0];
        decimal maxDd = 0;
        decimal maxDdPct = 0;
        foreach (var e in equity)
        {
            if (e > peak) peak = e;
            var dd = peak - e;
            var ddPct = peak == 0 ? 0 : dd / peak * 100m;
            if (dd > maxDd) maxDd = dd;
            if (ddPct > maxDdPct) maxDdPct = ddPct;
        }

        return new CalculatorResult(
            [
                Line("Calc.Result.MaxDrawdown", maxDd),
                Line("Calc.Result.MaxDrawdownPct", maxDdPct)
            ],
            "MDD = max(peak − equity); MDD% = MDD / peak × 100",
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
        return new CalculatorResult(
            [Line("Calc.Result.Expectancy", exp)],
            "E = winRate×avgWin − (1−winRate)×avgLoss",
            "Calc.Interp.Expectancy",
            [F(exp)]);
    }

    public static CalculatorResult Kelly(decimal winRatePct, decimal payoffRatio)
    {
        RequirePct(winRatePct, "Win rate %");
        RequirePositive(payoffRatio, "Payoff ratio");
        var p = winRatePct / 100m;
        var q = 1m - p;
        var b = payoffRatio;
        var full = (b * p - q) / b;
        if (full < 0) full = 0;
        return new CalculatorResult(
            [
                Line("Calc.Result.KellyFull", full * 100m),
                Line("Calc.Result.KellyHalf", full * 50m),
                Line("Calc.Result.KellyQuarter", full * 25m)
            ],
            "f* = (b×p − q)/b; half = f*/2; quarter = f*/4 (shown as % of bankroll)",
            "Calc.Interp.Kelly",
            [F(full * 100m)]);
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
