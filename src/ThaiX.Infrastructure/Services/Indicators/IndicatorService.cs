using Skender.Stock.Indicators;
using ThaiX.Application.Trading;

namespace ThaiX.Infrastructure.Services.Indicators;

public sealed class IndicatorService : IIndicatorService
{
    public int GetMinimumBars(KlineExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ResolveProfile(context).MinimumBars;
    }

    public IndicatorResult Compute(KlineExecutionContext context, IReadOnlyList<Kline> klines)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(klines);

        var profile = ResolveProfile(context);
        if (klines.Count < profile.MinimumBars)
        {
            throw new ArgumentException($"At least {profile.MinimumBars} klines are required.", nameof(klines));
        }

        for (var i = 1; i < klines.Count; i++)
        {
            if (klines[i - 1].Time > klines[i].Time)
            {
                throw new ArgumentException("Klines must be sorted ascending by Time.", nameof(klines));
            }
        }

        var quotes = new List<Quote>(klines.Count);
        for (var i = 0; i < klines.Count; i++)
        {
            var k = klines[i];
            quotes.Add(new Quote
            {
                Date = k.Time,
                Open = k.Open,
                High = k.High,
                Low = k.Low,
                Close = k.Close,
                Volume = k.Volume
            });
        }

        var ema9 = SkipWarmupPeriods(quotes.GetEma(profile.EmaFastPeriod), x => x.Ema).LastOrDefault();
        var ema21 = SkipWarmupPeriods(quotes.GetEma(profile.EmaMidPeriod), x => x.Ema).LastOrDefault();
        var ema50 = SkipWarmupPeriods(quotes.GetEma(profile.EmaSlowPeriod), x => x.Ema).LastOrDefault();
        var rsi14 = SkipWarmupPeriods(quotes.GetRsi(profile.RsiPeriod), x => x.Rsi).LastOrDefault();
        var atr14 = SkipWarmupPeriods(quotes.GetAtr(profile.AtrPeriod), x => x.Atr).LastOrDefault();

        if (ema9 is null || ema21 is null || ema50 is null || rsi14 is null || atr14 is null)
        {
            throw new ArgumentException("Indicator computation returned null values after warmup.", nameof(klines));
        }

        var start = klines.Count - profile.VolumeAveragePeriod;
        decimal volumeSum = 0m;
        for (var i = start; i < klines.Count; i++)
        {
            volumeSum += klines[i].Volume;
        }

        decimal highestHigh20 = decimal.MinValue;
        decimal lowestLow20 = decimal.MaxValue;
        // Previous N bars, excluding the current bar (klines.Count - 1)
        start = klines.Count - (profile.BreakoutLookbackPeriod + 1);
        for (var i = start; i < klines.Count - 1; i++)
        {
            if (klines[i].High > highestHigh20) highestHigh20 = klines[i].High;
            if (klines[i].Low < lowestLow20) lowestLow20 = klines[i].Low;
        }

        return new IndicatorResult
        {
            Ema9 = ToDecimal(ema9.Value),
            Ema21 = ToDecimal(ema21.Value),
            Ema50 = ToDecimal(ema50.Value),
            Rsi14 = ToDecimal(rsi14.Value),
            Atr14 = ToDecimal(atr14.Value),
            AverageVolume20 = volumeSum / profile.VolumeAveragePeriod,
            LatestClose = klines[^1].Close,
            HighestHigh20 = highestHigh20,
            LowestLow20 = lowestLow20
        };
    }

    private static IndicatorProfile ResolveProfile(KlineExecutionContext context)
    {
        if (context.MarketType is MarketType.Crypto or MarketType.CryptoSpot)
        {
            var tf = context.Timeframe.Trim().ToUpperInvariant();
            return tf switch
            {
                "WEEK1" => new IndicatorProfile(26, 6, 13, 26, 14, 14, 20, 20),
                "MONTH1" => new IndicatorProfile(12, 3, 6, 12, 7, 7, 10, 10),
                _ => IndicatorProfile.Default
            };
        }

        return IndicatorProfile.Default;
    }

    private static IEnumerable<double?> SkipWarmupPeriods<T>(IEnumerable<T> source, Func<T, double?> selector)
    {
        var warm = false;
        foreach (var item in source)
        {
            var value = selector(item);
            if (!warm)
            {
                if (value is null)
                {
                    continue;
                }

                warm = true;
            }

            yield return value;
        }
    }

    private static decimal ToDecimal(double value) => Convert.ToDecimal(value);

    private sealed record IndicatorProfile(
        int MinimumBars,
        int EmaFastPeriod,
        int EmaMidPeriod,
        int EmaSlowPeriod,
        int RsiPeriod,
        int AtrPeriod,
        int VolumeAveragePeriod,
        int BreakoutLookbackPeriod)
    {
        private const int DefaultMinimumBars = 60;

        public static IndicatorProfile Default { get; } =
            new(DefaultMinimumBars, 9, 21, 50, 14, 14, 20, 20);
    }
}
