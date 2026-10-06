namespace ThaiX.Client.Models.AssetPositions;

// ---- Crypto ----

public sealed class CryptoPositionListItemDto
{
    public Guid Id { get; init; }
    public Guid PortfolioId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal AverageEntryPrice { get; init; }
    public decimal TotalInvested { get; init; }
    public decimal RealizedPnl { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public bool IsClosed { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CryptoPositionsListRequest
{
    public Guid PortfolioId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsClosed { get; set; }
}

public sealed class CreateCryptoPositionRequest
{
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public string? Note { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Fee { get; set; }
    public DateTime TransactedAt { get; set; } = DateTime.UtcNow;
}

public sealed class UpdateCryptoPositionTargetsRequest
{
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public string? Note { get; set; }
}

public sealed class AddCryptoTransactionRequest
{
    public string TransactionType { get; set; } = "Buy";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Fee { get; set; }
    public DateTime TransactedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public string? ExternalRef { get; set; }
}

// ---- Stock ----

public sealed class StockPositionListItemDto
{
    public Guid Id { get; init; }
    public Guid PortfolioId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string Exchange { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal AverageEntryPrice { get; init; }
    public decimal TotalInvested { get; init; }
    public decimal RealizedPnl { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public bool IsClosed { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class StockPositionsListRequest
{
    public Guid PortfolioId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsClosed { get; set; }
}

public sealed class CreateStockPositionRequest
{
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = "HOSE";
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public string? Note { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Fee { get; set; }
    public DateTime TransactedAt { get; set; } = DateTime.UtcNow;
}

public sealed class UpdateStockPositionTargetsRequest
{
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public string? Note { get; set; }
}

public sealed class AddStockTransactionRequest
{
    public string TransactionType { get; set; } = "Buy";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? Fee { get; set; }
    public DateTime TransactedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public string? ExternalRef { get; set; }
}

// ---- Saving ----

public sealed class SavingPositionListItemDto
{
    public Guid Id { get; init; }
    public Guid PortfolioId { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string? AccountNumber { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal InterestRate { get; init; }
    public string InterestType { get; init; } = string.Empty;
    public DateOnly DepositDate { get; init; }
    public DateOnly? MaturityDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class SavingPositionsListRequest
{
    public Guid PortfolioId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
}

public sealed class CreateSavingPositionRequest
{
    public Guid PortfolioId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string InterestType { get; set; } = "Simple";
    public DateOnly DepositDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? MaturityDate { get; set; }
    public string? Note { get; set; }
}

public sealed class UpdateSavingPositionRequest
{
    public string BankName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string InterestType { get; set; } = "Simple";
    public DateOnly DepositDate { get; set; }
    public DateOnly? MaturityDate { get; set; }
    public string? Note { get; set; }
}

public sealed class WithdrawSavingPositionRequest
{
    public DateOnly WithdrawalDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

// ---- Transactions ----

public sealed class PositionTransactionDto
{
    public Guid Id { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal? Fee { get; init; }
    public DateTime TransactedAt { get; init; }
    public string? Note { get; init; }
    public string? ExternalRef { get; init; }
}

public sealed class TransactionsListRequest
{
    public Guid PositionId { get; set; }
    public string AssetType { get; set; } = "Crypto";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
