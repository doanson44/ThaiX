using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBins;

public sealed record GetJsonBinsQuery : PagedRequest, IAppQuery<PagedResult<JsonBinListItemDto>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }
    public JsonBinCategories? Category { get; init; }
    public Guid? ReferenceId { get; init; }
    public bool? IncludeExpired { get; init; }

    public string CacheKey => CacheKeys.JsonBins.List(
        SearchTerm,
        Category,
        ReferenceId,
        IncludeExpired,
        PageNumber,
        PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.JsonBins;
    public bool IsVersionedList => true;
}
