using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockSnapshots;

/// <summary>
/// Query to retrieve the latest close price for every listed Vietnamese stock in a single API call.
/// Fetches the most recent price records from VnDirect (no code filter) and returns one snapshot
/// per symbol (the most recent trading day). Returns an empty list on failure.
/// </summary>
public sealed record GetVnDirectStockSnapshotsQuery : IAppQuery<IReadOnlyList<VnDirectStockSnapshotDto>>;

/// <summary>
/// Latest price snapshot for a Vietnamese stock symbol.
/// </summary>
public sealed record VnDirectStockSnapshotDto
{
    public required string Symbol { get; init; }
    public required decimal LastPrice { get; init; }
    public required DateOnly PriceDate { get; init; }
}
