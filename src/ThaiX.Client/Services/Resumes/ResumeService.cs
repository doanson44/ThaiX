using System.Net;
using System.Net.Http.Json;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Resumes;

public sealed class ResumeService : IResumeService
{
    private const string BaseUrl = "api/resumes";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public ResumeService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<ResumeDto?> GetMineAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/mine", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ApiResponseReader.ReadSuccessDataAsync<ResumeDto>(response, cancellationToken);
    }

    public async Task<Guid> SaveAsync(UpsertResumeRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/mine", request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ResumePublic, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.CurrentUserProfile, cancellationToken);
        return id;
    }

    public Task<ResumeDto?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ResumePublic,
            slug,
            async ct =>
            {
                using var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/public/{Uri.EscapeDataString(slug)}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                return await ApiResponseReader.ReadSuccessDataAsync<ResumeDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<string> PolishTextAsync(PolishResumeTextRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ai/polish-text", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ResumeAiTextResponse>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> GenerateSummaryAsync(GenerateResumeSummaryRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ai/generate-summary", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ResumeAiTextResponse>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> OptimizeForJobAsync(OptimizeResumeForJobRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ai/optimize-for-job", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ResumeAiReportResponse>(response, cancellationToken);
        return result.Report;
    }

    public async Task<string> CheckConsistencyAsync(CheckResumeConsistencyRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ai/check-consistency", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ResumeAiReportResponse>(response, cancellationToken);
        return result.Report;
    }
}
