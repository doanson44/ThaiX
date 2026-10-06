using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.PriceAlerts;

namespace ThaiX.Client.Services.PriceAlerts;

/// <summary>
/// Client service for the price alerts API.
/// </summary>
public interface IPriceAlertService
{
    Task<PagedApiResponse<PriceAlertListItemDto>> GetAlertsAsync(
        PriceAlertsListRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreatePriceAlertRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdatePriceAlertRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
