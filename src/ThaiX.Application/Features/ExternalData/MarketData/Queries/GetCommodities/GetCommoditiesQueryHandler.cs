using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCommodities;

/// <summary>
/// Handler for GetCommoditiesQuery.
/// Retrieves commodity prices from CafeF external API via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetCommoditiesQueryHandler
    : IRequestHandler<GetCommoditiesQuery, CommoditiesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetCommoditiesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<CommoditiesResponse> Handle(
        GetCommoditiesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _externalDataService
            .GetCommoditiesAsync<CommoditiesResponse>(cancellationToken);

        if (result is null)
        {
            return new CommoditiesResponse
            {
                Data = new List<CommodityDto>(),
                Success = false,
                Message = "Failed to retrieve commodity prices from external API."
            };
        }

        return result;
    }
}
