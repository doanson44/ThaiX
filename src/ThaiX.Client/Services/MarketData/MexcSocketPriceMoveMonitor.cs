using System.Globalization;
using ThaiX.Client.Models.MarketData;

namespace ThaiX.Client.Services.MarketData;

/// <summary>
/// Client-only price-move detector for MEXC socket pages.
/// Compares each symbol's current price to the oldest sample inside the configured time window.
/// </summary>
public sealed class MexcSocketPriceMoveMonitor
{
    private readonly Dictionary<string, LinkedList<(DateTimeOffset At, decimal Price)>> _history = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTimeOffset> _cooldownUntil = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public MexcSocketPriceMoveAlertSettings Settings { get; private set; } = new();
    public bool IsEnabled { get; private set; }

    public void Configure(MexcSocketPriceMoveAlertSettings settings, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_gate)
        {
            Settings = settings.Clone();
            IsEnabled = enabled;
            if (!enabled)
            {
                _history.Clear();
                _cooldownUntil.Clear();
            }
        }
    }

    public void Disable() => Configure(Settings, enabled: false);

    public IReadOnlyList<MexcSocketPriceMoveAlert> Observe(string symbol, decimal price, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(symbol) || price <= 0m)
        {
            return Array.Empty<MexcSocketPriceMoveAlert>();
        }

        var at = now ?? DateTimeOffset.UtcNow;

        lock (_gate)
        {
            if (!IsEnabled || Settings.TimeWindowMinutes <= 0 || Settings.AbsoluteChangePercent <= 0m)
            {
                return Array.Empty<MexcSocketPriceMoveAlert>();
            }

            var key = symbol.Trim().ToUpperInvariant();
            if (_cooldownUntil.TryGetValue(key, out var until) && at < until)
            {
                return Array.Empty<MexcSocketPriceMoveAlert>();
            }

            if (!_history.TryGetValue(key, out var samples))
            {
                samples = new LinkedList<(DateTimeOffset At, decimal Price)>();
                _history[key] = samples;
            }

            var window = TimeSpan.FromMinutes(Settings.TimeWindowMinutes);
            var cutoff = at - window;
            while (samples.First is not null && samples.First.Value.At < cutoff)
            {
                samples.RemoveFirst();
            }

            samples.AddLast((at, price));

            if (samples.Count < 2)
            {
                return Array.Empty<MexcSocketPriceMoveAlert>();
            }

            var reference = samples.First!.Value.Price;
            if (reference <= 0m)
            {
                return Array.Empty<MexcSocketPriceMoveAlert>();
            }

            var changePercent = (price - reference) / reference * 100m;
            if (Math.Abs(changePercent) < Settings.AbsoluteChangePercent)
            {
                return Array.Empty<MexcSocketPriceMoveAlert>();
            }

            // Reset baseline after alert and silence this symbol for one window.
            samples.Clear();
            samples.AddLast((at, price));
            _cooldownUntil[key] = at + window;

            return
            [
                new MexcSocketPriceMoveAlert(
                    key,
                    reference,
                    price,
                    Math.Round(changePercent, 2),
                    Settings.TimeWindowMinutes)
            ];
        }
    }

    public static bool TryParsePrice(string? raw, out decimal price)
    {
        price = 0m;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out price) && price > 0m;
    }
}
