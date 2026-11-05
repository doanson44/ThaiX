namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Selects which TCBS content source a provider reads.
/// Bound from configuration: ExternalApis:Providers:{Provider}:Source.
/// </summary>
public enum TcbsSyncSource
{
    /// <summary>iWealth Club stream (monthly Analyst Top 10 posts).</summary>
    Stream = 0,

    /// <summary>TCBS iStock potential stock list page.</summary>
    PotentialStocks = 1
}
