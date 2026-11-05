namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>
/// A file attached to a TCBS portfolio update: either an iwealthclub image (file GUID)
/// or a PDF research file referenced by URL.
/// </summary>
public sealed class TcbsTop10Attachment
{
    private TcbsTop10Attachment() { }

    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public TcbsAttachmentKind Kind { get; private set; }

    /// <summary>Role of the image (only set when <see cref="Kind"/> is <see cref="TcbsAttachmentKind.Image"/>).</summary>
    public TcbsPortfolioImageType? ImageType { get; private set; }

    /// <summary>File GUID from the iwealthclub platform. Used to build the download URL.</summary>
    public Guid? FileGuid { get; private set; }

    /// <summary>Absolute URL of the attached PDF (only set when <see cref="Kind"/> is <see cref="TcbsAttachmentKind.Pdf"/>).</summary>
    public string? Url { get; private set; }

    public TcbsTop10Portfolio Portfolio { get; private set; } = null!;

    internal static TcbsTop10Attachment CreateImage(Guid portfolioId, TcbsPortfolioImageType imageType, Guid fileGuid) => new()
    {
        Id = Guid.NewGuid(),
        PortfolioId = portfolioId,
        Kind = TcbsAttachmentKind.Image,
        ImageType = imageType,
        FileGuid = fileGuid
    };

    internal static TcbsTop10Attachment CreatePdf(Guid portfolioId, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        return new TcbsTop10Attachment
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            Kind = TcbsAttachmentKind.Pdf,
            Url = url.Trim()
        };
    }
}
