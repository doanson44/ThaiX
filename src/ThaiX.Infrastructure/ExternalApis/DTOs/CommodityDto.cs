using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Commodity data (type=1): gold, silver, oil, metals, agricultural products.
/// </summary>
public sealed class CommodityDto
{
    [JsonPropertyName("_id")]
    public IdInfo? Id { get; set; }

    [JsonPropertyName("goods")]
    public string? Goods { get; set; }

    [JsonPropertyName("last")]
    public double Last { get; set; }

    [JsonPropertyName("high")]
    public double High { get; set; }

    [JsonPropertyName("low")]
    public double Low { get; set; }

    [JsonPropertyName("change")]
    public double Change { get; set; }

    [JsonPropertyName("changePercent")]
    public double ChangePercent { get; set; }

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
