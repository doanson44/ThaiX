namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Lifecycle status of a saving deposit position.
/// </summary>
public enum SavingStatus
{
    /// <summary>Deposit is active and has not yet reached maturity.</summary>
    Active = 1,

    /// <summary>Deposit has reached its maturity date and can be withdrawn.</summary>
    Matured = 2,

    /// <summary>Deposit was withdrawn before its maturity date.</summary>
    EarlyWithdrawn = 3,

    /// <summary>Deposit was withdrawn at or after maturity.</summary>
    Withdrawn = 4
}
