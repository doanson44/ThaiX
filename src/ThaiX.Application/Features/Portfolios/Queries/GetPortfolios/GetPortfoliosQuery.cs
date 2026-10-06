using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Queries.GetPortfolios;

public sealed record GetPortfoliosQuery : PagedRequest, IAppQuery<PagedResult<PortfolioListItemDto>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }
    public Guid OwnerId { get; init; }

    public string CacheKey => CacheKeys.Portfolios.List(OwnerId, SearchTerm, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Portfolios;
    public bool IsVersionedList => true;
}

public sealed record PortfolioListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required PortfolioType PortfolioType { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
