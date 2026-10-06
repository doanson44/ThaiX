using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.MarketScanner;

namespace ThaiX.Client.Services.MarketScanner;

/// <summary>
/// Client service for the market scanner rules API.
/// </summary>
public interface IMarketScannerService
{
    Task<PagedApiResponse<MarketScannerRuleListItemDto>> GetRulesAsync(
        MarketScannerRulesListRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateMarketScannerRuleRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateMarketScannerRuleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
