using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record CreateJsonBinCommand : IAppCommand<string>
{
    /// <summary>Optional. When blank, a unique category-prefixed code is generated.</summary>
    public string? Code { get; init; }

    public required string Name { get; init; }
    public JsonBinCategories Category { get; init; } = JsonBinCategories.Unknown;
    public required string ContentJson { get; init; }
    public string ContentType { get; init; } = "application/json";
    public bool Compress { get; init; }
    public string? Tags { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? ExpiredAtUtc { get; init; }
}
