using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatest;

/// <summary>
/// Query to retrieve VnDirect latest ratios by stock code.
/// </summary>
public sealed record GetVnDirectRatiosLatestQuery : IAppQuery<VnDirectRatiosLatestResponse>
{
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Response containing VnDirect latest ratios by stock code.
/// </summary>
public sealed record VnDirectRatiosLatestResponse
{
    public string Code { get; init; } = string.Empty;
    public List<VnDirectRatioItemDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect ratio item.
/// </summary>
public sealed record VnDirectRatioItemDto
{
    public string RatioCode { get; init; } = string.Empty;
    public decimal Value { get; init; }
}
