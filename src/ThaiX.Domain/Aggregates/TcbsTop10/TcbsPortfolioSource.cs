namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>Which source produced a TCBS portfolio update row.</summary>
public enum TcbsPortfolioSource
{
    /// <summary>iWealth Club stream (monthly Analyst Top 10 posts).</summary>
    IWealthClub = 0,

    /// <summary>TCBS iStock potential stock list page.</summary>
    TcbsPotentialStocks = 1
}
