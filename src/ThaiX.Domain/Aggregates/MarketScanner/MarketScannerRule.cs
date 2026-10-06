using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.MarketScanner;

public sealed class MarketScannerRule : BaseAuditableEntity
{
    private MarketScannerRule()
    {
    }

    public string Name { get; private set; } = null!;

    public MarketSignalType SignalType { get; private set; }

    public MarketTimeWindow Window { get; private set; }

    public decimal? Threshold { get; private set; }

    public bool IsEnabled { get; private set; }

    public static MarketScannerRule Create(
        string name,
        MarketSignalType signalType,
        MarketTimeWindow window,
        decimal? threshold)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new MarketScannerRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            SignalType = signalType,
            Window = window,
            Threshold = threshold,
            IsEnabled = true
        };
    }

    public void Update(
        string name,
        MarketTimeWindow window,
        decimal? threshold,
        bool isEnabled)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Window = window;
        Threshold = threshold;
        IsEnabled = isEnabled;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
