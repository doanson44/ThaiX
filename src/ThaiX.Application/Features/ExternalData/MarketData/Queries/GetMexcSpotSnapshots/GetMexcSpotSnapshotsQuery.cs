using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotSnapshots;

/// <summary>
/// Query to retrieve a lightweight snapshot of all MEXC spot tickers.
/// Returns only the fields required for price alert evaluation: symbol and last price.
/// </summary>
public sealed record GetMexcSpotSnapshotsQuery : IAppQuery<IReadOnlyList<MexcSpotSnapshotDto>>;

/// <summary>
/// Minimal MEXC spot ticker data needed by the price alert checker.
/// </summary>
public sealed record MexcSpotSnapshotDto
{
    public required string Symbol { get; init; }
    public required decimal LastPrice { get; init; }
    public decimal Volume24h { get; init; }
}
