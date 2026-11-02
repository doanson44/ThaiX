using System.Globalization;
using System.Text.Json;
using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;

public sealed class GetMexcGoldAnalysisQueryHandler(
    IExternalDataService externalDataService,
    IIndicatorService indicatorService,
    IKlineExecutionEngine executionEngine)
    : IRequestHandler<GetMexcGoldAnalysisQuery, MexcGoldAnalysisResponse>
{
    private const string GoldSymbol = "GOLD(XAUT)USDT";
    private const string DailyInterval = "1d";
    private const int HistoryLimit = 500;
    private const string InsufficientHistoryMessage = "Insufficient daily gold candle history for indicator computation.";

    public async Task<MexcGoldAnalysisResponse> Handle(
        GetMexcGoldAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await externalDataService.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
            GoldSymbol,
            DailyInterval,
            HistoryLimit,
            cancellationToken);

        var candles = rows?
            .Where(row => row.Count >= 8)
            .Select(ParseCandle)
            .Where(candle => candle is not null)
            .Select(candle => candle!)
            .GroupBy(candle => candle.OpenTime)
            .Select(group => group.First())
            .OrderBy(candle => candle.OpenTime)
            .TakeLast(HistoryLimit)
            .ToList();

        if (candles is not { Count: > 0 })
        {
            return new MexcGoldAnalysisResponse
            {
                Symbol = GoldSymbol,
                Message = "No valid daily gold candles were returned by MEXC.",
                Success = false
            };
        }

        var history = candles.Select(candle => new Kline
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(candle.OpenTime).UtcDateTime,
            Open = candle.Open,
            High = candle.High,
            Low = candle.Low,
            Close = candle.Close,
            Volume = candle.Volume
        }).ToList();

        var context = new KlineExecutionContext
        {
            MarketType = MarketType.CryptoSpot,
            Timeframe = DailyInterval,
            MarketRegime = MarketRegime.Neutral,
            EventRisk = EventRisk.Low
        };

        var latest = candles[^1];
        var previousClose = candles.Count > 1 ? candles[^2].Close : (decimal?)null;
        var change = previousClose.HasValue ? latest.Close - previousClose.Value : 0m;
        var changePct = previousClose is > 0m ? change / previousClose.Value * 100m : 0m;
        var response = new MexcGoldAnalysisResponse
        {
            Symbol = GoldSymbol,
            Price = latest.Close,
            BopPrice = latest.Open,
            Change = change,
            ChangePct = changePct,
            High = latest.High,
            Low = latest.Low,
            Volume = latest.Volume,
            QuoteVolume = latest.QuoteVolume,
            PreviousClose = previousClose,
            HistoryCount = candles.Count,
            History = candles.Select(candle => new MexcGoldCandle
            {
                Time = DateTimeOffset.FromUnixTimeMilliseconds(candle.OpenTime),
                DateLabel = DateTimeOffset.FromUnixTimeMilliseconds(candle.OpenTime)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume
            }).ToList(),
            LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(latest.OpenTime)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Success = false
        };

        if (candles.Count < indicatorService.GetMinimumBars(context))
        {
            return response with { Message = InsufficientHistoryMessage };
        }

        var indicators = indicatorService.Compute(context, history);
        var analysis = executionEngine.Execute(context, history);

        return response with
        {
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
            TakeProfit2 = analysis.Trade?.TakeProfit2,
            Message = "Analysis completed.",
            Success = true
        };
    }

    private static GoldCandle? ParseCandle(List<JsonElement> row)
    {
        if (!TryReadInt64(row[0], out var openTime) ||
            !TryReadDecimal(row[1], out var open) ||
            !TryReadDecimal(row[2], out var high) ||
            !TryReadDecimal(row[3], out var low) ||
            !TryReadDecimal(row[4], out var close) ||
            !TryReadDecimal(row[5], out var volume) ||
            !TryReadInt64(row[6], out var closeTime) ||
            !TryReadDecimal(row[7], out var quoteVolume) ||
            high < low ||
            high < open ||
            high < close ||
            low > open ||
            low > close)
        {
            return null;
        }

        return new GoldCandle(openTime, open, high, low, close, volume, closeTime, quoteVolume);
    }

    private static bool TryReadInt64(JsonElement element, out long value)
    {
        if (element.ValueKind == JsonValueKind.Number)
            return element.TryGetInt64(out value);

        return long.TryParse(element.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadDecimal(JsonElement element, out decimal value) =>
        decimal.TryParse(element.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private sealed record GoldCandle(
        long OpenTime,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        decimal Volume,
        long CloseTime,
        decimal QuoteVolume);
}
