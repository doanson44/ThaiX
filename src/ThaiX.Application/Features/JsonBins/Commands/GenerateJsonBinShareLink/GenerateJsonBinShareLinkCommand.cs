using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Commands.GenerateJsonBinShareLink;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record GenerateJsonBinShareLinkCommand : IAppCommand<GenerateJsonBinShareLinkResult>
{
    public required Guid Id { get; init; }

    /// <summary>Optional share-link expiry. Null means the link does not expire (until revoked).</summary>
    public DateTime? ShareExpiresAtUtc { get; init; }
}

public sealed record GenerateJsonBinShareLinkResult
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Token { get; init; }
    public required string RelativePath { get; init; }
    public DateTime? ShareExpiresAtUtc { get; init; }
}
