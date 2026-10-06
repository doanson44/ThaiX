using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;

public sealed record GetTcbsTop10PortfoliosQuery : IAppQuery<TcbsTop10PortfoliosResult>, ICacheableQuery
{
    public string CacheKey => CacheKeys.TcbsTop10Data.PortfoliosList();
    public TimeSpan? Expiration => TimeSpan.FromHours(6);
    public string CacheGroup => CacheGroups.TcbsTop10Data;
    public bool IsVersionedList => false;
}

public sealed record TcbsTop10PortfoliosResult
{
    public required IReadOnlyList<TcbsTop10PortfolioDto> Portfolios { get; init; }
    public required IReadOnlyList<string> CurrentHoldings { get; init; }
    public required IReadOnlyList<string> AllTimeHoldings { get; init; }
}

public sealed record TcbsTop10PortfolioDto
{
    public required Guid Id { get; init; }
    public required long SourceContentId { get; init; }
    public required DateOnly PostedAt { get; init; }
    public required DateOnly EffectiveDate { get; init; }
    public required IReadOnlyList<string> AddedTickers { get; init; }
    public required IReadOnlyList<string> RemovedTickers { get; init; }
    public required IReadOnlyList<TcbsTop10ImageDto> Images { get; init; }
}

public sealed record TcbsTop10ImageDto
{
    public required int ImageType { get; init; }
    public required Guid FileGuid { get; init; }
}
