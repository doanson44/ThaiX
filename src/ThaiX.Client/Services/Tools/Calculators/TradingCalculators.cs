using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class TradingCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static CalculatorResult PositionSize(decimal account, decimal riskPct, decimal entry, decimal stopLoss)
    {
        RequirePositive(account, "Account");
        RequirePositive(riskPct, "Risk %");
        RequirePositive(entry, "Entry");
        RequirePositive(stopLoss, "Stop loss");
        var riskDist = Math.Abs(entry - stopLoss);
        if (riskDist == 0) throw new CalculatorInputException("Entry and stop loss must differ.");

        var riskAmount = account * (riskPct / 100m);
        var qty = riskAmount / riskDist;
        var notional = qty * entry;

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskAmount", riskAmount),
                Line("Calc.Result.Quantity", qty),
                Line("Calc.Result.Notional", notional)
            ],
            "riskAmount = account × risk%; qty = riskAmount / |entry − SL|; notional = qty × entry",
            "Calc.Interp.PositionSize",
            [F(riskPct), F(qty), F(notional)]);
    }

    public static CalculatorResult RiskReward(decimal entry, decimal stopLoss, decimal takeProfit, TradeSide side)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(stopLoss, "Stop loss");
        RequirePositive(takeProfit, "Take profit");

        var risk = Math.Abs(entry - stopLoss);
        var reward = Math.Abs(takeProfit - entry);
        if (risk == 0) throw new CalculatorInputException("Entry and stop loss must differ.");

        var rr = reward / risk;
        var aligned = side == TradeSide.Long
            ? takeProfit > entry && stopLoss < entry
            : takeProfit < entry && stopLoss > entry;

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskDistance", risk),
                Line("Calc.Result.RewardDistance", reward),
                Line("Calc.Result.RiskReward", rr),
                new("Calc.Result.LevelsAligned", aligned ? "Calc.Yes" : "Calc.No")
            ],
            "R:R = |TP − entry| / |entry − SL|",
            "Calc.Interp.RiskReward",
            [F(rr)]);
    }

    public static CalculatorResult FuturesPnl(
        decimal entry, decimal exit, decimal size, decimal leverage, TradeSide side,
        decimal openFeePct, decimal closeFeePct)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(exit, "Exit");
        RequirePositive(size, "Size");
        RequirePositive(leverage, "Leverage");
        RequireNonNegative(openFeePct, "Open fee %");
        RequireNonNegative(closeFeePct, "Close fee %");

        var direction = side == TradeSide.Long ? 1m : -1m;
        var gross = (exit - entry) * size * direction;
        var notionalOpen = entry * size;
        var notionalClose = exit * size;
        var fees = notionalOpen * (openFeePct / 100m) + notionalClose * (closeFeePct / 100m);
        var net = gross - fees;
        var margin = notionalOpen / leverage;
        var roe = margin == 0 ? 0 : net / margin * 100m;

        return new CalculatorResult(
            [
                Line("Calc.Result.GrossPnl", gross),
                Line("Calc.Result.Fees", fees),
                Line("Calc.Result.NetPnl", net),
                Line("Calc.Result.Margin", margin),
                Line("Calc.Result.RoePct", roe)
            ],
            "gross = (exit − entry) × size × side; net = gross − fees; ROE% = net / (notional/leverage) × 100",
            "Calc.Interp.FuturesPnl",
            [F(net), F(roe)]);
    }

    public static CalculatorResult Liquidation(decimal entry, decimal leverage, TradeSide side, decimal mmrPct)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(leverage, "Leverage");
        RequireNonNegative(mmrPct, "MMR %");
        var mmr = mmrPct / 100m;
        // Isolated estimate (no cross / wallet extras).
        var liq = side == TradeSide.Long
            ? entry * (1m - (1m / leverage) + mmr)
            : entry * (1m + (1m / leverage) - mmr);
        if (liq < 0) liq = 0;

        return new CalculatorResult(
            [Line("Calc.Result.LiquidationPrice", liq)],
            "Long: entry×(1 − 1/lev + MMR); Short: entry×(1 + 1/lev − MMR) [isolated estimate]",
            "Calc.Interp.Liquidation",
            [F(liq)]);
    }

    public static CalculatorResult AverageEntry(IReadOnlyList<(decimal Price, decimal Qty)> legs)
    {
        if (legs.Count == 0) throw new CalculatorInputException("Add at least one price/qty row.");
        decimal cost = 0, qty = 0;
        foreach (var (price, q) in legs)
        {
            RequirePositive(price, "Price");
            RequirePositive(q, "Quantity");
            cost += price * q;
            qty += q;
        }

        var avg = cost / qty;
        return new CalculatorResult(
            [
                Line("Calc.Result.AverageEntry", avg),
                Line("Calc.Result.Quantity", qty),
                Line("Calc.Result.Cost", cost)
            ],
            "avg = Σ(price×qty) / Σ(qty)",
            "Calc.Interp.AverageEntry",
            [F(avg), F(qty)]);
    }

    public static CalculatorResult Dca(IReadOnlyList<(decimal Price, decimal Amount)> purchases)
    {
        if (purchases.Count == 0) throw new CalculatorInputException("Add at least one purchase row.");
        decimal invested = 0, qty = 0;
        foreach (var (price, amount) in purchases)
        {
            RequirePositive(price, "Price");
            RequirePositive(amount, "Amount");
            invested += amount;
            qty += amount / price;
        }

        var avg = invested / qty;
        return new CalculatorResult(
            [
                Line("Calc.Result.AverageEntry", avg),
                Line("Calc.Result.Quantity", qty),
                Line("Calc.Result.Invested", invested)
            ],
            "qty = Σ(amount/price); avg = invested / qty",
            "Calc.Interp.Dca",
            [F(avg), F(invested)]);
    }

    public static CalculatorResult BreakEven(decimal entry, TradeSide side, decimal openFeePct, decimal closeFeePct)
    {
        RequirePositive(entry, "Entry");
        RequireNonNegative(openFeePct, "Open fee %");
        RequireNonNegative(closeFeePct, "Close fee %");
        var fOpen = openFeePct / 100m;
        var fClose = closeFeePct / 100m;
        // Round-trip BE so net ≈ 0 after fees on notional.
        var be = side == TradeSide.Long
            ? entry * (1m + fOpen) / (1m - fClose)
            : entry * (1m - fOpen) / (1m + fClose);

        return new CalculatorResult(
            [Line("Calc.Result.BreakEvenPrice", be)],
            "Long BE ≈ entry×(1+fOpen)/(1−fClose); Short BE ≈ entry×(1−fOpen)/(1+fClose)",
            "Calc.Interp.BreakEven",
            [F(be)]);
    }

    public static CalculatorResult TradingFee(decimal notional, decimal feePct)
    {
        RequirePositive(notional, "Notional");
        RequireNonNegative(feePct, "Fee %");
        var fee = notional * (feePct / 100m);
        return new CalculatorResult(
            [Line("Calc.Result.Fees", fee)],
            "fee = notional × fee%",
            "Calc.Interp.TradingFee",
            [F(fee)]);
    }

    public static CalculatorResult Funding(decimal notional, decimal ratePct, int periods, TradeSide side)
    {
        RequirePositive(notional, "Notional");
        RequireNonNegative(periods, "Periods");
        // Positive rate: longs pay shorts. Sign: Long pays when rate > 0.
        var raw = notional * (ratePct / 100m) * periods;
        var paidByLong = raw;
        var userCashflow = side == TradeSide.Long ? -paidByLong : paidByLong;

        return new CalculatorResult(
            [
                Line("Calc.Result.FundingCashflow", userCashflow),
                Line("Calc.Result.FundingAbs", Math.Abs(userCashflow))
            ],
            "cashflow = notional × rate% × periods × (short:+1 / long:−1) when rate>0 means longs pay",
            "Calc.Interp.Funding",
            [F(userCashflow)]);
    }

    public static CalculatorResult Leverage(decimal notional, decimal margin)
    {
        RequirePositive(notional, "Notional");
        RequirePositive(margin, "Margin");
        var lev = notional / margin;
        return new CalculatorResult(
            [
                Line("Calc.Result.EffectiveLeverage", lev),
                Line("Calc.Result.RequiredMargin", notional / lev) // same as margin
            ],
            "leverage = notional / margin; margin = notional / leverage",
            "Calc.Interp.Leverage",
            [F(lev)]);
    }

    public static CalculatorResult StopLoss(
        decimal account, decimal riskPct, decimal positionQty, decimal entry, TradeSide side)
    {
        RequirePositive(account, "Account");
        RequirePositive(riskPct, "Risk %");
        RequirePositive(positionQty, "Position size");
        RequirePositive(entry, "Entry");
        var riskAmount = account * (riskPct / 100m);
        var dist = riskAmount / positionQty;
        var sl = side == TradeSide.Long ? entry - dist : entry + dist;
        if (sl <= 0) throw new CalculatorInputException("Computed stop loss is not positive — reduce risk or size.");

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskAmount", riskAmount),
                Line("Calc.Result.StopLossPrice", sl)
            ],
            "dist = (account×risk%) / qty; Long SL = entry − dist; Short SL = entry + dist",
            "Calc.Interp.StopLoss",
            [F(sl)]);
    }

    /// <summary>
    /// TP from target % of entry, or from R:R when <paramref name="stopLoss"/> is provided.
    /// </summary>
    public static CalculatorResult TakeProfit(
        decimal entry,
        TradeSide side,
        decimal positionQty,
        decimal? stopLoss,
        decimal? targetRr,
        decimal? targetPct)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(positionQty, "Position size");

        if (targetPct is decimal pct)
        {
            RequirePositive(pct, "Target %");
            var move = entry * (pct / 100m);
            var tpPct = side == TradeSide.Long ? entry + move : entry - move;
            var expectedPct = move * positionQty;
            return new CalculatorResult(
                [
                    Line("Calc.Result.TakeProfitPrice", tpPct),
                    Line("Calc.Result.ExpectedProfit", expectedPct)
                ],
                "Long TP = entry×(1+%); Short TP = entry×(1−%); profit = |TP−entry|×qty",
                "Calc.Interp.TakeProfit",
                [F(tpPct), F(expectedPct)]);
        }

        if (targetRr is decimal rr && stopLoss is decimal sl)
        {
            RequirePositive(rr, "Target R:R");
            RequirePositive(sl, "Stop loss");
            var risk = Math.Abs(entry - sl);
            if (risk == 0) throw new CalculatorInputException("Entry and stop loss must differ.");
            var reward = risk * rr;
            var tpRr = side == TradeSide.Long ? entry + reward : entry - reward;
            var expectedRr = reward * positionQty;
            return new CalculatorResult(
                [
                    Line("Calc.Result.TakeProfitPrice", tpRr),
                    Line("Calc.Result.ExpectedProfit", expectedRr),
                    Line("Calc.Result.RiskReward", rr)
                ],
                "reward = |entry−SL| × R:R; Long TP = entry+reward; Short TP = entry−reward",
                "Calc.Interp.TakeProfit",
                [F(tpRr), F(expectedRr)]);
        }

        throw new CalculatorInputException("Provide target % , or target R:R together with stop loss.");
    }

    private static CalcLine Line(string key, decimal value) => new(key, F(value));

    private static string F(decimal v) => v.ToString("0.########", Inv);

    private static void RequirePositive(decimal v, string name)
    {
        if (v <= 0) throw new CalculatorInputException($"{name} must be greater than zero.");
    }

    private static void RequireNonNegative(decimal v, string name)
    {
        if (v < 0) throw new CalculatorInputException($"{name} cannot be negative.");
    }

    private static void RequireNonNegative(int v, string name)
    {
        if (v < 0) throw new CalculatorInputException($"{name} cannot be negative.");
    }
}
