using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Cryptocurrency data (type=3): Bitcoin, Ethereum, etc.
/// </summary>
public sealed class CryptocurrencyDto
{
    [JsonPropertyName("_id")]
    public IdInfo? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("marketCap")]
    public double MarketCap { get; set; }

    [JsonPropertyName("vol24H")]
    public double Vol24H { get; set; }

    [JsonPropertyName("totalVol")]
    public double TotalVol { get; set; }

    [JsonPropertyName("change24H")]
    public double Change24H { get; set; }

    [JsonPropertyName("change7D")]
    public double Change7D { get; set; }

    [JsonPropertyName("last_update")]
    public string? LastUpdate { get; set; }

    public sealed class IdInfo
    {
        [JsonPropertyName("Timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("Machine")]
        public int Machine { get; set; }

        [JsonPropertyName("Pid")]
        public int Pid { get; set; }

        [JsonPropertyName("Increment")]
        public int Increment { get; set; }

        [JsonPropertyName("CreationTime")]
        public string? CreationTime { get; set; }
    }
}
