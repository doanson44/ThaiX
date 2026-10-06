using System.Globalization;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Trading;

namespace ThaiX.Infrastructure.Services.MarketData;

public sealed class ExternalMarketDataService : IExternalMarketDataService
{
    private static readonly HashSet<string> SupportedCryptoTimeframes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Min1", "Min5", "Min15", "Min30", "Min60",
        "Hour4", "Hour8", "Day1", "Week1", "Month1"
    };

    private static readonly Dictionary<string, string> SpotTimeframeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Min1"] = "1m",
        ["Min5"] = "5m",
        ["Min15"] = "15m",
        ["Min30"] = "30m",
        ["Min60"] = "60m",
        ["Hour4"] = "4h",
        ["Day1"] = "1d",
        ["Week1"] = "1W",
        ["Month1"] = "1M"
    };

    private readonly IExternalDataService _external;

    public ExternalMarketDataService(IExternalDataService external)
    {
        _external = external;
    }

    private async Task<object> GetRawKlinesAsync(
        string symbol,
        MarketType marketType,
        string timeframe,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        var normalized = symbol.Trim().ToUpperInvariant();

        return marketType switch
        {
            MarketType.Crypto => await GetCryptoRawAsync(normalized, timeframe, ct),
            MarketType.CryptoSpot => await GetSpotCryptoRawAsync(normalized, timeframe, ct),
            MarketType.Stock => await GetStockRawAsync(normalized, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(marketType), marketType, "Unsupported market type.")
        };
    }

    public async Task<IReadOnlyList<Kline>> GetKlinesAsync(
        string symbol,
        MarketType marketType,
        string timeframe,
        bool useLongTermTimeframe,
        CancellationToken ct)
    {
        var raw = await GetRawKlinesAsync(symbol, marketType, timeframe, ct);
        var klines = KlineMapper.Map(raw, marketType);

        if (marketType != MarketType.Stock)
        {
            return klines;
        }

        if (!useLongTermTimeframe)
        {
            return klines;
        }

        var normalizedTimeframe = timeframe.Trim();
        return normalizedTimeframe.ToUpperInvariant() switch
        {
            "WEEK1" => AggregateByWeek(klines),
            "MONTH1" => AggregateByMonth(klines),
            _ => throw new ArgumentException("Invalid stock long-term timeframe. Supported values: Week1, Month1.", nameof(timeframe))
        };
    }

    private static IReadOnlyList<Kline> AggregateByWeek(IReadOnlyList<Kline> dayKlines)
    {
        var calendar = CultureInfo.InvariantCulture.Calendar;

        return dayKlines
            .GroupBy(x =>
            {
                var week = calendar.GetWeekOfYear(x.Time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                return (x.Time.Year, Week: week);
            })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Week)
            .Select(BuildAggregateKline)
            .ToList();
    }

    private static IReadOnlyList<Kline> AggregateByMonth(IReadOnlyList<Kline> dayKlines)
    {
        return dayKlines
            .GroupBy(x => (x.Time.Year, x.Time.Month))
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(BuildAggregateKline)
            .ToList();
    }

    private static Kline BuildAggregateKline(IEnumerable<Kline> group)
    {
        var ordered = group.OrderBy(x => x.Time).ToList();
        var first = ordered[0];
        var last = ordered[^1];

        return new Kline
        {
            Time = first.Time,
            Open = first.Open,
            High = ordered.Max(x => x.High),
            Low = ordered.Min(x => x.Low),
            Close = last.Close,
            Volume = ordered.Sum(x => x.Volume)
        };
    }

    private async Task<object> GetSpotCryptoRawAsync(string symbol, string timeframe, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(timeframe))
        {
            throw new ArgumentException("Timeframe is required for spot crypto market.", nameof(timeframe));
        }

        var normalizedTimeframe = timeframe.Trim();
        if (!SpotTimeframeMap.TryGetValue(normalizedTimeframe, out var spotInterval))
        {
            throw new ArgumentException(
                "Invalid spot crypto timeframe. Supported values: Min1, Min5, Min15, Min30, Min60, Hour4, Day1, Week1, Month1.",
                nameof(timeframe));
        }

        var raw = await _external.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(symbol, spotInterval, ct);
        if (raw is null)
        {
            throw new InvalidOperationException("Failed to fetch spot crypto klines from external API.");
        }

        return raw;
    }

    private async Task<object> GetCryptoRawAsync(string symbol, string timeframe, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(timeframe))
        {
            throw new ArgumentException("Timeframe is required for crypto market.", nameof(timeframe));
        }

        var normalizedTimeframe = timeframe.Trim();
        if (!SupportedCryptoTimeframes.Contains(normalizedTimeframe))
        {
            throw new ArgumentException(
                "Invalid crypto timeframe. Supported values: Min1, Min5, Min15, Min30, Min60, Hour4, Hour8, Day1, Week1, Month1.",
                nameof(timeframe));
        }

        var raw = await _external.GetMexcContractKlineAsync<MexcContractKlineRawResponse>(symbol, normalizedTimeframe, ct);
        if (raw is null || !raw.Success || raw.Code != 0 || raw.Data is null)
        {
            throw new InvalidOperationException("Failed to fetch crypto klines from external API.");
        }

        return raw;
    }

    private async Task<object> GetStockRawAsync(string symbol, CancellationToken ct)
    {
        var raw = await _external.GetVnDirectStockPricesAsync<VnDirectStockPricesRawResponse>(symbol, ct);
        if (raw is null || raw.Data is null || raw.Data.Count == 0)
        {
            throw new InvalidOperationException("Failed to fetch stock klines from external API.");
        }

        return raw;
    }
}
