using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinByCode;

public sealed record GetJsonBinByCodeQuery : IAppQuery<JsonBinDetailDto?>, ICacheableQuery
{
    public required string Code { get; init; }

    public string CacheKey => CacheKeys.JsonBins.ByCode((Code ?? string.Empty).Trim().ToUpperInvariant());
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.JsonBins;
    public bool IsVersionedList => false;
}
