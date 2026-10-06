using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRecommendations;

/// <summary>
/// Handler for GetVnDirectRecommendationsQuery.
/// Retrieves VnDirect recommendations from external API via proxy.
/// </summary>
public sealed class GetVnDirectRecommendationsQueryHandler
    : IRequestHandler<GetVnDirectRecommendationsQuery, VnDirectRecommendationsResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectRecommendationsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectRecommendationsResponse> Handle(
        GetVnDirectRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnDirectRecommendationsAsync<InternalVnDirectRecommendationsResponse>(cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectRecommendationsResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect recommendations from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new VnDirectRecommendationDto
            {
                Code = x.Code,
                Firm = x.Firm,
                Type = x.Type,
                ReportDate = x.ReportDate,
                Source = x.Source,
                Analyst = x.Analyst,
                ReportPrice = x.ReportPrice,
                TargetPrice = x.TargetPrice,
                AvgTargetPrice = x.AvgTargetPrice
            })
            .ToList();

        return new VnDirectRecommendationsResponse
        {
            CurrentPage = apiResponse.CurrentPage,
            Size = apiResponse.Size,
            TotalElements = apiResponse.TotalElements,
            TotalPages = apiResponse.TotalPages,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalVnDirectRecommendationsResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalVnDirectRecommendationDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectRecommendationDto
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
}
