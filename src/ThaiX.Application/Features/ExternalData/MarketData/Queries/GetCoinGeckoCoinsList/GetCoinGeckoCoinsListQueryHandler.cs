using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinsList;

/// <summary>
/// Handler for GetCoinGeckoCoinsListQuery.
/// Retrieves CoinGecko coins list via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetCoinGeckoCoinsListQueryHandler
    : IRequestHandler<GetCoinGeckoCoinsListQuery, CoinGeckoCoinsListResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetCoinGeckoCoinsListQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<CoinGeckoCoinsListResponse> Handle(
        GetCoinGeckoCoinsListQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetCoinGeckoCoinsListAsync<List<InternalCoinGeckoCoin>>(cancellationToken);

        if (apiResponse is null)
        {
            return new CoinGeckoCoinsListResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve CoinGecko coins list from external API."
            };
        }

        var mappedData = apiResponse
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Id) &&
                !string.IsNullOrWhiteSpace(item.Symbol) &&
                !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => new CoinGeckoCoinDto
            {
                Id = item.Id!,
                Symbol = item.Symbol!,
                Name = item.Name!
            })
            .ToList();

        return new CoinGeckoCoinsListResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalCoinGeckoCoin
    {
        public string? Id { get; init; }
        public string? Symbol { get; init; }
        public string? Name { get; init; }
    }
}