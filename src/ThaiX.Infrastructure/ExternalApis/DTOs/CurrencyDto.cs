using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Currency exchange rate data (type=2): USD, EUR, GBP, etc.
/// </summary>
public sealed class CurrencyDto
{
    [JsonPropertyName("ProductName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("CurrentPrice")]
    public double CurrentPrice { get; set; }

    [JsonPropertyName("OtherPrice")]
    public double OtherPrice { get; set; }

    [JsonPropertyName("PrevPrice")]
    public double PrevPrice { get; set; }

    [JsonPropertyName("UpdateDate")]
    public string? UpdateDate { get; set; }

    [JsonPropertyName("DbId")]
    public string? DbId { get; set; }

    [JsonPropertyName("Last")]
    public double Last { get; set; }

    [JsonPropertyName("High")]
    public double High { get; set; }

    [JsonPropertyName("Low")]
    public double Low { get; set; }

    [JsonPropertyName("change24H")]
    public double Change24H { get; set; }

    [JsonPropertyName("change7D")]
    public double Change7D { get; set; }

    [JsonPropertyName("CompanyName")]
    public string? CompanyName { get; set; }
}
