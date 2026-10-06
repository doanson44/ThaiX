using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24HrBySymbol;

/// <summary>
/// Handler for GetBinanceFuturesTicker24HrBySymbolQuery.
/// Retrieves Binance futures 24hr ticker by symbol.
/// </summary>
public sealed class GetBinanceFuturesTicker24HrBySymbolQueryHandler
    : IRequestHandler<GetBinanceFuturesTicker24HrBySymbolQuery, BinanceFuturesTicker24HrBySymbolResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceFuturesTicker24HrBySymbolQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceFuturesTicker24HrBySymbolResponse> Handle(
        GetBinanceFuturesTicker24HrBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new BinanceFuturesTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetBinanceFuturesTicker24HrBySymbolAsync<JsonElement>(symbol, cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceFuturesTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve Binance futures ticker from external API."
            };
        }

        if (apiResponse.ValueKind != JsonValueKind.Object)
        {
            return new BinanceFuturesTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Unexpected Binance futures ticker payload."
            };
        }

        if (!BinanceFuturesTickerMapper.TryMap(apiResponse, out var dto))
        {
            return new BinanceFuturesTicker24HrBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to map Binance futures ticker data."
            };
        }

        return new BinanceFuturesTicker24HrBySymbolResponse
        {
            Symbol = symbol,
            Data = dto,
            Success = true,
            Message = "Success"
        };
    }
}
