using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;

/// <summary>
/// Handler for GetBinanceFuturesTicker24HrQuery.
/// Retrieves Binance futures 24hr tickers for all symbols.
/// </summary>
public sealed class GetBinanceFuturesTicker24HrQueryHandler
    : IRequestHandler<GetBinanceFuturesTicker24HrQuery, BinanceFuturesTicker24HrResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceFuturesTicker24HrQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceFuturesTicker24HrResponse> Handle(
        GetBinanceFuturesTicker24HrQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetBinanceFuturesTicker24HrAsync<JsonElement>(cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceFuturesTicker24HrResponse
            {
                Data = new List<BinanceFuturesTickerDto>(),
                Success = false,
                Message = "Failed to retrieve Binance futures tickers from external API."
            };
        }

        if (apiResponse.ValueKind != JsonValueKind.Array)
        {
            return new BinanceFuturesTicker24HrResponse
            {
                Data = new List<BinanceFuturesTickerDto>(),
                Success = false,
                Message = "Unexpected Binance futures ticker payload."
            };
        }

        var items = new List<BinanceFuturesTickerDto>();
        foreach (var element in apiResponse.EnumerateArray())
        {
            if (BinanceFuturesTickerMapper.TryMap(element, out var dto))
            {
                items.Add(dto);
            }
        }

        if (items.Count == 0)
        {
            return new BinanceFuturesTicker24HrResponse
            {
                Data = new List<BinanceFuturesTickerDto>(),
                Success = false,
                Message = "No Binance futures ticker data returned."
            };
        }

        var usdtItems = items
            .Where(x => x.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new BinanceFuturesTicker24HrResponse
        {
            Data = usdtItems,
            Success = true,
            Message = "Success"
        };
    }
}
