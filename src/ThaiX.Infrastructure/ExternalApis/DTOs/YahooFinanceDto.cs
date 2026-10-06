using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Yahoo Finance chart API response.
/// </summary>
public sealed class YahooFinanceChartResponse
{
    [JsonPropertyName("chart")]
    public YahooFinanceChart? Chart { get; set; }
}

/// <summary>
/// Yahoo Finance chart data.
/// </summary>
public sealed class YahooFinanceChart
{
    [JsonPropertyName("result")]
    public List<YahooFinanceResult>? Result { get; set; }

    [JsonPropertyName("error")]
    public object? Error { get; set; }
}

/// <summary>
/// Yahoo Finance result containing meta and indicators.
/// </summary>
public sealed class YahooFinanceResult
{
    [JsonPropertyName("meta")]
    public YahooFinanceMeta? Meta { get; set; }

    [JsonPropertyName("timestamp")]
    public List<long>? Timestamp { get; set; }

    [JsonPropertyName("indicators")]
    public YahooFinanceIndicators? Indicators { get; set; }
}

/// <summary>
/// Yahoo Finance meta information.
/// </summary>
public sealed class YahooFinanceMeta
{
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchangeName")]
    public string? ExchangeName { get; set; }

    [JsonPropertyName("instrumentType")]
    public string? InstrumentType { get; set; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; set; }

    [JsonPropertyName("regularMarketTime")]
    public long? RegularMarketTime { get; set; }

    [JsonPropertyName("chartPreviousClose")]
    public decimal? ChartPreviousClose { get; set; }

    [JsonPropertyName("regularMarketDayHigh")]
    public decimal? RegularMarketDayHigh { get; set; }

    [JsonPropertyName("regularMarketDayLow")]
    public decimal? RegularMarketDayLow { get; set; }

    [JsonPropertyName("fiftyTwoWeekHigh")]
    public decimal? FiftyTwoWeekHigh { get; set; }

    [JsonPropertyName("fiftyTwoWeekLow")]
    public decimal? FiftyTwoWeekLow { get; set; }
}

/// <summary>
/// Yahoo Finance indicators (quote, adjclose).
/// </summary>
public sealed class YahooFinanceIndicators
{
    [JsonPropertyName("quote")]
    public List<YahooFinanceQuote>? Quote { get; set; }
}

/// <summary>
/// Yahoo Finance quote data (open, high, low, close, volume).
/// </summary>
public sealed class YahooFinanceQuote
{
    [JsonPropertyName("open")]
    public List<decimal?>? Open { get; set; }

    [JsonPropertyName("high")]
    public List<decimal?>? High { get; set; }

    [JsonPropertyName("low")]
    public List<decimal?>? Low { get; set; }

    [JsonPropertyName("close")]
    public List<decimal?>? Close { get; set; }

    [JsonPropertyName("volume")]
    public List<long?>? Volume { get; set; }
}
