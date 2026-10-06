using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinMarket;

/// <summary>
/// Query to retrieve CoinGecko coin market data by coin id.
/// Example coin ids: bitcoin, ethereum.
/// </summary>
public sealed record GetCoinGeckoCoinMarketQuery : IAppQuery<CoinGeckoCoinMarketResponse>
{
    public string CoinId { get; init; } = string.Empty;
}

/// <summary>
/// Response containing CoinGecko market data for a coin id.
/// </summary>
public sealed record CoinGeckoCoinMarketResponse
{
    public required string CoinId { get; init; }
    public CoinGeckoCoinMarketDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Coin market item from CoinGecko /coins/markets endpoint.
/// </summary>
public sealed record CoinGeckoCoinMarketDto
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public string? Image { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal? MarketCap { get; init; }
    public int? MarketCapRank { get; init; }
    public decimal? High24H { get; init; }
    public decimal? Low24H { get; init; }
    public decimal? PriceChangePercentage24H { get; init; }
}