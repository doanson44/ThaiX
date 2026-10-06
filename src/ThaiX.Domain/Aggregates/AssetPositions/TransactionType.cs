namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// The type of transaction recorded against a position.
/// </summary>
public enum TransactionType
{
    /// <summary>Purchase of an asset.</summary>
    Buy = 1,

    /// <summary>Sale of an asset (partial or full).</summary>
    Sell = 2,

    /// <summary>Trading or exchange fee (reduces quantity or cash).</summary>
    Fee = 3,

    /// <summary>Dividend income received from a stock holding.</summary>
    Dividend = 4,

    /// <summary>Staking or yield reward received from a crypto holding.</summary>
    StakingReward = 5,

    /// <summary>Transfer of asset in from another source (no cost basis change).</summary>
    TransferIn = 6,

    /// <summary>Transfer of asset out to another destination.</summary>
    TransferOut = 7
}
