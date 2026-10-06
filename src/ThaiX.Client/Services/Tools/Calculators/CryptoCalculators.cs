using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class CryptoCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static CalculatorResult MarketCap(decimal price, decimal circulatingSupply)
    {
        RequirePositive(price, "Price");
        RequirePositive(circulatingSupply, "Circulating supply");
        var mcap = price * circulatingSupply;
        return new CalculatorResult(
            [Line("Calc.Result.MarketCap", mcap)],
            "marketCap = price × circulatingSupply",
            "Calc.Interp.MarketCap",
            [F(mcap)]);
    }

    public static CalculatorResult MarketCapCompare(decimal circulatingSupply, decimal targetMarketCap)
    {
        RequirePositive(circulatingSupply, "Circulating supply");
        RequirePositive(targetMarketCap, "Target market cap");
        var implied = targetMarketCap / circulatingSupply;
        return new CalculatorResult(
            [Line("Calc.Result.ImpliedPrice", implied)],
            "impliedPrice = targetMarketCap / circulatingSupply",
            "Calc.Interp.MarketCapCompare",
            [F(implied)]);
    }

    public static CalculatorResult AthDrawdown(decimal ath, decimal current)
    {
        RequirePositive(ath, "ATH");
        RequirePositive(current, "Current price");
        if (current > ath) throw new CalculatorInputException("Current price cannot exceed ATH for drawdown.");
        var ddPct = (ath - current) / ath * 100m;
        var recovery = current == 0 ? 0 : (ath / current - 1m) * 100m;
        return new CalculatorResult(
            [
                Line("Calc.Result.MaxDrawdownPct", ddPct),
                Line("Calc.Result.RecoveryPct", recovery)
            ],
            "drawdown% = (ATH − current)/ATH × 100; recovery% = ATH/current − 1",
            "Calc.Interp.AthDrawdown",
            [F(ddPct), F(recovery)]);
    }

    private static CalcLine Line(string key, decimal value) => new(key, F(value));
    private static string F(decimal v) => v.ToString("0.########", Inv);

    private static void RequirePositive(decimal v, string name)
    {
        if (v <= 0) throw new CalculatorInputException($"{name} must be greater than zero.");
    }
}
