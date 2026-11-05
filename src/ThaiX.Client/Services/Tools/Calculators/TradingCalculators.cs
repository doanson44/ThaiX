using System.Globalization;

namespace ThaiX.Client.Services.Tools.Calculators;

public static class TradingCalculators
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static CalculatorResult PositionSize(
        decimal account,
        decimal riskPct,
        decimal entry,
        decimal stopLoss,
        TradeSide side = TradeSide.Long,
        decimal feePct = 0m,
        decimal slippagePct = 0m)
    {
        RequirePositive(account, "Account");
        RequirePositive(riskPct, "Risk %");
        if (riskPct > 100) throw new CalculatorInputException("Risk % cannot exceed 100.");
        RequirePositive(entry, "Entry");
        RequirePositive(stopLoss, "Stop loss");
        RequireFeePct(feePct, "Fee per side %");
        RequireFeePct(slippagePct, "Slippage per side %");

        var stopIsAligned = side == TradeSide.Long ? stopLoss < entry : stopLoss > entry;
        if (!stopIsAligned)
            throw new CalculatorInputException("Stop loss must be below entry for long or above entry for short.");

        var slippage = slippagePct / 100m;
        var fee = feePct / 100m;
        var effectiveEntry = side == TradeSide.Long
            ? entry * (1m + slippage)
            : entry * (1m - slippage);
        var effectiveStop = side == TradeSide.Long
            ? stopLoss * (1m - slippage)
            : stopLoss * (1m + slippage);
        var priceRiskPerUnit = Math.Abs(effectiveEntry - effectiveStop);
        var feesPerUnit = (effectiveEntry + effectiveStop) * fee;
        var slippageCostPerUnit = Math.Abs(effectiveEntry - entry) + Math.Abs(effectiveStop - stopLoss);
        var totalRiskPerUnit = priceRiskPerUnit + feesPerUnit;
        if (totalRiskPerUnit <= 0)
            throw new CalculatorInputException("Total risk per unit must be greater than zero.");

        var riskAmount = account * (riskPct / 100m);
        var qty = riskAmount / totalRiskPerUnit;
        var notional = qty * effectiveEntry;

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskAmount", riskAmount),
                Line("Calc.Result.PriceRiskPerUnit", priceRiskPerUnit),
                Line("Calc.Result.FeesPerUnit", feesPerUnit),
                Line("Calc.Result.SlippageCostPerUnit", slippageCostPerUnit),
                Line("Calc.Result.TotalRiskPerUnit", totalRiskPerUnit),
                Line("Calc.Result.Quantity", qty),
                Line("Calc.Result.Notional", notional)
            ],
            "riskPerUnit = adverse entry/stop distance + entry/stop fees; qty = account risk amount / riskPerUnit; fees and slippage are charged per side",
            "Calc.Interp.PositionSize",
            [F(riskPct), F(qty), F(notional)]);
    }

    public static CalculatorResult RiskReward(
        decimal entry,
        decimal stopLoss,
        decimal takeProfit,
        TradeSide side,
        decimal feePct = 0m,
        decimal slippagePct = 0m)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(stopLoss, "Stop loss");
        RequirePositive(takeProfit, "Take profit");
        RequireFeePct(feePct, "Fee per side %");
        RequireFeePct(slippagePct, "Slippage per side %");

        var slippage = slippagePct / 100m;
        var fee = feePct / 100m;
        var effectiveEntry = side == TradeSide.Long
            ? entry * (1m + slippage)
            : entry * (1m - slippage);
        var effectiveStop = side == TradeSide.Long
            ? stopLoss * (1m - slippage)
            : stopLoss * (1m + slippage);
        var effectiveTarget = side == TradeSide.Long
            ? takeProfit * (1m - slippage)
            : takeProfit * (1m + slippage);

        var grossRisk = Math.Abs(effectiveEntry - effectiveStop);
        var grossReward = Math.Abs(effectiveTarget - effectiveEntry);
        if (grossRisk == 0) throw new CalculatorInputException("Entry and stop loss must differ.");

        var riskFees = (effectiveEntry + effectiveStop) * fee;
        var rewardFees = (effectiveEntry + effectiveTarget) * fee;
        var risk = grossRisk + riskFees;
        var reward = grossReward - rewardFees;
        if (reward <= 0)
            throw new CalculatorInputException("Take-profit reward must exceed round-trip fees and slippage.");

        var rr = reward / risk;
        var breakEvenWinRatePct = risk / (risk + reward) * 100m;
        var aligned = side == TradeSide.Long
            ? takeProfit > entry && stopLoss < entry
            : takeProfit < entry && stopLoss > entry;

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskDistance", risk),
                Line("Calc.Result.RewardDistance", reward),
                Line("Calc.Result.RiskReward", rr),
                Line("Calc.Result.BreakEvenWinRatePct", breakEvenWinRatePct),
                new("Calc.Result.LevelsAligned", aligned ? "Calc.Yes" : "Calc.No"),
                Line("Calc.Result.Fees", riskFees),
                Line("Calc.Result.Fees", rewardFees)
            ],
            "net risk = adverse stop distance + entry/stop fees; net reward = adverse target distance - entry/target fees; net R:R = net reward / net risk",
            "Calc.Interp.RiskReward",
            [F(rr)]);
    }

    public static CalculatorResult FuturesPnl(
        decimal entry, decimal exit, decimal size, decimal leverage, TradeSide side,
        decimal openFeePct, decimal closeFeePct, decimal slippagePct = 0m, decimal contractSize = 1m)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(exit, "Exit");
        RequirePositive(size, "Contracts");
        RequirePositive(contractSize, "Contract size");
        RequirePositive(leverage, "Leverage");
        RequireFeePct(openFeePct, "Open fee %");
        RequireFeePct(closeFeePct, "Close fee %");

        RequireFeePct(slippagePct, "Slippage %");

        var quantity = size * contractSize;
        var direction = side == TradeSide.Long ? 1m : -1m;
        var slippage = slippagePct / 100m;
        var effectiveEntry = side == TradeSide.Long
            ? entry * (1m + slippage)
            : entry * (1m - slippage);
        var effectiveExit = side == TradeSide.Long
            ? exit * (1m - slippage)
            : exit * (1m + slippage);
        var grossBeforeSlippage = (exit - entry) * quantity * direction;
        var gross = (effectiveExit - effectiveEntry) * quantity * direction;
        var slippageCost = grossBeforeSlippage - gross;
        var notionalOpen = effectiveEntry * quantity;
        var notionalClose = effectiveExit * quantity;
        var fees = notionalOpen * (openFeePct / 100m) + notionalClose * (closeFeePct / 100m);
        var net = gross - fees;
        var margin = notionalOpen / leverage;
        var roe = margin == 0 ? 0 : net / margin * 100m;

        return new CalculatorResult(
            [
                Line("Calc.Result.GrossPnl", gross),
                Line("Calc.Result.ContractSize", contractSize),
                Line("Calc.Result.BaseQuantity", quantity),
                Line("Calc.Result.SlippageCost", slippageCost),
                Line("Calc.Result.Fees", fees),
                Line("Calc.Result.NetPnl", net),
                Line("Calc.Result.Margin", margin),
                Line("Calc.Result.RoePct", roe)
            ],
            "Linear contract model: base quantity = contracts × contract size; gross = (adjustedExit - adjustedEntry) × base quantity × direction; slippage is applied on both sides; net = gross - fees; ROE% = net / margin × 100",
            "Calc.Interp.FuturesPnl",
            [F(net), F(roe)]);
    }

    public static CalculatorResult Liquidation(decimal entry, decimal leverage, TradeSide side, decimal mmrPct)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(leverage, "Leverage");
        if (leverage < 1) throw new CalculatorInputException("Leverage must be at least 1x.");
        RequireNonNegative(mmrPct, "MMR %");
        if (mmrPct >= 100) throw new CalculatorInputException("MMR % must be below 100.");
        var mmr = mmrPct / 100m;
        // Linear isolated model: equity at liquidation equals maintenance margin.
        var liq = side == TradeSide.Long
            ? entry * (1m - (1m / leverage)) / (1m - mmr)
            : entry * (1m + (1m / leverage)) / (1m + mmr);
        if (liq < 0) liq = 0;

        return new CalculatorResult(
            [Line("Calc.Result.LiquidationPrice", liq)],
            "Linear isolated estimate: Long = entry×(1 − 1/lev)/(1 − MMR); Short = entry×(1 + 1/lev)/(1 + MMR); excludes tiered MMR, fees, funding, and cross margin.",
            "Calc.Interp.Liquidation",
            [F(liq)]);
    }

    public static CalculatorResult AverageEntry(
        IReadOnlyList<(decimal Price, decimal Qty)> legs,
        decimal feePct = 0m)
    {
        if (legs.Count == 0) throw new CalculatorInputException("Add at least one price/qty row.");
        RequireFeePct(feePct, "Fee per leg %");

        decimal grossCost = 0m;
        decimal quantity = 0m;
        foreach (var (price, qty) in legs)
        {
            RequirePositive(price, "Price");
            RequirePositive(qty, "Quantity");
            grossCost += price * qty;
            quantity += qty;
        }

        var fees = grossCost * (feePct / 100m);
        var totalCost = grossCost + fees;
        var averageEntry = totalCost / quantity;
        return new CalculatorResult(
            [
                Line("Calc.Result.AverageEntry", averageEntry),
                Line("Calc.Result.Quantity", quantity),
                Line("Calc.Result.Cost", totalCost),
                Line("Calc.Result.Fees", fees)
            ],
            "average entry = (sum of price × quantity + per-leg fees) / total quantity",
            "Calc.Interp.AverageEntry",
            [F(averageEntry), F(quantity)]);
    }

    public static CalculatorResult Dca(
        IReadOnlyList<(decimal Price, decimal Amount)> purchases,
        decimal feePct = 0m,
        decimal? currentPrice = null)
    {
        if (purchases.Count == 0) throw new CalculatorInputException("Add at least one purchase row.");
        RequireFeePct(feePct, "Fee per purchase %");
        if (currentPrice is decimal marketPrice)
            RequirePositive(marketPrice, "Current market price");

        decimal invested = 0m;
        decimal quantity = 0m;
        foreach (var (price, amount) in purchases)
        {
            RequirePositive(price, "Price");
            RequirePositive(amount, "Amount");
            invested += amount;
            quantity += amount / price;
        }

        var fees = invested * (feePct / 100m);
        var totalOutlay = invested + fees;
        var averageEntry = totalOutlay / quantity;
        var lines = new List<CalcLine>
        {
            Line("Calc.Result.AverageEntry", averageEntry),
            Line("Calc.Result.Quantity", quantity),
            Line("Calc.Result.Invested", invested),
            Line("Calc.Result.Fees", fees),
            Line("Calc.Result.TotalOutlay", totalOutlay)
        };

        if (currentPrice is decimal current)
        {
            var marketValue = quantity * current;
            lines.Add(Line("Calc.Result.MarketValue", marketValue));
            lines.Add(Line("Calc.Result.UnrealizedPnl", marketValue - totalOutlay));
        }

        return new CalculatorResult(
            lines,
            "quantity = sum(amount / price); fees = invested × fee%; average entry = (invested + fees) / quantity; current value = quantity × current price",
            "Calc.Interp.Dca",
            [F(averageEntry), F(totalOutlay)]);
    }

    public static CalculatorResult BreakEven(decimal entry, TradeSide side, decimal openFeePct, decimal closeFeePct)
    {
        RequirePositive(entry, "Entry");
        RequireFeePct(openFeePct, "Open fee %");
        RequireFeePct(closeFeePct, "Close fee %");
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
        RequireFeePct(feePct, "Fee %");
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
        decimal account,
        decimal riskPct,
        decimal positionQty,
        decimal entry,
        TradeSide side,
        decimal feePct = 0m,
        decimal slippagePct = 0m)
    {
        RequirePositive(account, "Account");
        RequirePositive(riskPct, "Risk %");
        if (riskPct > 100) throw new CalculatorInputException("Risk % cannot exceed 100.");
        RequirePositive(positionQty, "Position size");
        RequirePositive(entry, "Entry");
        RequireFeePct(feePct, "Fee per side %");
        RequireFeePct(slippagePct, "Slippage per side %");

        var riskAmount = account * (riskPct / 100m);
        var unitRiskBudget = riskAmount / positionQty;
        var fee = feePct / 100m;
        var slippage = slippagePct / 100m;
        var effectiveEntry = side == TradeSide.Long
            ? entry * (1m + slippage)
            : entry * (1m - slippage);

        var effectiveStop = side == TradeSide.Long
            ? (effectiveEntry * (1m + fee) - unitRiskBudget) / (1m - fee)
            : (unitRiskBudget + effectiveEntry * (1m - fee)) / (1m + fee);
        var stopLoss = side == TradeSide.Long
            ? effectiveStop / (1m - slippage)
            : effectiveStop / (1m + slippage);

        if (stopLoss <= 0)
            throw new CalculatorInputException("Computed stop loss is not positive; reduce risk or position size.");
        if (side == TradeSide.Long ? stopLoss >= entry : stopLoss <= entry)
            throw new CalculatorInputException("Risk budget is too small to cover round-trip fees and slippage.");

        var priceRiskPerUnit = Math.Abs(effectiveEntry - effectiveStop);
        var feesPerUnit = (effectiveEntry + effectiveStop) * fee;
        var slippageCostPerUnit = Math.Abs(effectiveEntry - entry) + Math.Abs(effectiveStop - stopLoss);

        return new CalculatorResult(
            [
                Line("Calc.Result.RiskAmount", riskAmount),
                Line("Calc.Result.StopLossPrice", stopLoss),
                Line("Calc.Result.PriceRiskPerUnit", priceRiskPerUnit),
                Line("Calc.Result.FeesPerUnit", feesPerUnit),
                Line("Calc.Result.SlippageCostPerUnit", slippageCostPerUnit),
                Line("Calc.Result.TotalRiskPerUnit", priceRiskPerUnit + feesPerUnit)
            ],
            "stop price solves per-unit loss = adverse entry/stop distance + entry/stop fees for the configured risk budget; fees and slippage are per side",
            "Calc.Interp.StopLoss",
            [F(stopLoss)]);
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
        decimal? targetPct,
        decimal feePct = 0m,
        decimal slippagePct = 0m)
    {
        RequirePositive(entry, "Entry");
        RequirePositive(positionQty, "Position size");
        RequireFeePct(feePct, "Fee per side %");
        RequireFeePct(slippagePct, "Slippage per side %");

        if (targetPct is not null && targetRr is not null)
            throw new CalculatorInputException("Choose either target % or target R:R, not both.");

        decimal targetPrice;
        if (targetPct is decimal pct)
        {
            RequirePositive(pct, "Target %");
            if (side == TradeSide.Short && pct >= 100)
                throw new CalculatorInputException("Short target % must be below 100.");
            var move = entry * (pct / 100m);
            targetPrice = side == TradeSide.Long ? entry + move : entry - move;
        }
        else if (targetRr is decimal rr && stopLoss is decimal stop)
        {
            RequirePositive(rr, "Target R:R");
            RequirePositive(stop, "Stop loss");
            var stopIsAligned = side == TradeSide.Long ? stop < entry : stop > entry;
            if (!stopIsAligned)
                throw new CalculatorInputException("Stop loss must be below entry for long or above entry for short.");
            targetPrice = side == TradeSide.Long
                ? entry + Math.Abs(entry - stop) * rr
                : entry - Math.Abs(entry - stop) * rr;
        }
        else
        {
            throw new CalculatorInputException("Provide target %, or target R:R together with stop loss.");
        }

        var fee = feePct / 100m;
        var slippage = slippagePct / 100m;
        var effectiveEntry = side == TradeSide.Long
            ? entry * (1m + slippage)
            : entry * (1m - slippage);
        var effectiveTarget = side == TradeSide.Long
            ? targetPrice * (1m - slippage)
            : targetPrice * (1m + slippage);
        var grossProfit = Math.Abs(effectiveTarget - effectiveEntry) * positionQty;
        var fees = (effectiveEntry + effectiveTarget) * fee * positionQty;
        var netProfit = grossProfit - fees;
        var lines = new List<CalcLine>
        {
            Line("Calc.Result.TakeProfitPrice", targetPrice),
            Line("Calc.Result.GrossProfit", grossProfit),
            Line("Calc.Result.ExpectedProfit", netProfit),
            Line("Calc.Result.Fees", fees)
        };

        if (stopLoss is decimal stopLevel)
        {
            RequirePositive(stopLevel, "Stop loss");
            var stopIsAligned = side == TradeSide.Long ? stopLevel < entry : stopLevel > entry;
            if (!stopIsAligned)
                throw new CalculatorInputException("Stop loss must be below entry for long or above entry for short.");

            var effectiveStop = side == TradeSide.Long
                ? stopLevel * (1m - slippage)
                : stopLevel * (1m + slippage);
            var netRisk = (Math.Abs(effectiveEntry - effectiveStop)
                + (effectiveEntry + effectiveStop) * fee) * positionQty;
            lines.Add(Line("Calc.Result.NetRisk", netRisk));
            if (netProfit > 0)
            {
                lines.Add(Line("Calc.Result.RiskReward", netProfit / netRisk));
                lines.Add(Line("Calc.Result.BreakEvenWinRatePct", netRisk / (netRisk + netProfit) * 100m));
            }
            else
            {
                lines.Add(new CalcLine("Calc.Result.BreakEvenWinRatePct", "N/A"));
            }
        }

        return new CalculatorResult(
            lines,
            "net profit = adverse target move × quantity − entry/target fees; optional net R:R compares net profit with adverse stop loss after fees",
            "Calc.Interp.TakeProfit",
            [F(targetPrice), F(netProfit)]);
    }

    private static CalcLine Line(string key, decimal value) => new(key, F(value));

    private static string F(decimal v) => v.ToString("0.########", Inv);

    private static void RequirePositive(decimal v, string name)
    {
        if (v <= 0) throw new CalculatorInputException($"{name} must be greater than zero.");
    }

    private static void RequireFeePct(decimal value, string name)
    {
        RequireNonNegative(value, name);
        if (value >= 100)
            throw new CalculatorInputException($"{name} must be below 100.");
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
