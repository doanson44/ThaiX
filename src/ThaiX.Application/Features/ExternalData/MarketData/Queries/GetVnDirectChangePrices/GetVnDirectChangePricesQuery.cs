using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectChangePrices;

/// <summary>
/// Query to retrieve VnDirect change prices for market indices (VNINDEX, HNX, UPCOM, VN30, VN30F1M).
/// </summary>
public sealed record GetVnDirectChangePricesQuery : IAppQuery<VnDirectChangePricesResponse>;

/// <summary>
/// Response containing VnDirect change prices data.
/// </summary>
public sealed record VnDirectChangePricesResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectChangePriceDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Change price item for a market index.
/// </summary>
public sealed record VnDirectChangePriceDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal BopPrice { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePct { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
}
