using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24Hr;

/// <summary>
/// Handler for GetBinanceSpotTicker24HrQuery.
/// Retrieves Binance spot 24hr tickers for all symbols.
/// </summary>
public sealed class GetBinanceSpotTicker24HrQueryHandler
    : IRequestHandler<GetBinanceSpotTicker24HrQuery, BinanceSpotTicker24HrResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceSpotTicker24HrQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceSpotTicker24HrResponse> Handle(
        GetBinanceSpotTicker24HrQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetBinanceSpotTicker24HrAsync<JsonElement>(cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceSpotTicker24HrResponse
            {
                Data = new List<BinanceSpotTickerDto>(),
                Success = false,
                Message = "Failed to retrieve Binance spot tickers from external API."
            };
        }

        if (apiResponse.ValueKind != JsonValueKind.Array)
        {
            return new BinanceSpotTicker24HrResponse
            {
                Data = new List<BinanceSpotTickerDto>(),
                Success = false,
                Message = "Unexpected Binance spot ticker payload."
            };
        }

        var items = new List<BinanceSpotTickerDto>();
        foreach (var element in apiResponse.EnumerateArray())
        {
            if (BinanceSpotTickerMapper.TryMap(element, out var dto))
            {
                items.Add(dto);
            }
        }

        if (items.Count == 0)
        {
            return new BinanceSpotTicker24HrResponse
            {
                Data = new List<BinanceSpotTickerDto>(),
                Success = false,
                Message = "No Binance spot ticker data returned."
            };
        }

        var usdtItems = items
            .Where(x => x.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new BinanceSpotTicker24HrResponse
        {
            Data = usdtItems,
            Success = true,
            Message = "Success"
        };
    }
}
