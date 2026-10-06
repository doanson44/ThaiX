using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;

/// <summary>
/// Query to retrieve Dragon Capital fund portfolio data.
/// Supported fund codes: VF1, VF4, VFMVN30, VFMVND, VFMMID.
/// </summary>
public sealed record GetDragonCapitalFundPortfolioQuery : IAppQuery<DragonCapitalFundPortfolioResponse>
{
    public required string FundCode { get; init; }
}

/// <summary>
/// Response containing Dragon Capital fund portfolio data.
/// </summary>
public sealed record DragonCapitalFundPortfolioResponse
{
    public required string FundCode { get; init; }
    public DateTime? TradingDate { get; init; }
    public List<AssetTypeAllocation> AllocationByAssetTypes { get; init; } = [];
    public List<SectorAllocation> AllocationBySectors { get; init; } = [];
    public List<TopHoldingDto> Top10Holdings { get; init; } = [];
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Asset type allocation (HOSE, HNX, UPCOM, Cash).
/// </summary>
public sealed record AssetTypeAllocation
{
    public required string SourceName { get; init; }
    public decimal ValueAssetType { get; init; }
}

/// <summary>
/// Sector allocation with fund weight percentage.
/// </summary>
public sealed record SectorAllocation
{
    public required string IndustryLevel2 { get; init; }
    public decimal FundWeight { get; init; }
}

/// <summary>
/// Top holding stock information.
/// </summary>
public sealed record TopHoldingDto
{
    public required string AssetId { get; init; }
    public decimal Weight { get; init; }
    public string? Exchange { get; init; }
    public string? IndustryLevel { get; init; }
    public string? SectorLevel { get; init; }
    public long? HoldingVolume { get; init; }
    public decimal? MarketValue { get; init; }
}
