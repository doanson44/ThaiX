namespace ThaiX.Domain.Aggregates.PriceAlerts;

/// <summary>
/// The type of financial asset being tracked by a price alert.
/// </summary>
public enum AssetType
{
    /// <summary>Cryptocurrency traded on MEXC spot market.</summary>
    CryptoSpot = 1,

    /// <summary>Vietnamese stock listed on HOSE, HNX, or UPCOM.</summary>
    VnStock = 2
}
