using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record UpdateJsonBinCommand : IAppCommand<string>
{
    public required Guid Id { get; init; }

    /// <summary>Optional. When blank, the existing code is kept.</summary>
    public string? Code { get; init; }

    public required string Name { get; init; }
    public JsonBinCategories Category { get; init; }
    public string? ContentJson { get; init; }
    public string ContentType { get; init; } = "application/json";
    public bool? Compress { get; init; }
    public string? Tags { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? ExpiredAtUtc { get; init; }
    public bool ClearExpiration { get; init; }
}
