using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinsList;

/// <summary>
/// Query to retrieve CoinGecko coins list (id, symbol, name).
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetCoinGeckoCoinsListQuery : IAppQuery<CoinGeckoCoinsListResponse>;

/// <summary>
/// Response containing CoinGecko coins list.
/// </summary>
public sealed record CoinGeckoCoinsListResponse
{
    public required List<CoinGeckoCoinDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Coin item from CoinGecko /coins/list endpoint.
/// </summary>
public sealed record CoinGeckoCoinDto
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
}