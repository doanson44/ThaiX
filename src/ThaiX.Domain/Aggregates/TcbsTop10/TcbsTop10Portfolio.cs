using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>
/// Aggregate root representing a TCBS portfolio update, from either the iWealth Club stream
/// (monthly Analyst Top 10) or the TCBS potential stock list page.
/// Tracks which tickers were added or removed and the attached files.
/// </summary>
public sealed class TcbsTop10Portfolio : BaseEntity
{
    private readonly List<TcbsTop10Ticker> _tickers = new();
    private readonly List<TcbsTop10Attachment> _attachments = new();

    // Private constructor for EF Core
    private TcbsTop10Portfolio() { }

    /// <summary>Source that produced this row. Distinguishes iWealth Club posts from potential stock pulls.</summary>
    public TcbsPortfolioSource Source { get; private set; }

    /// <summary>Content ID from the iwealthclub platform (dedup key and pagination cursor). Null for other sources.</summary>
    public long? SourceContentId { get; private set; }

    /// <summary>Date the update was posted, or the date the change was detected for non-iwealthclub sources.</summary>
    public DateOnly PostedAt { get; private set; }

    /// <summary>Date from which the updated portfolio is effective.</summary>
    public DateOnly EffectiveDate { get; private set; }

    /// <summary>Tickers added or removed in this update.</summary>
    public IReadOnlyCollection<TcbsTop10Ticker> Tickers => _tickers.AsReadOnly();

    /// <summary>Images and PDFs attached to this portfolio update.</summary>
    public IReadOnlyCollection<TcbsTop10Attachment> Attachments => _attachments.AsReadOnly();

    /// <summary>Creates a new portfolio update record.</summary>
    public static TcbsTop10Portfolio Create(
        long? sourceContentId,
        DateOnly postedAt,
        DateOnly effectiveDate,
        TcbsPortfolioSource source = TcbsPortfolioSource.IWealthClub)
    {
        return new TcbsTop10Portfolio
        {
            Id = Guid.NewGuid(),
            Source = source,
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

    /// <summary>Attaches an iwealthclub image to this portfolio update.</summary>
    public void AddImage(TcbsPortfolioImageType imageType, Guid fileGuid)
    {
        _attachments.Add(TcbsTop10Attachment.CreateImage(Id, imageType, fileGuid));
    }

    /// <summary>Attaches a PDF research file to this portfolio update.</summary>
    public void AddPdf(string url)
    {
        _attachments.Add(TcbsTop10Attachment.CreatePdf(Id, url));
    }
}
