using System.Net;
using System.Net.Http.Json;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.ApiClients;

public sealed class ApiClientService : IApiClientService
{
    private const string BaseUrl = "api/api-clients";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public ApiClientService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<IReadOnlyList<ApiClientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ApiClient,
            "all",
            async ct =>
            {
                using var response = await _httpClient.GetAsync(BaseUrl, ct);
                var data = await ApiResponseReader.ReadSuccessDataAsync<List<ApiClientDto>>(response, ct);
                return (IReadOnlyList<ApiClientDto>)data;
            },
            cancellationToken: cancellationToken);
    }

    public Task<ApiClientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ApiClient,
            id.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                return await ApiResponseReader.ReadSuccessDataAsync<ApiClientDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetAvailableScopesAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ApiClientScopes,
            "all",
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/available-scopes", ct);
                var data = await ApiResponseReader.ReadSuccessDataAsync<List<string>>(response, ct);
                return (IReadOnlyList<string>)data;
            },
            cancellationToken: cancellationToken);
    }

    public async Task<CreateApiClientResponse> CreateAsync(
        CreateApiClientRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<CreateApiClientResponse>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        return result;
    }

    public async Task<ApiClientDto> UpdateAsync(
        Guid id,
        UpdateApiClientRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ApiClientDto>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        return result;
    }

    public async Task<ApiClientDto> UpdateScopesAsync(
        Guid id,
        UpdateApiClientScopesRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/scopes", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ApiClientDto>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ApiClientScopes, cancellationToken);
        return result;
    }

    public async Task<RegenerateApiClientSecretResponse> RegenerateSecretAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/regenerate-secret", null, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<RegenerateApiClientSecretResponse>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        return result;
    }

    public async Task<ApiClientDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/activate", null, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ApiClientDto>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        return result;
    }

    public async Task<ApiClientDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/deactivate", null, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ApiClientDto>(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateApiClientCachesAsync(cancellationToken);
    }

    private async Task InvalidateApiClientCachesAsync(CancellationToken cancellationToken)
    {
        await _cache.InvalidateGroupAsync(CacheGroups.ApiClient, cancellationToken);
    }
}
