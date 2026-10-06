using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.PriceAlerts;

public sealed class PriceAlertService : IPriceAlertService
{
    private const string BaseUrl = "api/price-alerts";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public PriceAlertService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<PriceAlertListItemDto>> GetAlertsAsync(
        PriceAlertsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.PriceAlert,
            path,
            ct => FetchAlertsAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        CreatePriceAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateAlertsAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdatePriceAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateAlertsAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateAlertsAsync(cancellationToken);
    }

    private async Task<PagedApiResponse<PriceAlertListItemDto>> FetchAlertsAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<PriceAlertListItemDto>(response, cancellationToken);
    }

    private Task InvalidateAlertsAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.PriceAlert, cancellationToken);

    private static string BuildListPath(PriceAlertsListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}&sortBy={request.SortBy ?? "symbol"}&sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        if (request.IsEnabled.HasValue)
            sb.Append($"&isEnabled={request.IsEnabled.Value.ToString().ToLowerInvariant()}");

        return sb.ToString();
    }
}
