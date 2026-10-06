using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24HrBySymbol;

/// <summary>
/// Handler for GetBinanceSpotTicker24HrBySymbolQuery.
/// Retrieves Binance spot 24hr ticker by symbol.
/// </summary>
public sealed class GetBinanceSpotTicker24HrBySymbolQueryHandler
    : IRequestHandler<GetBinanceSpotTicker24HrBySymbolQuery, BinanceSpotTicker24HrBySymbolResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceSpotTicker24HrBySymbolQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceSpotTicker24HrBySymbolResponse> Handle(
        GetBinanceSpotTicker24HrBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new BinanceSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetBinanceSpotTicker24HrBySymbolAsync<JsonElement>(symbol, cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve Binance spot ticker from external API."
            };
        }

        if (apiResponse.ValueKind != JsonValueKind.Object)
        {
            return new BinanceSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Unexpected Binance spot ticker payload."
            };
        }

        if (!BinanceSpotTickerMapper.TryMap(apiResponse, out var dto))
        {
            return new BinanceSpotTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to map Binance spot ticker data."
            };
        }

        return new BinanceSpotTicker24HrBySymbolResponse
        {
            Symbol = symbol,
            Data = dto,
            Success = true,
            Message = "Success"
        };
    }
}
