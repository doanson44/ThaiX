using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotSnapshots;

/// <summary>
/// Handler for GetMexcSpotSnapshotsQuery.
/// Fetches all MEXC spot 24hr tickers and projects the fields needed by the price alert engine.
/// No DB access. Returns empty list on API failure.
/// </summary>
public sealed class GetMexcSpotSnapshotsQueryHandler
    : IRequestHandler<GetMexcSpotSnapshotsQuery, IReadOnlyList<MexcSpotSnapshotDto>>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcSpotSnapshotsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<IReadOnlyList<MexcSpotSnapshotDto>> Handle(
        GetMexcSpotSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var tickers = await _externalDataService
            .GetMexcSpotTicker24HrAsync<List<InternalMexcSpotTicker>>(cancellationToken);

        if (tickers is null)
            return [];

        return tickers
            .Where(t => !string.IsNullOrWhiteSpace(t.Symbol)
                && t.Symbol!.EndsWith("USDT", StringComparison.OrdinalIgnoreCase)
                && t.LastPrice.HasValue && t.LastPrice.Value > 0)
            .Select(t => new MexcSpotSnapshotDto
            {
                Symbol = t.Symbol!,
                LastPrice = t.LastPrice!.Value,
                Volume24h = t.Volume ?? 0m
            })
            .ToList();
    }

    private sealed record InternalMexcSpotTicker
    {
        public string? Symbol { get; init; }
        public decimal? LastPrice { get; init; }
        public decimal? Volume { get; init; }
    }
}
