using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRecommendations;

/// <summary>
/// Query to retrieve VnDirect recommendations list.
/// </summary>
public sealed record GetVnDirectRecommendationsQuery : IAppQuery<VnDirectRecommendationsResponse>;

/// <summary>
/// Response containing VnDirect recommendations data.
/// </summary>
public sealed record VnDirectRecommendationsResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectRecommendationDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect recommendation item.
/// </summary>
public sealed record VnDirectRecommendationDto
{
    public string Code { get; init; } = string.Empty;
    public string Firm { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string ReportDate { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Analyst { get; init; } = string.Empty;
    public decimal ReportPrice { get; init; }
    public decimal TargetPrice { get; init; }
    public decimal AvgTargetPrice { get; init; }
}
