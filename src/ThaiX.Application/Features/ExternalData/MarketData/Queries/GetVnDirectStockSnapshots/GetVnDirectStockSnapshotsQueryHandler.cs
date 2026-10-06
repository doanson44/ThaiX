using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockSnapshots;

/// <summary>
/// Handler for GetVnDirectStockSnapshotsQuery.
/// Fetches all stock prices from VnDirect in one batch call (sorted date DESC, size 9999).
/// Groups records by code and takes the latest date per symbol.
/// Returns an empty list on API failure.
/// </summary>
public sealed class GetVnDirectStockSnapshotsQueryHandler
    : IRequestHandler<GetVnDirectStockSnapshotsQuery, IReadOnlyList<VnDirectStockSnapshotDto>>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectStockSnapshotsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<IReadOnlyList<VnDirectStockSnapshotDto>> Handle(
        GetVnDirectStockSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _externalDataService
            .GetVnDirectStockPricesAllAsync<InternalVnDirectPricesResponse>(cancellationToken);

        if (response?.Data is null || response.Data.Count == 0)
            return [];

        // Sorted DESC by date — first occurrence of each code is already the most recent.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var snapshots = new List<VnDirectStockSnapshotDto>();

        foreach (var record in response.Data)
        {
            if (string.IsNullOrWhiteSpace(record.Code) || record.Close <= 0)
                continue;

            if (!seen.Add(record.Code))
                continue;

            if (!DateOnly.TryParseExact(record.Date, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var priceDate))
                continue;

            snapshots.Add(new VnDirectStockSnapshotDto
            {
                Symbol = record.Code.Trim().ToUpperInvariant(),
                LastPrice = record.Close,
                PriceDate = priceDate
            });
        }

        return snapshots;
    }

    private sealed record InternalVnDirectPricesResponse
    {
        public List<InternalStockPriceRecord>? Data { get; init; }
    }

    private sealed record InternalStockPriceRecord
    {
        public string? Code { get; init; }
        public string Date { get; init; } = string.Empty;
        public decimal Close { get; init; }
    }
}
