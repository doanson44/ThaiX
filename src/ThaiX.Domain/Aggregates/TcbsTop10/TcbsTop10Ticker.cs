namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>A ticker that was added or removed in a Top 10 portfolio update.</summary>
public sealed class TcbsTop10Ticker
{
    private TcbsTop10Ticker() { }

    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public string Ticker { get; private set; } = null!;
    public TcbsPortfolioChangeType ChangeType { get; private set; }

    public TcbsTop10Portfolio Portfolio { get; private set; } = null!;

    internal static TcbsTop10Ticker Create(Guid portfolioId, string ticker, TcbsPortfolioChangeType changeType) => new()
    {
        Id = Guid.NewGuid(),
        PortfolioId = portfolioId,
        Ticker = ticker.Trim().ToUpperInvariant(),
        ChangeType = changeType
    };
}
