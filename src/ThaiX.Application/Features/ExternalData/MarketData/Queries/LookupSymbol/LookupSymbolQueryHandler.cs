using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotSnapshots;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockSnapshots;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.LookupSymbol;

/// <summary>
/// Handler for LookupSymbolQuery.
/// Searches VnDirect snapshots (VnStock) then MEXC spot snapshots (CryptoSpot).
/// Snapshot lists are cached per asset type for 24 hours via ICacheService.
/// </summary>
public sealed class LookupSymbolQueryHandler : IRequestHandler<LookupSymbolQuery, SymbolLookupDto?>
{
    private readonly ISender _sender;
    private readonly ICacheService _cache;

    public LookupSymbolQueryHandler(ISender sender, ICacheService cache)
    {
        _sender = sender;
        _cache = cache;
    }

    public async Task<SymbolLookupDto?> Handle(LookupSymbolQuery request, CancellationToken cancellationToken)
    {
        var normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();

        // --- VnStock ---
        var vnCacheKey = CacheKeys.ExternalData.SymbolLookup("snapshots:vnstock");
        var vnSnapshots = await _cache.GetAsync<IReadOnlyList<VnDirectStockSnapshotDto>>(vnCacheKey, cancellationToken);
        if (vnSnapshots is null)
        {
            vnSnapshots = await _sender.Send(new GetVnDirectStockSnapshotsQuery(), cancellationToken);
            await _cache.SetAsync(vnCacheKey, vnSnapshots, TimeSpan.FromDays(1), CacheGroups.ExternalData, cancellationToken);
        }

        var vnMatch = vnSnapshots.FirstOrDefault(
            s => string.Equals(s.Symbol, normalizedSymbol, StringComparison.OrdinalIgnoreCase));

        if (vnMatch is not null)
            return new SymbolLookupDto(vnMatch.Symbol, vnMatch.Symbol, nameof(AssetType.VnStock));

        // --- CryptoSpot ---
        var cryptoCacheKey = CacheKeys.ExternalData.SymbolLookup("snapshots:cryptospot");
        var cryptoSnapshots = await _cache.GetAsync<IReadOnlyList<MexcSpotSnapshotDto>>(cryptoCacheKey, cancellationToken);
        if (cryptoSnapshots is null)
        {
            cryptoSnapshots = await _sender.Send(new GetMexcSpotSnapshotsQuery(), cancellationToken);
            await _cache.SetAsync(cryptoCacheKey, cryptoSnapshots, TimeSpan.FromDays(1), CacheGroups.ExternalData, cancellationToken);
        }

        // Try exact match first (e.g. "BTCUSDT"), then fall back to {symbol}USDT (e.g. "BTC" -> "BTCUSDT").
        var cryptoMatch = cryptoSnapshots.FirstOrDefault(
            s => string.Equals(s.Symbol, normalizedSymbol, StringComparison.OrdinalIgnoreCase));

        if (cryptoMatch is null && !normalizedSymbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
        {
            var withUsdt = normalizedSymbol + "USDT";
            cryptoMatch = cryptoSnapshots.FirstOrDefault(
                s => string.Equals(s.Symbol, withUsdt, StringComparison.OrdinalIgnoreCase));
        }

        if (cryptoMatch is not null)
            return new SymbolLookupDto(cryptoMatch.Symbol, cryptoMatch.Symbol, nameof(AssetType.CryptoSpot));

        return null;
    }
}
