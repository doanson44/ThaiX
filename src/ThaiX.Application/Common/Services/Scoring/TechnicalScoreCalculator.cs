using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

namespace ThaiX.Application.Common.Services.Scoring;

public sealed class TechnicalScoreCalculator : ITechnicalScoreCalculator
{
    // Weights for long vs short strategy signals
    private const decimal LongStrategyWeight = 0.65m;
    private const decimal ShortStrategyWeight = 0.35m;

    // Max TechnicalScore = 70 so CompositeScore can reach 100 (70 technical + 30 portfolio)
    // Scores scaled proportionally from the 0-50 range to 0-70 range (×1.4)
    private static readonly IReadOnlyDictionary<string, int> SignalScores =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["STRONG BUY"] = 70,
            ["BUY"] = 49,
            ["NEUTRAL"] = 35,
            ["SELL"] = 14,
            ["STRONG SELL"] = 0
        };

    public TechnicalScoreResult Calculate(VnDirectTechnicalSignalDto? longSignal, VnDirectTechnicalSignalDto? shortSignal)
    {
        var longSignalText = NormalizeSignal(longSignal?.TotalSignal);
        var shortSignalText = NormalizeSignal(shortSignal?.TotalSignal);

        var longScore = MapSignal(longSignalText);
        var shortScore = MapSignal(shortSignalText);

        // Weighted composite; max = 70 when both strategies are STRONG BUY
        var compositeScore = Clamp(
            (int)Math.Round((longScore * LongStrategyWeight) + (shortScore * ShortStrategyWeight), MidpointRounding.AwayFromZero),
            0, 70);

        var (longBuyCount, longSellCount) = CountIndicators(longSignal?.Indicators);
        var (shortBuyCount, shortSellCount) = CountIndicators(shortSignal?.Indicators);

        return new TechnicalScoreResult
        {
            LongSignal = longSignalText,
            ShortSignal = shortSignalText,
            LongScore = longScore,
            ShortScore = shortScore,
            CompositeScore = compositeScore,
            LongBuyCount = longBuyCount,
            LongSellCount = longSellCount,
            ShortBuyCount = shortBuyCount,
            ShortSellCount = shortSellCount
        };
    }

    private static (int buyCount, int sellCount) CountIndicators(IEnumerable<VnDirectTechnicalIndicatorDto>? indicators)
    {
        if (indicators is null) return (0, 0);

        var buyCount = 0;
        var sellCount = 0;

        foreach (var indicator in indicators)
        {
            if (string.IsNullOrWhiteSpace(indicator.Signal)) continue;

            var signal = indicator.Signal.Trim();
            if (signal.Equals("BUY", StringComparison.OrdinalIgnoreCase)) buyCount++;
            else if (signal.Equals("SELL", StringComparison.OrdinalIgnoreCase)) sellCount++;
        }

        return (buyCount, sellCount);
    }

    private static string NormalizeSignal(string? signal)
        => string.IsNullOrWhiteSpace(signal) ? "NEUTRAL" : signal.Trim();

    private static int MapSignal(string signal)
        => SignalScores.TryGetValue(signal, out var score) ? score : SignalScores["NEUTRAL"];

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;
}
