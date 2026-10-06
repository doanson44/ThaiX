using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCryptocurrencies;

/// <summary>
/// Query to retrieve cryptocurrency prices from CafeF external API.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetCryptocurrenciesQuery : IAppQuery<CryptocurrenciesResponse>;

/// <summary>
/// Response containing cryptocurrency prices data from CafeF (type=3).
/// </summary>
public sealed record CryptocurrenciesResponse
{
    public required List<CryptocurrencyDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Cryptocurrency price information (Bitcoin, Ethereum, etc.).
/// </summary>
public sealed record CryptocurrencyDto
{
    public required string Name { get; init; }
    public required string Symbol { get; init; }
    public decimal Price { get; init; }
    public decimal MarketCap { get; init; }
    public decimal Vol24H { get; init; }
    public decimal Change24H { get; init; }
    public decimal Change7D { get; init; }
    public string? LastUpdate { get; init; }
}
