namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Discriminates between asset position types for cross-portfolio queries.
/// </summary>
public enum PositionAssetType
{
    /// <summary>Cryptocurrency traded on a spot market.</summary>
    Crypto = 1,

    /// <summary>Vietnamese or foreign listed stock.</summary>
    Stock = 2,

    /// <summary>Bank saving / deposit account.</summary>
    Saving = 3
}
