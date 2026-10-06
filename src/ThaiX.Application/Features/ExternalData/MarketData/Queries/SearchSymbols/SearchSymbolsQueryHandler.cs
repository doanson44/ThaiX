using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotSnapshots;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockSnapshots;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.SearchSymbols;

public sealed class SearchSymbolsQueryHandler : IRequestHandler<SearchSymbolsQuery, SymbolSearchResultDto>
{
    private readonly ISender _sender;
    private readonly ICacheService _cacheService;

    public SearchSymbolsQueryHandler(ISender sender, ICacheService cacheService)
    {
        _sender = sender;
        _cacheService = cacheService;
    }

    public async Task<SymbolSearchResultDto> Handle(SearchSymbolsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var isVnStock = string.Equals(request.AssetType, nameof(AssetType.VnStock), StringComparison.OrdinalIgnoreCase);

        var cacheKey = CacheKeys.ExternalData.SymbolSearchFullList(request.AssetType);
        var allSymbols = await _cacheService.GetAsync<IReadOnlyList<SymbolSearchItemDto>>(cacheKey, cancellationToken);

        if (allSymbols is null)
        {
            var fetched = new List<SymbolSearchItemDto>();

            if (isVnStock)
            {
                var snapshots = await _sender.Send(new GetVnDirectStockSnapshotsQuery(), cancellationToken);
                fetched.AddRange(snapshots.Select(s => new SymbolSearchItemDto(
                    s.Symbol,
                    $"{s.Symbol} \u2014 {s.LastPrice:N0}")));
            }
            else
            {
                var snapshots = await _sender.Send(new GetMexcSpotSnapshotsQuery(), cancellationToken);
                fetched.AddRange(snapshots.Select(s =>
                {
                    var sym = s.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase)
                        ? s.Symbol[..^4].ToUpperInvariant()
                        : s.Symbol.ToUpperInvariant();
                    return new SymbolSearchItemDto(s.Symbol, $"{sym} \u2014 ${s.LastPrice.ToString("0.####", CultureInfo.InvariantCulture)}");
                }));
            }

            allSymbols = fetched.OrderBy(s => s.Symbol).ToList();
            await _cacheService.SetAsync(cacheKey, allSymbols, TimeSpan.FromDays(1), CacheGroups.ExternalData, cancellationToken);
        }

        IEnumerable<SymbolSearchItemDto> filtered = allSymbols;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            filtered = filtered.Where(s => s.Symbol.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var resultList = filtered.ToList();
        var totalCount = resultList.Count;
        var pagedItems = resultList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new SymbolSearchResultDto(pagedItems, totalCount);
    }
}
