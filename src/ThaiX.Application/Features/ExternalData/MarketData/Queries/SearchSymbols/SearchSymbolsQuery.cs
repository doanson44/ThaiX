using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.SearchSymbols;

/// <summary>
/// Searches and paginates symbols for a given asset type (CryptoSpot or VnStock).
/// </summary>
public sealed record SearchSymbolsQuery(
    string AssetType,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IAppQuery<SymbolSearchResultDto>;

public sealed record SymbolSearchResultDto(
    IReadOnlyList<SymbolSearchItemDto> Items,
    int TotalCount);

public sealed record SymbolSearchItemDto(
    string Symbol,
    string DisplayText);
