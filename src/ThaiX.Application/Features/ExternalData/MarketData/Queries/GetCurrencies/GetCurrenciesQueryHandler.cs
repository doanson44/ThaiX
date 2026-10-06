using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCurrencies;

/// <summary>
/// Handler for GetCurrenciesQuery.
/// Retrieves currency exchange rates from CafeF external API via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetCurrenciesQueryHandler
    : IRequestHandler<GetCurrenciesQuery, CurrenciesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetCurrenciesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<CurrenciesResponse> Handle(
        GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _externalDataService
            .GetCurrenciesAsync<CurrenciesResponse>(cancellationToken);

        if (result is null)
        {
            return new CurrenciesResponse
            {
                Data = new List<CurrencyDto>(),
                Success = false,
                Message = "Failed to retrieve currency exchange rates from external API."
            };
        }

        return result;
    }
}
