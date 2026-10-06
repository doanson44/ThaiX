using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractSnapshots;

/// <summary>
/// Handler for GetMexcContractSnapshotsQuery.
/// Fetches raw MEXC contract tickers and projects only the fields needed by the market scanner.
/// No DB access, no composite scoring.
/// </summary>
public sealed class GetMexcContractSnapshotsQueryHandler
    : IRequestHandler<GetMexcContractSnapshotsQuery, IReadOnlyList<MexcContractSnapshotDto>>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcContractSnapshotsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<IReadOnlyList<MexcContractSnapshotDto>> Handle(
        GetMexcContractSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _externalDataService
            .GetMexcContractTickerAsync<InternalMexcApiResponse>(request.BypassCache, cancellationToken);

        if (response is null || !response.Success || response.Code != 0 || response.Data is null)
            return [];

        return response.Data
            .Where(t => !string.IsNullOrWhiteSpace(t.Symbol)
                        && t.LastPrice.HasValue
                        && t.Symbol!.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .Select(t => new MexcContractSnapshotDto
            {
                Symbol = t.Symbol!,
                LastPrice = t.LastPrice!.Value,
                FundingRate = t.FundingRate ?? 0m,
                Volume24h = t.Volume24 ?? 0m
            })
            .ToList();
    }

    private sealed record InternalMexcApiResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public List<InternalMexcTicker>? Data { get; init; }
    }

    private sealed record InternalMexcTicker
    {
        public string? Symbol { get; init; }
        public decimal? LastPrice { get; init; }
        public decimal? FundingRate { get; init; }
        public decimal? Volume24 { get; init; }
    }
}
