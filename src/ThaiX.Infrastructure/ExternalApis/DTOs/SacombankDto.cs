using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Sacombank exchange rate item (currency or gold).
/// </summary>
public sealed class SacombankExchangeRateItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("bidInCash")]
    public decimal BidInCash { get; set; }

    [JsonPropertyName("bidInTransfer")]
    public decimal BidInTransfer { get; set; }

    [JsonPropertyName("offerInCash")]
    public decimal OfferInCash { get; set; }

    [JsonPropertyName("offerInTransfer")]
    public decimal OfferInTransfer { get; set; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }
}
