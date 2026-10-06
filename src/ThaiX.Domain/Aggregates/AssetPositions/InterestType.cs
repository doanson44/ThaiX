namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Interest compounding type for a saving deposit.
/// </summary>
public enum InterestType
{
    /// <summary>Simple interest calculated on principal only.</summary>
    Simple = 1,

    /// <summary>Compound interest calculated on principal plus accumulated interest.</summary>
    Compound = 2
}
