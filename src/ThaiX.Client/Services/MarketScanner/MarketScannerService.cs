using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.MarketScanner;

public sealed class MarketScannerService : IMarketScannerService
{
    private const string BaseUrl = "api/market-scanner/rules";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public MarketScannerService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<MarketScannerRuleListItemDto>> GetRulesAsync(
        MarketScannerRulesListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.MarketScannerRule,
            path,
            ct => FetchRulesAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        CreateMarketScannerRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateRulesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateMarketScannerRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateRulesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateRulesAsync(cancellationToken);
    }

    private async Task<PagedApiResponse<MarketScannerRuleListItemDto>> FetchRulesAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<MarketScannerRuleListItemDto>(response, cancellationToken);
    }

    private Task InvalidateRulesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.MarketScannerRule, cancellationToken);

    private static string BuildListPath(MarketScannerRulesListRequest request)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}?pageNumber={1}&pageSize={2}&sortBy={3}&sortDescending={4}{5}",
            BaseUrl,
            request.PageNumber,
            request.PageSize,
            request.SortBy ?? "name",
            request.SortDescending.ToString().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(request.SearchTerm) ? string.Empty : $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
    }
}
