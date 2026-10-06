using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockPrices;

/// <summary>
/// Query to retrieve VnDirect stock price history by stock code.
/// </summary>
public sealed record GetVnDirectStockPricesQuery : IAppQuery<VnDirectStockPricesResponse>
{
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Response containing VnDirect stock price history.
/// </summary>
public sealed record VnDirectStockPricesResponse
{
    public string Code { get; init; } = string.Empty;
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectStockPriceDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect stock price item.
/// </summary>
public sealed record VnDirectStockPriceDto
{
    public string Code { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public string Floor { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal BasicPrice { get; init; }
    public decimal CeilingPrice { get; init; }
    public decimal FloorPrice { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Average { get; init; }
    public decimal AdOpen { get; init; }
    public decimal AdHigh { get; init; }
    public decimal AdLow { get; init; }
    public decimal AdClose { get; init; }
    public decimal AdAverage { get; init; }
    public decimal NmVolume { get; init; }
    public decimal NmValue { get; init; }
    public decimal PtVolume { get; init; }
    public decimal PtValue { get; init; }
    public decimal Change { get; init; }
    public decimal AdChange { get; init; }
    public decimal PctChange { get; init; }
}
