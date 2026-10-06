namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>Whether a ticker was added to or removed from the Top 10 portfolio.</summary>
public enum TcbsPortfolioChangeType
{
    /// <summary>Ticker duoc them vao danh muc.</summary>
    Added = 0,

    /// <summary>Ticker bi loai bo khoi danh muc.</summary>
    Removed = 1
}
