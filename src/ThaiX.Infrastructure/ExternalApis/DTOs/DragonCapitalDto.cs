using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Dragon Capital API response wrapper.
/// </summary>
public sealed class DragonCapitalApiResponse
{
    [JsonPropertyName("returnValue")]
    public DragonCapitalReturnValue? ReturnValue { get; set; }

    [JsonPropertyName("cacheable")]
    public bool Cacheable { get; set; }
}

/// <summary>
/// Dragon Capital return value containing fund portfolio data.
/// </summary>
public sealed class DragonCapitalReturnValue
{
    [JsonPropertyName("fundCode")]
    public string? FundCode { get; set; }

    [JsonPropertyName("tradingDate")]
    public DateTime? TradingDate { get; set; }

    [JsonPropertyName("allocationByAssetTypes")]
    public List<AllocationByAssetType>? AllocationByAssetTypes { get; set; }

    [JsonPropertyName("allocationBySectors")]
    public List<AllocationBySector>? AllocationBySectors { get; set; }

    [JsonPropertyName("top10Holding")]
    public List<TopHolding>? Top10Holding { get; set; }
}

/// <summary>
/// Asset type allocation (HOSE, HNX, UPCOM, Cash).
/// </summary>
public sealed class AllocationByAssetType
{
    [JsonPropertyName("sourceName")]
    public string? SourceName { get; set; }

    [JsonPropertyName("valueAssetType")]
    public decimal ValueAssetType { get; set; }
}

/// <summary>
/// Sector allocation with fund weight.
/// </summary>
public sealed class AllocationBySector
{
    [JsonPropertyName("industryLevel2")]
    public string? IndustryLevel2 { get; set; }

    [JsonPropertyName("fundWeight")]
    public Dictionary<string, decimal>? FundWeight { get; set; }
}

/// <summary>
/// Top holding information.
/// </summary>
public sealed class TopHolding
{
    [JsonPropertyName("assetId")]
    public string? AssetId { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("industryLevel")]
    public string? IndustryLevel { get; set; }

    [JsonPropertyName("sectorLevel")]
    public string? SectorLevel { get; set; }

    [JsonPropertyName("holdingVolume")]
    public long? HoldingVolume { get; set; }

    [JsonPropertyName("marketValue")]
    public decimal? MarketValue { get; set; }

    [JsonPropertyName("foreignRoom")]
    public long? ForeignRoom { get; set; }
}
