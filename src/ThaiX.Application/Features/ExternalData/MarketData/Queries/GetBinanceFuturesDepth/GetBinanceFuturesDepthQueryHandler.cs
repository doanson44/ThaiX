using MediatR;
using System.Globalization;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesDepth;

/// <summary>
/// Handler for GetBinanceFuturesDepthQuery.
/// Retrieves Binance futures depth for a symbol.
/// </summary>
public sealed class GetBinanceFuturesDepthQueryHandler
    : IRequestHandler<GetBinanceFuturesDepthQuery, BinanceFuturesDepthResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceFuturesDepthQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceFuturesDepthResponse> Handle(
        GetBinanceFuturesDepthQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new BinanceFuturesDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetBinanceFuturesDepthAsync<JsonElement>(symbol, cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceFuturesDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve Binance futures depth from external API."
            };
        }

        if (apiResponse.ValueKind != JsonValueKind.Object)
        {
            return new BinanceFuturesDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Unexpected Binance futures depth payload."
            };
        }

        if (!TryReadInt64(apiResponse, "lastUpdateId", out var lastUpdateId))
        {
            return new BinanceFuturesDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Binance returned incomplete depth data."
            };
        }

        var bids = ReadOrderBook(apiResponse, "bids");
        var asks = ReadOrderBook(apiResponse, "asks");

        if (bids.Count == 0 || asks.Count == 0)
        {
            return new BinanceFuturesDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Binance returned empty depth data."
            };
        }

        TryReadInt64(apiResponse, "E", out var eventTime);
        TryReadInt64(apiResponse, "T", out var transactionTime);

        return new BinanceFuturesDepthResponse
        {
            Symbol = symbol,
            Data = new BinanceFuturesDepthDataDto
            {
                Asks = asks,
                Bids = bids,
                LastUpdateId = lastUpdateId,
                EventTime = eventTime,
                TransactionTime = transactionTime
            },
            Success = true,
            Message = "Success"
        };
    }

    private static List<List<decimal>> ReadOrderBook(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<List<decimal>>();
        foreach (var row in property.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            decimal? price = null;
            decimal? quantity = null;
            var index = 0;

            foreach (var item in row.EnumerateArray())
            {
                if (index == 0 && TryReadDecimal(item, out var p))
                {
                    price = p;
                }
                else if (index == 1 && TryReadDecimal(item, out var q))
                {
                    quantity = q;
                }

                if (++index >= 2)
                {
                    break;
                }
            }

            if (price.HasValue && quantity.HasValue)
            {
                results.Add(new List<decimal> { price.Value, quantity.Value });
            }
        }

        return results;
    }

    private static bool TryReadDecimal(JsonElement element, out decimal value)
    {
        value = 0;
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetDecimal(out value);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString();
            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        return false;
    }

    private static bool TryReadInt64(JsonElement element, string propertyName, out long value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetInt64(out value);
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();
            return long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        return false;
    }
}
