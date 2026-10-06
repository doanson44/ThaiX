using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>
/// Aggregate root representing a monthly TCBS Top 10 portfolio update.
/// Tracks which tickers were added or removed and the associated images.
/// </summary>
public sealed class TcbsTop10Portfolio : BaseEntity
{
    private readonly List<TcbsTop10Ticker> _tickers = new();
    private readonly List<TcbsTop10Image> _images = new();

    // Private constructor for EF Core
    private TcbsTop10Portfolio() { }

    /// <summary>Content ID from the iwealthclub platform (used as dedup key and pagination cursor).</summary>
    public long SourceContentId { get; private set; }

    /// <summary>Date the portfolio update was posted on iwealthclub.</summary>
    public DateOnly PostedAt { get; private set; }

    /// <summary>Date from which the updated portfolio is effective.</summary>
    public DateOnly EffectiveDate { get; private set; }

    /// <summary>Tickers added or removed in this update.</summary>
    public IReadOnlyCollection<TcbsTop10Ticker> Tickers => _tickers.AsReadOnly();

    /// <summary>Images attached to this portfolio update.</summary>
    public IReadOnlyCollection<TcbsTop10Image> Images => _images.AsReadOnly();

    /// <summary>Creates a new portfolio update record.</summary>
    public static TcbsTop10Portfolio Create(long sourceContentId, DateOnly postedAt, DateOnly effectiveDate)
    {
        return new TcbsTop10Portfolio
        {
            Id = Guid.NewGuid(),
            SourceContentId = sourceContentId,
            PostedAt = postedAt,
            EffectiveDate = effectiveDate
        };
    }

    /// <summary>Records a ticker change (add or remove) for this update.</summary>
    public void AddTicker(string ticker, TcbsPortfolioChangeType changeType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ticker, nameof(ticker));
        _tickers.Add(TcbsTop10Ticker.Create(Id, ticker, changeType));
    }

    /// <summary>Attaches an image to this portfolio update.</summary>
    public void AddImage(TcbsPortfolioImageType imageType, Guid fileGuid)
    {
        _images.Add(TcbsTop10Image.Create(Id, imageType, fileGuid));
    }
}
