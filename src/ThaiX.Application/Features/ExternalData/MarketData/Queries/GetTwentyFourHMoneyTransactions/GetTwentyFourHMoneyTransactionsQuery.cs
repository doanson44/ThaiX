using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactions;

/// <summary>
/// Query to retrieve 24HMoney SSI transaction list by stock symbol.
/// </summary>
public sealed record GetTwentyFourHMoneyTransactionsQuery : IAppQuery<TwentyFourHMoneyTransactionsResponse>
{
    public string Symbol { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PerPage { get; init; } = 1000;
}

/// <summary>
/// Response containing 24HMoney SSI transactions.
/// </summary>
public sealed record TwentyFourHMoneyTransactionsResponse
{
    public string Symbol { get; init; } = string.Empty;
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int Status { get; init; }
    public int ExecuteTimeMs { get; init; }
    public List<TwentyFourHMoneyTransactionDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// 24HMoney transaction item.
/// </summary>
public sealed record TwentyFourHMoneyTransactionDto
{
    public decimal Price { get; init; }
    public decimal Change { get; init; }
    public long MatchQuantity { get; init; }
    public long TotalVolume { get; init; }
    public string Time { get; init; } = string.Empty;
    public string Side { get; init; } = string.Empty;
}
