using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Lottery;

public sealed class LotteryService : ILotteryService
{
    private const string BaseUrl = "api/lottery/power-655";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public LotteryService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<Power655AnalysisResponse> GetAnalysisAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.LotteryAnalysis,
            "analysis",
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/analysis", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<Power655AnalysisResponse>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task TriggerSyncAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/sync", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.LotteryAnalysis, cancellationToken);
    }

    public async Task TriggerPredictAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/predict", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.LotteryAnalysis, cancellationToken);
    }
}
