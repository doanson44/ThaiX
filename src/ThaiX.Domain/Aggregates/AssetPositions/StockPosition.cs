using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Represents a user's current holding of a listed stock.
/// Maintains a weighted average entry price built from all Buy transactions.
/// </summary>
public sealed class StockPosition : BaseAuditableEntity
{
    private readonly List<PositionTransaction> _transactions = [];

    private StockPosition()
    {
    }

    /// <summary>The portfolio this position belongs to.</summary>
    public Guid PortfolioId { get; private set; }

    public Portfolio Portfolio { get; private set; } = null!;

    /// <summary>Ticker symbol (e.g. VNM, VIC).</summary>
    public string Symbol { get; private set; } = null!;

    /// <summary>The exchange where this stock is listed.</summary>
    public StockExchange Exchange { get; private set; }

    /// <summary>Current total quantity held (in shares / lots).</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Weighted average entry price across all Buy transactions.</summary>
    public decimal AverageEntryPrice { get; private set; }

    /// <summary>Total amount invested including fees.</summary>
    public decimal TotalInvested { get; private set; }

    /// <summary>Accumulated realized PnL from all Sell transactions.</summary>
    public decimal RealizedPnl { get; private set; }

    /// <summary>Optional take-profit target price.</summary>
    public decimal? TargetPrice { get; private set; }

    /// <summary>Optional stop-loss price.</summary>
    public decimal? StopLoss { get; private set; }

    /// <summary>True when Quantity has reached zero (position fully closed).</summary>
    public bool IsClosed { get; private set; }

    /// <summary>User-defined note or memo for this position.</summary>
    public string? Note { get; private set; }

    public IReadOnlyCollection<PositionTransaction> Transactions => _transactions.AsReadOnly();

    public static StockPosition Create(
        Guid portfolioId,
        string symbol,
        StockExchange exchange,
        decimal? targetPrice,
        decimal? stopLoss,
        string? note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        return new StockPosition
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            Symbol = symbol.Trim().ToUpperInvariant(),
            Exchange = exchange,
            Quantity = 0,
            AverageEntryPrice = 0,
            TotalInvested = 0,
            RealizedPnl = 0,
            TargetPrice = targetPrice,
            StopLoss = stopLoss,
            IsClosed = false,
            Note = note?.Trim()
        };
    }

    /// <summary>
    /// Records a Buy transaction and recalculates the weighted average entry price.
    /// </summary>
    public PositionTransaction AddBuy(decimal quantity, decimal price, decimal? fee, DateTime transactedAt, string? note, string? externalRef)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var totalCost = quantity * price + (fee ?? 0);
        AverageEntryPrice = (Quantity * AverageEntryPrice + quantity * price) / (Quantity + quantity);
        Quantity += quantity;
        TotalInvested += totalCost;
        IsClosed = false;

        var tx = PositionTransaction.Create(Id, PositionAssetType.Stock, TransactionType.Buy, quantity, price, fee, transactedAt, note, externalRef);
        _transactions.Add(tx);
        return tx;
    }

    /// <summary>
    /// Records a Sell transaction. Reduces quantity and accumulates realized PnL.
    /// Raises <see cref="PositionClosedEvent"/> when quantity reaches zero.
    /// </summary>
    public PositionTransaction AddSell(decimal quantity, decimal price, decimal? fee, DateTime transactedAt, string? note, string? externalRef)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        if (quantity > Quantity)
            throw new InvalidOperationException($"Cannot sell {quantity} — current quantity is {Quantity}.");

        var grossProceeds = quantity * price - (fee ?? 0);
        var costBasis = quantity * AverageEntryPrice;
        RealizedPnl += grossProceeds - costBasis;
        Quantity -= quantity;

        if (Quantity == 0)
        {
            IsClosed = true;
            AddDomainEvent(new PositionClosedEvent(Id, PositionAssetType.Stock, PortfolioId, Symbol));
        }

        var tx = PositionTransaction.Create(Id, PositionAssetType.Stock, TransactionType.Sell, quantity, price, fee, transactedAt, note, externalRef);
        _transactions.Add(tx);
        return tx;
    }

    public void UpdateTargets(decimal? targetPrice, decimal? stopLoss, string? note)
    {
        TargetPrice = targetPrice;
        StopLoss = stopLoss;
        Note = note?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}
