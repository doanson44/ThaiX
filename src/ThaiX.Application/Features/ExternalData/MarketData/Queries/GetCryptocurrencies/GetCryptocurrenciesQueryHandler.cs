using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCryptocurrencies;

/// <summary>
/// Handler for GetCryptocurrenciesQuery.
/// Retrieves cryptocurrency prices from CafeF external API via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetCryptocurrenciesQueryHandler
    : IRequestHandler<GetCryptocurrenciesQuery, CryptocurrenciesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetCryptocurrenciesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<CryptocurrenciesResponse> Handle(
        GetCryptocurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _externalDataService
            .GetCryptocurrenciesAsync<CryptocurrenciesResponse>(cancellationToken);

        if (result is null)
        {
            return new CryptocurrenciesResponse
            {
                Data = new List<CryptocurrencyDto>(),
                Success = false,
                Message = "Failed to retrieve cryptocurrency prices from external API."
            };
        }

        return result;
    }
}
