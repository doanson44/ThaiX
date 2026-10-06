using System.Globalization;

namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>
/// Parses raw string values returned by the ChainBroker API into typed values.
/// All monetary values are stored as raw USD (e.g. 6_650_000 for "$6.65M").
/// All percentage values are stored as plain decimals (e.g. -2.13 for "-2.13%").
/// </summary>
internal static class ChainBrokerValueParser
{
    /// <summary>Parses "$6.65M", "$882K", "$1.15B" to raw USD decimal.</summary>
    internal static decimal? ParseUsd(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return ParseWithSuffix(raw.TrimStart('$'));
    }

    /// <summary>Parses "126M", "1B", "190M" token counts (no $ prefix).</summary>
    internal static decimal? ParseCount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return ParseWithSuffix(raw.Trim());
    }

    /// <summary>Parses "+1.58%", "-2.13%", "20.7%" to plain decimal (e.g. 1.58, -2.13).</summary>
    internal static decimal? ParsePercent(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim().TrimEnd('%');
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val
            : null;
    }

    /// <summary>Parses "3.28x", "0.881x", "16.2x" to plain decimal (e.g. 3.28).</summary>
    internal static decimal? ParseRoi(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim().TrimEnd('x', 'X');
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val
            : null;
    }

    /// <summary>Parses a plain decimal string like "7.51" or "77.91".</summary>
    internal static decimal? ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return decimal.TryParse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val
            : null;
    }

    /// <summary>
    /// Parses "2026-05-13" (ISO 8601) or "Apr 24, 2026" (ChainBroker unlock format) to DateOnly.
    /// </summary>
    internal static DateOnly? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();

        if (DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var iso))
            return iso;

        if (DateOnly.TryParseExact(s, "MMM d, yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var unlockFmt))
            return unlockFmt;

        return null;
    }

    private static decimal? ParseWithSuffix(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();

        decimal multiplier = 1m;
        if (s.EndsWith("B", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000_000_000m;
            s = s[..^1];
        }
        else if (s.EndsWith("M", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000_000m;
            s = s[..^1];
        }
        else if (s.EndsWith("K", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000m;
            s = s[..^1];
        }

        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val * multiplier
            : null;
    }
}
