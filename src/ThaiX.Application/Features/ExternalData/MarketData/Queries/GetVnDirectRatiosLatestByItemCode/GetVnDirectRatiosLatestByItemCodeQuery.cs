using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatestByItemCode;

/// <summary>
/// Query to retrieve VnDirect latest market ratios by predefined item codes.
/// </summary>
public sealed record GetVnDirectRatiosLatestByItemCodeQuery : IAppQuery<VnDirectRatiosLatestByItemCodeResponse>;

/// <summary>
/// Response containing VnDirect latest market ratios by item codes.
/// </summary>
public sealed record VnDirectRatiosLatestByItemCodeResponse
{
    public List<VnDirectRatioByItemCodeDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect ratio item keyed by itemCode.
/// </summary>
public sealed record VnDirectRatioByItemCodeDto
{
    public string Code { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public string ReportDate { get; init; } = string.Empty;
    public string ItemCode { get; init; } = string.Empty;
    public string RatioCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public decimal Value { get; init; }
}
