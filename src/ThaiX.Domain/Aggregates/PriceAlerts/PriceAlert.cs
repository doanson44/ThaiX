using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.PriceAlerts;

/// <summary>
/// Represents a user-defined price alert for a financial asset.
/// The alert triggers a Slack notification when the condition is met.
/// </summary>
public sealed class PriceAlert : BaseAuditableEntity
{
    private PriceAlert()
    {
    }

    /// <summary>Ticker symbol (e.g. BTCUSDT for crypto, VNM for stock).</summary>
    public string Symbol { get; private set; } = null!;

    /// <summary>The type of asset being tracked.</summary>
    public AssetType AssetType { get; private set; }

    /// <summary>The condition that triggers the alert.</summary>
    public AlertCondition Condition { get; private set; }

    /// <summary>The price level that triggers the alert.</summary>
    public decimal TargetPrice { get; private set; }

    /// <summary>Optional note displayed in the notification message.</summary>
    public string? Note { get; private set; }

    /// <summary>Whether this alert is actively checked by the background job.</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Whether this alert fires once and then auto-disables.
    /// When false, the alert fires repeatedly every time the condition is met.
    /// </summary>
    public bool IsOneTime { get; private set; }

    /// <summary>When the alert was last triggered. Null if never triggered.</summary>
    public DateTime? LastTriggeredAt { get; private set; }

    /// <summary>Total number of times this alert has been triggered.</summary>
    public int TriggerCount { get; private set; }

    public static PriceAlert Create(
        string symbol,
        AssetType assetType,
        AlertCondition condition,
        decimal targetPrice,
        string? note,
        bool isOneTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetPrice);

        return new PriceAlert
        {
            Id = Guid.NewGuid(),
            Symbol = symbol.Trim().ToUpperInvariant(),
            AssetType = assetType,
            Condition = condition,
            TargetPrice = targetPrice,
            Note = note?.Trim(),
            IsEnabled = true,
            IsOneTime = isOneTime,
            TriggerCount = 0
        };
    }

    public void Update(
        AlertCondition condition,
        decimal targetPrice,
        string? note,
        bool isOneTime,
        bool isEnabled)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetPrice);

        Condition = condition;
        TargetPrice = targetPrice;
        Note = note?.Trim();
        IsOneTime = isOneTime;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Records a trigger event. Disables the alert if it is one-time.
    /// </summary>
    public void RecordTrigger(DateTime triggeredAt)
    {
        LastTriggeredAt = triggeredAt;
        TriggerCount++;

        if (IsOneTime)
            IsEnabled = false;
    }

    /// <summary>
    /// Disables the alert without recording a trigger (e.g., when the linked position is closed).
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
