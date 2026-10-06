using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;

public sealed record GetJsonBinByIdQuery : IAppQuery<JsonBinDetailDto?>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.JsonBins.ById(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.JsonBins;
    public bool IsVersionedList => false;
}
