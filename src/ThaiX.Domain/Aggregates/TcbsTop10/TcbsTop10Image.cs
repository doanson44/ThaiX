namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>An image attached to a TCBS Top 10 portfolio update, identified by its iwealthclub file GUID.</summary>
public sealed class TcbsTop10Image
{
    private TcbsTop10Image() { }

    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public TcbsPortfolioImageType ImageType { get; private set; }

    /// <summary>File GUID from iwealthclub platform. Used to build download URL.</summary>
    public Guid FileGuid { get; private set; }

    public TcbsTop10Portfolio Portfolio { get; private set; } = null!;

    internal static TcbsTop10Image Create(Guid portfolioId, TcbsPortfolioImageType imageType, Guid fileGuid) => new()
    {
        Id = Guid.NewGuid(),
        PortfolioId = portfolioId,
        ImageType = imageType,
        FileGuid = fileGuid
    };
}
