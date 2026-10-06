using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinContent;

public sealed record GetJsonBinContentQuery : IAppQuery<JsonBinContentDto?>, ICacheableQuery
{
    public required Guid Id { get; init; }
    public bool Decompress { get; init; } = true;

    public string CacheKey => $"{CacheKeys.JsonBins.Content(Id)}:d={Decompress}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.JsonBins;
    public bool IsVersionedList => false;
}
