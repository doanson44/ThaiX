using System.Globalization;
using System.Text.Json;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;

internal static class BinanceFuturesTickerMapper
{
    public static bool TryMap(JsonElement element, out BinanceFuturesTickerDto dto)
    {
        dto = default!;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!TryReadString(element, "symbol", out var symbol))
        {
            return false;
        }

        dto = new BinanceFuturesTickerDto
        {
            Symbol = symbol!,
            PriceChange = ReadDecimalOptional(element, "priceChange"),
            PriceChangePercent = ReadDecimalOptional(element, "priceChangePercent"),
            WeightedAvgPrice = ReadDecimalOptional(element, "weightedAvgPrice"),
            LastPrice = ReadDecimalOptional(element, "lastPrice"),
            LastQty = ReadDecimalOptional(element, "lastQty"),
            OpenPrice = ReadDecimalOptional(element, "openPrice"),
            HighPrice = ReadDecimalOptional(element, "highPrice"),
            LowPrice = ReadDecimalOptional(element, "lowPrice"),
            Volume = ReadDecimalOptional(element, "volume"),
            QuoteVolume = ReadDecimalOptional(element, "quoteVolume"),
            OpenTime = ReadInt64Optional(element, "openTime"),
            CloseTime = ReadInt64Optional(element, "closeTime"),
            FirstId = ReadInt64Optional(element, "firstId"),
            LastId = ReadInt64Optional(element, "lastId"),
            Count = ReadInt64Optional(element, "count")
        };

        return true;
    }

    private static bool TryReadString(JsonElement element, string propertyName, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        value = property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.ToString(),
            _ => null
        };

        return !string.IsNullOrWhiteSpace(value);
    }

    private static decimal? ReadDecimalOptional(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetDecimal(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static long ReadInt64Optional(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt64(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();
            if (long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }
}
