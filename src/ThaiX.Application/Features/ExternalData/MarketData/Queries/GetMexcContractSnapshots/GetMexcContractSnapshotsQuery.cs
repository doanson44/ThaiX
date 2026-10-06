using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractSnapshots;

/// <summary>
/// Query to retrieve a lightweight snapshot of all MEXC contract tickers.
/// Returns only the fields required for market signal detection: price, funding rate, and volume.
/// Does not perform DB enrichment or composite scoring.
/// </summary>
public sealed record GetMexcContractSnapshotsQuery : IAppQuery<IReadOnlyList<MexcContractSnapshotDto>>
{
    public bool BypassCache { get; init; }
}

/// <summary>
/// Minimal MEXC contract ticker data needed by the market scanner engine.
/// </summary>
public sealed record MexcContractSnapshotDto
{
    public required string Symbol { get; init; }
    public required decimal LastPrice { get; init; }
    public decimal FundingRate { get; init; }
    public decimal Volume24h { get; init; }
}
