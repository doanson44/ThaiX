using System.Globalization;
using System.Text.Json;
using ThaiX.Application.Trading;

namespace ThaiX.Infrastructure.Services.MarketData;

public static class KlineMapper
{
    public static IReadOnlyList<Kline> Map(object raw, MarketType marketType)
    {
        ArgumentNullException.ThrowIfNull(raw);

        return marketType switch
        {
            MarketType.Crypto => MapCrypto(raw),
            MarketType.CryptoSpot => MapCryptoSpot(raw),
            MarketType.Stock => MapStock(raw),
            _ => throw new ArgumentOutOfRangeException(nameof(marketType), marketType, "Unsupported market type.")
        };
    }

    private static IReadOnlyList<Kline> MapCrypto(object raw)
    {
        if (raw is not MexcContractKlineRawResponse response || response.Data is null)
        {
            throw new ArgumentException("Invalid crypto kline payload.", nameof(raw));
        }

        var data = response.Data;
        var count = data.Time.Count;
        if (count == 0 ||
            data.Open.Count != count ||
            data.High.Count != count ||
            data.Low.Count != count ||
            data.Close.Count != count ||
            data.Vol.Count != count)
        {
            throw new ArgumentException("Inconsistent crypto kline arrays.", nameof(raw));
        }

        var klines = new List<Kline>(count);
        for (var i = 0; i < count; i++)
        {
            klines.Add(new Kline
            {
                Time = FromUnix(data.Time[i]),
                Open = data.Open[i],
                High = data.High[i],
                Low = data.Low[i],
                Close = data.Close[i],
                Volume = data.Vol[i]
            });
        }

        return klines.OrderBy(x => x.Time).ToList();
    }

    private static IReadOnlyList<Kline> MapCryptoSpot(object raw)
    {
        if (raw is not List<List<JsonElement>> rows)
        {
            throw new ArgumentException("Invalid spot kline payload: expected List<List<JsonElement>>.", nameof(raw));
        }

        var klines = new List<Kline>(rows.Count);
        foreach (var row in rows)
        {
            if (row.Count < 6)
            {
                continue;
            }

            long openTimeRaw;
            try
            {
                openTimeRaw = row[0].ValueKind == JsonValueKind.Number
                    ? row[0].GetInt64()
                    : long.Parse(row[0].GetString() ?? "0", CultureInfo.InvariantCulture);
            }
            catch
            {
                continue;
            }

            static decimal ParseField(JsonElement el)
            {
                return el.ValueKind == JsonValueKind.Number
                    ? el.GetDecimal()
                    : decimal.Parse(el.GetString() ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            klines.Add(new Kline
            {
                Time = FromUnix(openTimeRaw),
                Open = ParseField(row[1]),
                High = ParseField(row[2]),
                Low = ParseField(row[3]),
                Close = ParseField(row[4]),
                Volume = ParseField(row[5])
            });
        }

        return klines.OrderBy(x => x.Time).ToList();
    }

    private static IReadOnlyList<Kline> MapStock(object raw)
    {
        if (raw is not VnDirectStockPricesRawResponse response)
        {
            throw new ArgumentException("Invalid stock kline payload.", nameof(raw));
        }

        var klines = new List<Kline>(response.Data.Count);
        foreach (var x in response.Data)
        {
            if (!DateTime.TryParse(x.Date, out var parsed))
            {
                continue;
            }

            var utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            klines.Add(new Kline
            {
                Time = utc,
                Open = x.Open,
                High = x.High,
                Low = x.Low,
                Close = x.Close,
                Volume = x.NmVolume
            });
        }

        if (klines.Count == 0)
        {
            throw new ArgumentException("No valid stock klines could be mapped.", nameof(raw));
        }

        return klines.OrderBy(x => x.Time).ToList();
    }

    private static DateTime FromUnix(long value)
    {
        return value > 9_999_999_999
            ? DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime
            : DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
    }
}

public sealed record MexcContractKlineRawResponse
{
    public bool Success { get; init; }
    public int Code { get; init; }
    public MexcContractKlineRawData? Data { get; init; }
}

public sealed record MexcContractKlineRawData
{
    public List<long> Time { get; init; } = [];
    public List<decimal> Open { get; init; } = [];
    public List<decimal> Close { get; init; } = [];
    public List<decimal> High { get; init; } = [];
    public List<decimal> Low { get; init; } = [];
    public List<decimal> Vol { get; init; } = [];
}

public sealed record VnDirectStockPricesRawResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectStockPriceRawDto> Data { get; init; } = [];
}

public sealed record VnDirectStockPriceRawDto
{
    public string Date { get; init; } = string.Empty;
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal NmVolume { get; init; }
}
