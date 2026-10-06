using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCurrencies;

/// <summary>
/// Query to retrieve currency exchange rates from CafeF external API.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetCurrenciesQuery : IAppQuery<CurrenciesResponse>;

/// <summary>
/// Response containing currency exchange rates data from CafeF (type=2).
/// </summary>
public sealed record CurrenciesResponse
{
    public required List<CurrencyDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Currency exchange rate information (USD, EUR, GBP, etc.).
/// </summary>
public sealed record CurrencyDto
{
    public required string ProductName { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal OtherPrice { get; init; }
    public decimal PrevPrice { get; init; }
    public decimal Change24H { get; init; }
    public decimal Change7D { get; init; }
    public string? UpdateDate { get; init; }
}
