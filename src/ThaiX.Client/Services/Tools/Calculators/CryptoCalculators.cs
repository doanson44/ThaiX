using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class CryptoCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static CalculatorResult MarketCap(
        decimal price,
        decimal circulatingSupply,
        decimal? totalSupply = null,
        decimal? maxSupply = null)
    {
        RequirePositive(price, "Price");
        RequirePositive(circulatingSupply, "Circulating supply");

        if (totalSupply is decimal total)
        {
            RequirePositive(total, "Total supply");
            if (total < circulatingSupply)
                throw new CalculatorInputException("Total supply cannot be below circulating supply.");
        }

        if (maxSupply is decimal maximum)
        {
            RequirePositive(maximum, "Maximum supply");
            var minimumMaximum = totalSupply ?? circulatingSupply;
            if (maximum < minimumMaximum)
                throw new CalculatorInputException("Maximum supply cannot be below total or circulating supply.");
        }

        var lines = new List<CalcLine>
        {
            Line("Calc.Result.MarketCap", price * circulatingSupply)
        };

        if (totalSupply is decimal totalValue)
            lines.Add(Line("Calc.Result.TotalSupplyValue", price * totalValue));

        if (maxSupply is decimal maxValue)
            lines.Add(Line("Calc.Result.Fdv", price * maxValue));

        return new CalculatorResult(
            lines,
            "marketCap = price × circulatingSupply; total supply value = price × totalSupply; FDV = price × maxSupply",
            "Calc.Interp.MarketCap",
            [F(price * circulatingSupply)]);
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
