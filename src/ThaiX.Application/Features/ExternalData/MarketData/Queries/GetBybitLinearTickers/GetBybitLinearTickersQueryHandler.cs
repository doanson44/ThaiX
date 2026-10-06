using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitLinearTickers;

/// <summary>
/// Handler for GetBybitLinearTickersQuery.
/// Retrieves Bybit linear/perpetual market tickers via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetBybitLinearTickersQueryHandler
    : IRequestHandler<GetBybitLinearTickersQuery, BybitLinearTickersResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBybitLinearTickersQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BybitLinearTickersResponse> Handle(
        GetBybitLinearTickersQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetBybitLinearTickersAsync<InternalBybitApiResponse>(cancellationToken);

        if (apiResponse is null || apiResponse.RetCode != 0 || apiResponse.Result?.List is null)
        {
            return new BybitLinearTickersResponse
            {
                Data = new List<BybitLinearTickerDto>(),
                Success = false,
                Message = apiResponse?.RetMsg ?? "Failed to retrieve Bybit linear tickers from external API."
            };
        }

        var mappedData = apiResponse.Result.List
            .Where(ticker => ticker.Symbol != null
                && ticker.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .Select(ticker => new BybitLinearTickerDto
            {
                Symbol = ticker.Symbol ?? string.Empty,
                LastPrice = ticker.LastPrice,
                IndexPrice = ticker.IndexPrice,
                MarkPrice = ticker.MarkPrice,
                Price24hPcnt = ticker.Price24hPcnt,
                HighPrice24h = ticker.HighPrice24h,
                LowPrice24h = ticker.LowPrice24h,
                Volume24h = ticker.Volume24h,
                Turnover24h = ticker.Turnover24h,
                OpenInterest = ticker.OpenInterest,
                FundingRate = ticker.FundingRate,
                NextFundingTime = ticker.NextFundingTime
            })
            .ToList();

        return new BybitLinearTickersResponse
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
        public List<InternalBybitLinearTicker>? List { get; init; }
    }

    private sealed record InternalBybitLinearTicker
    {
        public string? Symbol { get; init; }
        public string? LastPrice { get; init; }
        public string? IndexPrice { get; init; }
        public string? MarkPrice { get; init; }
        public string? Price24hPcnt { get; init; }
        public string? HighPrice24h { get; init; }
        public string? LowPrice24h { get; init; }
        public string? Volume24h { get; init; }
        public string? Turnover24h { get; init; }
        public string? OpenInterest { get; init; }
        public string? FundingRate { get; init; }
        public string? NextFundingTime { get; init; }
    }
}
