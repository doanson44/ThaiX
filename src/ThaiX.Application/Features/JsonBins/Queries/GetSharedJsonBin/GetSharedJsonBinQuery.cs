using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Queries.GetSharedJsonBin;

/// <summary>
/// Anonymous public read by share token. No auth required at the endpoint.
/// </summary>
public sealed record GetSharedJsonBinQuery : IAppQuery<JsonBinSharedDto?>
{
    public required string Token { get; init; }
}

/// <summary>
/// Public share payload -- content only, no internal ids/audit fields.
/// </summary>
public sealed record JsonBinSharedDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string ContentType { get; init; }
    public required string ContentJson { get; init; }
    public DateTime? ShareExpiresAtUtc { get; init; }
}
