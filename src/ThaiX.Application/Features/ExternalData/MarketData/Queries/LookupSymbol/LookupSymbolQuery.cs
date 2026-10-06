using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.LookupSymbol;

/// <summary>
/// Resolves a raw symbol string to its canonical ticker and asset type.
/// Searches VnDirect (VnStock) first, then MEXC spot (CryptoSpot).
/// For CryptoSpot, automatically appends "USDT" when only the base asset is provided (e.g. "BTC" -> "BTCUSDT").
/// Returns null if the symbol cannot be resolved to any known asset.
/// </summary>
public sealed record LookupSymbolQuery(string Symbol) : IAppQuery<SymbolLookupDto?>;

/// <summary>
/// Resolved symbol metadata returned by LookupSymbolQuery.
/// </summary>
public sealed record SymbolLookupDto(
    /// <summary>Exact ticker as returned by the data source (e.g. "BTCUSDT", "VCB").</summary>
    string Symbol,
    /// <summary>Full display name. Currently equals Symbol; extend when a long-name source becomes available.</summary>
    string FullName,
    /// <summary>Asset class: "VnStock" or "CryptoSpot".</summary>
    string AssetType);
