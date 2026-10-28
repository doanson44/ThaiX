using System.Text.Json.Serialization;
using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;

public sealed class GetInvestingVnIndexAnalysisQueryHandler
    : IRequestHandler<GetInvestingVnIndexAnalysisQuery, InvestingVnIndexAnalysisResponse>
{
    private const string InsufficientHistoryMessage = "Insufficient daily candle history for indicator computation.";

    private readonly IExternalDataService _externalDataService;
    private readonly IIndicatorService _indicatorService;
    private readonly IKlineExecutionEngine _executionEngine;

    public GetInvestingVnIndexAnalysisQueryHandler(
        IExternalDataService externalDataService,
        IIndicatorService indicatorService,
        IKlineExecutionEngine executionEngine)
    {
        _externalDataService = externalDataService;
        _indicatorService = indicatorService;
        _executionEngine = executionEngine;
    }

    public async Task<InvestingVnIndexAnalysisResponse> Handle(
        GetInvestingVnIndexAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetInvestingVnIndexCandlesAsync<InternalInvestingResponse>(cancellationToken);

        var candles = apiResponse?.Candles?
            .Where(x => x.Time.HasValue && x.Open.HasValue && x.High.HasValue && x.Low.HasValue && x.Close.HasValue)
            .Select(x => new Kline
            {
                Time = x.Time!.Value.UtcDateTime,
                Open = x.Open!.Value,
                High = x.High!.Value,
                Low = x.Low!.Value,
                Close = x.Close!.Value,
                Volume = x.Volume ?? 0m
            })
            .OrderBy(x => x.Time)
            .ToList();

        if (candles is null || candles.Count == 0)
        {
            return new InvestingVnIndexAnalysisResponse
            {
                Success = false,
                Message = "No valid VNINDEX candles were returned by Investing.com."
            };
        }

        var context = new KlineExecutionContext
        {
            MarketType = MarketType.Stock,
            Timeframe = "1d",
            // Do not infer a risk-on regime from the instrument identity. The caller must supply
            // a market-regime assessment before the stock strategy can emit a directional signal.
            MarketRegime = MarketRegime.Neutral,
            EventRisk = EventRisk.Low
        };

        if (candles.Count < _indicatorService.GetMinimumBars(context))
        {
            var latest = candles[^1];
            return new InvestingVnIndexAnalysisResponse
            {
                Success = false,
                Message = InsufficientHistoryMessage,
                HistoryCount = candles.Count,
                LatestTime = new DateTimeOffset(latest.Time, TimeSpan.Zero),
                Open = latest.Open,
                High = latest.High,
                Low = latest.Low,
                Close = latest.Close,
                Volume = latest.Volume
            };
        }

        var indicators = _indicatorService.Compute(context, candles);
        var analysis = _executionEngine.Execute(context, candles);
        var last = candles[^1];

        return new InvestingVnIndexAnalysisResponse
        {
            Success = true,
            Message = "Analysis completed. Signal generation is neutral until a market regime is provided.",
            HistoryCount = candles.Count,
            LatestTime = new DateTimeOffset(last.Time, TimeSpan.Zero),
            Open = last.Open,
            High = last.High,
            Low = last.Low,
            Close = last.Close,
            Volume = last.Volume,
            Ema9 = indicators.Ema9,
            Ema21 = indicators.Ema21,
            Ema50 = indicators.Ema50,
            Rsi14 = indicators.Rsi14,
            Atr14 = indicators.Atr14,
            AverageVolume20 = indicators.AverageVolume20,
            HighestHigh20 = indicators.HighestHigh20,
            LowestLow20 = indicators.LowestLow20,
            Trend = analysis.Trend.ToString(),
            Momentum = analysis.Momentum.ToString(),
            Setup = analysis.Setup.ToString(),
            Signal = analysis.Signal.ToString(),
            Confidence = analysis.Confidence,
            Entry = analysis.Trade?.Entry,
            StopLoss = analysis.Trade?.StopLoss,
            TakeProfit1 = analysis.Trade?.TakeProfit1,
            TakeProfit2 = analysis.Trade?.TakeProfit2
        };
    }

    private sealed record InternalInvestingResponse
    {
        [JsonPropertyName("candles")]
        public List<InternalCandle>? Candles { get; init; }
    }

    private sealed record InternalCandle
    {
        [JsonPropertyName("t")]
        public DateTimeOffset? Time { get; init; }

        [JsonPropertyName("o")]
        public decimal? Open { get; init; }

        [JsonPropertyName("h")]
        public decimal? High { get; init; }

        [JsonPropertyName("l")]
        public decimal? Low { get; init; }

        [JsonPropertyName("c")]
        public decimal? Close { get; init; }

        [JsonPropertyName("v")]
        public decimal? Volume { get; init; }
    }
}
