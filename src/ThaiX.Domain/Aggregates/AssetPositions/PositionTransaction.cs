using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// An immutable record of a single trade or event against an asset position.
/// Transactions are the source of truth; positions are materialized state.
/// </summary>
public sealed class PositionTransaction : BaseEntity
{
    private PositionTransaction()
    {
    }

    /// <summary>The position this transaction belongs to.</summary>
    public Guid PositionId { get; private set; }

    /// <summary>The asset type discriminator so queries can filter without joining.</summary>
    public PositionAssetType AssetType { get; private set; }

    /// <summary>Type of transaction (Buy/Sell/Fee/Dividend ...).</summary>
    public TransactionType TransactionType { get; private set; }

    /// <summary>Quantity of asset involved in this transaction.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Price per unit at transaction time. Zero for dividends/fees expressed as cash.</summary>
    public decimal Price { get; private set; }

    /// <summary>Optional fee paid for this transaction (e.g. broker commission).</summary>
    public decimal? Fee { get; private set; }

    /// <summary>
    /// User-defined date/time of the transaction. May differ from CreatedAt (e.g. importing history).
    /// </summary>
    public DateTime TransactedAt { get; private set; }

    /// <summary>Optional note or memo.</summary>
    public string? Note { get; private set; }

    /// <summary>External reference (broker ref, exchange transaction id) for deduplication.</summary>
    public string? ExternalRef { get; private set; }

    internal static PositionTransaction Create(
        Guid positionId,
        PositionAssetType assetType,
        TransactionType transactionType,
        decimal quantity,
        decimal price,
        decimal? fee,
        DateTime transactedAt,
        string? note,
        string? externalRef)
    {
        return new PositionTransaction
        {
            Id = Guid.NewGuid(),
            PositionId = positionId,
            AssetType = assetType,
            TransactionType = transactionType,
            Quantity = quantity,
            Price = price,
            Fee = fee,
            TransactedAt = transactedAt,
            Note = note?.Trim(),
            ExternalRef = externalRef?.Trim()
        };
    }
}
