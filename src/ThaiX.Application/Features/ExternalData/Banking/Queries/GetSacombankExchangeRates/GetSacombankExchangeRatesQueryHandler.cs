using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.Banking.Queries.GetSacombankExchangeRates;

/// <summary>
/// Handler for GetSacombankExchangeRatesQuery.
/// Retrieves Sacombank exchange rates via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetSacombankExchangeRatesQueryHandler
    : IRequestHandler<GetSacombankExchangeRatesQuery, SacombankExchangeRatesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetSacombankExchangeRatesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<SacombankExchangeRatesResponse> Handle(
        GetSacombankExchangeRatesQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetSacombankExchangeRatesAsync<InternalSacombankResponse>(cancellationToken);

        if (apiResponse is null || apiResponse.StatusCode != 200 || apiResponse.Data is null)
        {
            return new SacombankExchangeRatesResponse
            {
                ExchangeRates = [],
                Success = false,
                Message = apiResponse?.Msg ?? "Failed to retrieve Sacombank exchange rates from external API."
            };
        }

        var updateDate = apiResponse.Date > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(apiResponse.Date).DateTime
            : (DateTime?)null;

        var exchangeRates = apiResponse.Data
            .Select(item => new ExchangeRateItem
            {
                CurrencyCode = item.CurrencyCode ?? string.Empty,
                BidInCash = item.BidInCash,
                BidInTransfer = item.BidInTransfer,
                OfferInCash = item.OfferInCash,
                OfferInTransfer = item.OfferInTransfer,
                CreatedDate = item.CreatedDate
            })
            .ToList();

        return new SacombankExchangeRatesResponse
        {
            UpdateDate = updateDate,
            ExchangeRates = exchangeRates,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalSacombankResponse
    {
        public int StatusCode { get; init; }
        public string? Msg { get; init; }
        public long Date { get; init; }
        public List<InternalExchangeRateItem>? Data { get; init; }
    }

    private sealed record InternalExchangeRateItem
    {
        public string? CurrencyCode { get; init; }
        public decimal BidInCash { get; init; }
        public decimal BidInTransfer { get; init; }
        public decimal OfferInCash { get; init; }
        public decimal OfferInTransfer { get; init; }
        public string? CreatedDate { get; init; }
    }
}
