using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;

/// <summary>
/// Handler for GetMexcSpotTicker24HrBySymbolQuery.
/// Retrieves MEXC spot 24h ticker statistics by symbol.
/// </summary>
public sealed class GetMexcSpotTicker24HrBySymbolQueryHandler
    : IRequestHandler<GetMexcSpotTicker24HrBySymbolQuery, MexcSpotTicker24HrBySymbolResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcSpotTicker24HrBySymbolQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcSpotTicker24HrBySymbolResponse> Handle(
        GetMexcSpotTicker24HrBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new MexcSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetMexcSpotTicker24HrBySymbolAsync<InternalMexcSpotTicker24Hr>(symbol, cancellationToken);

        if (apiResponse is null || string.IsNullOrWhiteSpace(apiResponse.Symbol))
        {
            return new MexcSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve MEXC spot 24h ticker data from external API."
            };
        }

        return new MexcSpotTicker24HrBySymbolResponse
        {
            Symbol = symbol,
            Data = new MexcSpotTicker24HrDto
            {
                Symbol = apiResponse.Symbol,
                PriceChange = apiResponse.PriceChange,
                PriceChangePercent = apiResponse.PriceChangePercent,
                PrevClosePrice = apiResponse.PrevClosePrice,
                LastPrice = apiResponse.LastPrice,
                BidPrice = apiResponse.BidPrice,
                BidQty = apiResponse.BidQty,
                AskPrice = apiResponse.AskPrice,
                AskQty = apiResponse.AskQty,
                OpenPrice = apiResponse.OpenPrice,
                HighPrice = apiResponse.HighPrice,
                LowPrice = apiResponse.LowPrice,
                Volume = apiResponse.Volume,
                QuoteVolume = apiResponse.QuoteVolume,
                OpenTime = apiResponse.OpenTime,
                CloseTime = apiResponse.CloseTime,
                Count = apiResponse.Count
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcSpotTicker24Hr
    {
        public string Symbol { get; init; } = string.Empty;
        public string? PriceChange { get; init; }
        public string? PriceChangePercent { get; init; }
        public string? PrevClosePrice { get; init; }
        public string? LastPrice { get; init; }
        public string? BidPrice { get; init; }
        public string? BidQty { get; init; }
        public string? AskPrice { get; init; }
        public string? AskQty { get; init; }
        public string? OpenPrice { get; init; }
        public string? HighPrice { get; init; }
        public string? LowPrice { get; init; }
        public string? Volume { get; init; }
        public string? QuoteVolume { get; init; }
        public long OpenTime { get; init; }
        public long CloseTime { get; init; }
        public long? Count { get; init; }
    }
}
