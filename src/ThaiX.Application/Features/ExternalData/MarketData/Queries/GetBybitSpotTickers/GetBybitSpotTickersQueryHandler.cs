using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitSpotTickers;

/// <summary>
/// Handler for GetBybitSpotTickersQuery.
/// Retrieves Bybit spot market tickers via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetBybitSpotTickersQueryHandler
    : IRequestHandler<GetBybitSpotTickersQuery, BybitSpotTickersResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBybitSpotTickersQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BybitSpotTickersResponse> Handle(
        GetBybitSpotTickersQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetBybitSpotTickersAsync<InternalBybitApiResponse>(cancellationToken);

        if (apiResponse is null || apiResponse.RetCode != 0 || apiResponse.Result?.List is null)
        {
            return new BybitSpotTickersResponse
            {
                Data = new List<BybitSpotTickerDto>(),
                Success = false,
                Message = apiResponse?.RetMsg ?? "Failed to retrieve Bybit spot tickers from external API."
            };
        }

        var mappedData = apiResponse.Result.List
            .Where(ticker => ticker.Symbol != null
                && ticker.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .Select(ticker => new BybitSpotTickerDto
            {
                Symbol = ticker.Symbol ?? string.Empty,
                LastPrice = ticker.LastPrice,
                Bid1Price = ticker.Bid1Price,
                Ask1Price = ticker.Ask1Price,
                Price24hPcnt = ticker.Price24hPcnt,
                HighPrice24h = ticker.HighPrice24h,
                LowPrice24h = ticker.LowPrice24h,
                Volume24h = ticker.Volume24h,
                Turnover24h = ticker.Turnover24h,
                UsdIndexPrice = ticker.UsdIndexPrice
            })
            .ToList();

        return new BybitSpotTickersResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalBybitApiResponse
    {
        public int RetCode { get; init; }
        public string? RetMsg { get; init; }
        public InternalBybitTickersResult? Result { get; init; }
    }

    private sealed record InternalBybitTickersResult
    {
        public List<InternalBybitSpotTicker>? List { get; init; }
    }

    private sealed record InternalBybitSpotTicker
    {
        public string? Symbol { get; init; }
        public string? LastPrice { get; init; }
        public string? Bid1Price { get; init; }
        public string? Ask1Price { get; init; }
        public string? Price24hPcnt { get; init; }
        public string? HighPrice24h { get; init; }
        public string? LowPrice24h { get; init; }
        public string? Volume24h { get; init; }
        public string? Turnover24h { get; init; }
        public string? UsdIndexPrice { get; init; }
    }
}
