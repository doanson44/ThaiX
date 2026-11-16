namespace ThaiX.Application.Strategies;

/// <summary>
/// Roulette wheel selection: weighted sampling without replacement.
/// Each candidate occupies a wheel slice proportional to its weight, so its chance
/// of being selected on a spin equals weight / remainingTotal. After a candidate
/// wins, it is removed and the wheel is re-normalized over the remaining candidates.
/// </summary>
public static class RouletteWheelSelector
{
    /// <summary>
    /// Selects <paramref name="count"/> distinct numbers using roulette wheel selection
    /// without replacement. Numbers with a weight greater than zero form the wheel;
    /// zero-weight numbers are never selected while any positive-weight number remains.
    /// If the positive-weight pool runs out, the remaining picks fall back to the lowest
    /// available numbers (deterministic ascending fill).
    /// </summary>
    /// <param name="weights">Number to weight map. Weights must be non-negative.</param>
    /// <param name="count">How many distinct numbers to select (1-55).</param>
    /// <param name="random">Random source. Pass a seeded instance for determinism.</param>
    public static IReadOnlyList<int> SelectWithoutReplacement(
        IReadOnlyDictionary<int, int> weights,
        int count,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(weights);
        ArgumentNullException.ThrowIfNull(random);
        if (count is < 1 or > 55)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be between 1 and 55.");
        }

        var positive = weights
            .Where(kv => kv.Value > 0)
            .Select(kv => kv.Key)
            .OrderBy(n => n)
            .ToList();

        var zeroPool = Enumerable.Range(1, 55)
            .Where(n => !weights.ContainsKey(n) || weights[n] <= 0)
            .OrderBy(n => n)
            .ToList();

        var selected = new List<int>(count);

        while (selected.Count < count)
        {
            if (positive.Count == 0)
            {
                // No signal left: fill from the lowest available number (deterministic).
                selected.Add(zeroPool[0]);
                zeroPool.RemoveAt(0);
                continue;
            }

            var total = positive.Sum(n => weights[n]);
            var target = random.Next(0, total);

            var cumulative = 0;
            var chosenIndex = 0;
            for (var i = 0; i < positive.Count; i++)
            {
                cumulative += weights[positive[i]];
                if (target < cumulative)
                {
                    chosenIndex = i;
                    break;
                }
            }

            selected.Add(positive[chosenIndex]);
            positive.RemoveAt(chosenIndex);
        }

        return selected;
    }
}
