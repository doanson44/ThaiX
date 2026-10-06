namespace ThaiX.Infrastructure.Configuration;

public sealed class MarketScannerOptions
{
    public const string SectionName = "MarketScanner";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// How many minutes of snapshots to retain in memory.
    /// Should be longer than the longest configured rule window.
    /// </summary>
    public int MaxSnapshotRetentionMinutes { get; set; } = 75;

    /// <summary>
    /// Minimum minutes before the same signal is fired again for the same symbol.
    /// </summary>
    public int CooldownMinutes { get; set; } = 15;
}
