using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.MarketData;

/// <summary>
/// A single matched order (transaction tick) for a stock symbol, sourced from 24HMoney's
/// SSI transaction-list endpoint. One row per matched order per symbol per trade date.
/// </summary>
public sealed class TwentyFourHMoneyTransaction : BaseEntity
{
    private TwentyFourHMoneyTransaction() { }

    public string Symbol { get; private set; } = string.Empty;

    /// <summary>Trading calendar date (Vietnam) this transaction belongs to.</summary>
    public DateOnly TradeDate { get; private set; }

    /// <summary>Time-of-day the order matched, as reported by the source (e.g. "13:45:41").</summary>
    public string TradeTime { get; private set; } = string.Empty;

    public decimal Price { get; private set; }
    public decimal Change { get; private set; }
    public long MatchQuantity { get; private set; }

    /// <summary>Cumulative matched volume for the symbol at this point in the trading day.</summary>
    public long TotalVolume { get; private set; }

    /// <summary>Raw side code from the source (e.g. "bu"/"sd").</summary>
    public string Side { get; private set; } = string.Empty;

    public static TwentyFourHMoneyTransaction Create(
        string symbol,
        DateOnly tradeDate,
        string tradeTime,
        decimal price,
        decimal change,
        long matchQuantity,
        long totalVolume,
        string side)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        return new TwentyFourHMoneyTransaction
        {
            Id = Guid.NewGuid(),
            Symbol = symbol.Trim().ToUpperInvariant(),
            TradeDate = tradeDate,
            TradeTime = tradeTime,
            Price = price,
            Change = change,
            MatchQuantity = matchQuantity,
            TotalVolume = totalVolume,
            Side = side
        };
    }
}
