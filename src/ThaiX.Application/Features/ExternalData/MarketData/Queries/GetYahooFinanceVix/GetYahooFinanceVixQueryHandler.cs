using MediatR;
using Skender.Stock.Indicators;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

/// <summary>
/// Handler for GetYahooFinanceVixQuery.
/// Retrieves VIX (Volatility Index) data via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetYahooFinanceVixQueryHandler
    : IRequestHandler<GetYahooFinanceVixQuery, YahooFinanceVixResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetYahooFinanceVixQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<YahooFinanceVixResponse> Handle(
        GetYahooFinanceVixQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetYahooFinanceVixChartAsync<InternalYahooFinanceResponse>(cancellationToken);

        if (apiResponse?.Chart?.Result is null || apiResponse.Chart.Result.Count == 0)
        {
            return new YahooFinanceVixResponse
            {
                Symbol = "^VIX",
                Success = false,
                Message = "Failed to retrieve VIX data from Yahoo Finance API."
            };
        }

        var result = apiResponse.Chart.Result[0];
        var meta = result.Meta;

        if (meta is null)
        {
            return new YahooFinanceVixResponse
            {
                Symbol = "^VIX",
                Success = false,
                Message = "VIX data meta is null."
            };
        }

        var lastUpdateTime = meta.RegularMarketTime.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(meta.RegularMarketTime.Value).DateTime
            : (DateTime?)null;

        var currentValue = meta.RegularMarketPrice;

        // Build paired time-series, exclude null closes
        var timestamps = result.Timestamp ?? [];
        var rawClose = result.Indicators?.Quote?.FirstOrDefault()?.Close ?? [];

        var series = timestamps
            .Zip(rawClose, (ts, close) => (Timestamp: ts, Close: close))
            .Where(x => x.Close.HasValue)
            .Select(x => (Timestamp: x.Timestamp, Close: x.Close!.Value))
            .OrderBy(x => x.Timestamp)
            .ToList();

        if (series.Count < 6)
        {
            return new YahooFinanceVixResponse
            {
                Symbol = meta.Symbol ?? "^VIX",
                CurrentPrice = currentValue,
                CurrentValue = currentValue,
                PreviousClose = meta.ChartPreviousClose,
                DayHigh = meta.RegularMarketDayHigh,
                DayLow = meta.RegularMarketDayLow,
                FiftyTwoWeekHigh = meta.FiftyTwoWeekHigh,
                FiftyTwoWeekLow = meta.FiftyTwoWeekLow,
                LastUpdateTime = lastUpdateTime,
                Success = true,
                Message = "Insufficient data for feature engineering."
            };
        }

        // EMA via Skender.Stock.Indicators
        var quotes = series
            .Select(x => new Quote
            {
                Date = DateTimeOffset.FromUnixTimeSeconds(x.Timestamp).DateTime,
                Close = x.Close
            })
            .ToList();

        var ema10Results = quotes.GetEma(10).ToList();
        var ema20Results = quotes.GetEma(20).ToList();

        var lastEma10 = ema10Results.LastOrDefault(e => e.Ema.HasValue)?.Ema;
        var lastEma20 = ema20Results.LastOrDefault(e => e.Ema.HasValue)?.Ema;

        // Momentum: (current - previous) / previous
        var prices = series.Select(x => x.Close).ToList();
        int last = prices.Count - 1;

        decimal? momentum3D = last >= 3 && prices[last - 3] != 0m
            ? (prices[last] - prices[last - 3]) / prices[last - 3]
            : null;

        decimal? momentum5D = last >= 5 && prices[last - 5] != 0m
            ? (prices[last] - prices[last - 5]) / prices[last - 5]
            : null;

        // Percentile: rank of current value within dataset
        decimal current = prices[last];
        int rank = prices.Count(p => p < current);
        double percentile = (double)rank / prices.Count;

        // Spike detection: abs change > 10% vs previous close
        bool isSpike = last >= 1
            && prices[last - 1] != 0m
            && Math.Abs((double)((prices[last] - prices[last - 1]) / prices[last - 1])) > 0.1;

        // Regime detection
        bool isLow = percentile < 0.5;
        bool isRising = lastEma10.HasValue && lastEma20.HasValue && lastEma10.Value > lastEma20.Value;

        string regime = (isLow, isRising) switch
        {
            (true, false) => "RiskOn",
            (true, true) => "EarlyRisk",
            (false, true) => "Panic",
            (false, false) => "Recovery"
        };

        return new YahooFinanceVixResponse
        {
            Symbol = meta.Symbol ?? "^VIX",
            CurrentPrice = currentValue,
            CurrentValue = currentValue,
            PreviousClose = meta.ChartPreviousClose,
            DayHigh = meta.RegularMarketDayHigh,
            DayLow = meta.RegularMarketDayLow,
            FiftyTwoWeekHigh = meta.FiftyTwoWeekHigh,
            FiftyTwoWeekLow = meta.FiftyTwoWeekLow,
            LastUpdateTime = lastUpdateTime,
            Ema10 = lastEma10.HasValue ? (decimal)lastEma10.Value : null,
            Ema20 = lastEma20.HasValue ? (decimal)lastEma20.Value : null,
            Momentum3D = momentum3D,
            Momentum5D = momentum5D,
            Percentile = percentile,
            IsSpike = isSpike,
            Regime = regime,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalYahooFinanceResponse
    {
        public InternalChart? Chart { get; init; }
    }

    private sealed record InternalChart
    {
        public List<InternalResult>? Result { get; init; }
    }

    private sealed record InternalResult
    {
        public InternalMeta? Meta { get; init; }
        public List<long>? Timestamp { get; init; }
        public InternalIndicators? Indicators { get; init; }
    }

    private sealed record InternalIndicators
    {
        public List<InternalQuote>? Quote { get; init; }
    }

    private sealed record InternalQuote
    {
        public List<decimal?>? Close { get; init; }
    }

    private sealed record InternalMeta
    {
        public string? Symbol { get; init; }
        public decimal? RegularMarketPrice { get; init; }
        public long? RegularMarketTime { get; init; }
        public decimal? ChartPreviousClose { get; init; }
        public decimal? RegularMarketDayHigh { get; init; }
        public decimal? RegularMarketDayLow { get; init; }
        public decimal? FiftyTwoWeekHigh { get; init; }
        public decimal? FiftyTwoWeekLow { get; init; }
    }
}
