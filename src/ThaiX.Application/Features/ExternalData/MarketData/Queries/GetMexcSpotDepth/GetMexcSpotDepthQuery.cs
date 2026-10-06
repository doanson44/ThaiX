using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotDepth;

/// <summary>
/// Query to retrieve MEXC spot order book depth by symbol.
/// Limit is fixed at 5000 in provider configuration.
/// </summary>
public sealed record GetMexcSpotDepthQuery : IAppQuery<MexcSpotDepthResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing MEXC spot order book depth.
/// </summary>
public sealed record MexcSpotDepthResponse
{
    public required string Symbol { get; init; }
    public MexcSpotDepthDataDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC spot depth payload.
/// bids/asks rows are [price, quantity].
/// </summary>
public sealed record MexcSpotDepthDataDto
{
    public long LastUpdateId { get; init; }
    public long? Timestamp { get; init; }
    public required List<List<string>> Bids { get; init; }
    public required List<List<string>> Asks { get; init; }
}
