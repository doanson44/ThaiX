using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.Banking.Queries.GetSacombankExchangeRates;

/// <summary>
/// Query to retrieve Sacombank exchange rates.
/// Includes currencies (USD, EUR, GBP, etc.) and gold (SJC, SBJ).
/// </summary>
public sealed record GetSacombankExchangeRatesQuery : IAppQuery<SacombankExchangeRatesResponse>;

/// <summary>
/// Response containing Sacombank exchange rates.
/// </summary>
public sealed record SacombankExchangeRatesResponse
{
    public DateTime? UpdateDate { get; init; }
    public List<ExchangeRateItem> ExchangeRates { get; init; } = [];
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Exchange rate item (currency or gold).
/// </summary>
public sealed record ExchangeRateItem
{
    public required string CurrencyCode { get; init; }
    public decimal BidInCash { get; init; }
    public decimal BidInTransfer { get; init; }
    public decimal OfferInCash { get; init; }
    public decimal OfferInTransfer { get; init; }
    public string? CreatedDate { get; init; }
}
