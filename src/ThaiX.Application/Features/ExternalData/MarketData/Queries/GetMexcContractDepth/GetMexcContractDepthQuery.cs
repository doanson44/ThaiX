using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractDepth;

/// <summary>
/// Query to retrieve MEXC contract depth by symbol.
/// Example symbols: BTC_USDT, ETH_USDT.
/// </summary>
public sealed record GetMexcContractDepthQuery : IAppQuery<MexcContractDepthResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing MEXC contract depth data.
/// </summary>
public sealed record MexcContractDepthResponse
{
    public required string Symbol { get; init; }
    public MexcContractDepthDataDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC contract order book depth data.
/// asks/bids rows use [price, volume, orderCount].
/// </summary>
public sealed record MexcContractDepthDataDto
{
    public required List<List<decimal>> Asks { get; init; }
    public required List<List<decimal>> Bids { get; init; }
    public long Version { get; init; }
    public long Timestamp { get; init; }
}
