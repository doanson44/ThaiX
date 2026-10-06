namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Exchange where a stock is listed.
/// </summary>
public enum StockExchange
{
    /// <summary>Ho Chi Minh Stock Exchange.</summary>
    HOSE = 1,

    /// <summary>Hanoi Stock Exchange.</summary>
    HNX = 2,

    /// <summary>Unlisted Public Company Market.</summary>
    UPCOM = 3,

    /// <summary>Foreign exchange (e.g. NYSE, NASDAQ).</summary>
    Other = 4
}
