namespace ThaiX.Domain.Aggregates.Portfolios;

/// <summary>
/// Categorizes the investment strategy of a portfolio.
/// </summary>
public enum PortfolioType
{
    /// <summary>Short-term active trading.</summary>
    Trading = 1,

    /// <summary>Long-term buy-and-hold investments.</summary>
    LongTerm = 2,

    /// <summary>Retirement savings and investments.</summary>
    Retirement = 3,

    /// <summary>Cash savings and deposit accounts.</summary>
    Savings = 4
}
