namespace ThaiX.Domain.Aggregates.PriceAlerts;

/// <summary>
/// The condition that triggers a price alert.
/// </summary>
public enum AlertCondition
{
    /// <summary>Triggers when price rises above the target price.</summary>
    Above = 1,

    /// <summary>Triggers when price falls below the target price.</summary>
    Below = 2
}
