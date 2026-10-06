using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCommodities;

/// <summary>
/// Query to retrieve commodity prices from CafeF external API.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetCommoditiesQuery : IAppQuery<CommoditiesResponse>;

/// <summary>
/// Response containing commodity prices data from CafeF (type=1).
/// </summary>
public sealed record CommoditiesResponse
{
    public required List<CommodityDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Commodity price information (gold, silver, oil, metals, agricultural products).
/// </summary>
public sealed record CommodityDto
{
    public required string Goods { get; init; }
    public decimal Last { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePercent { get; init; }
    public string? LastUpdate { get; init; }
}
