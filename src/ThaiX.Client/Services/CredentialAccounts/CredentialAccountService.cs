using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.CredentialAccounts;

public sealed class CredentialAccountService : ICredentialAccountService
{
    private const string BaseUrl = "api/credential-accounts";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public CredentialAccountService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<CredentialAccountDto>> GetListAsync(
        CredentialAccountsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.CredentialAccount,
            path,
            ct => FetchListAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public Task<CredentialAccountDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.CredentialAccount,
            id.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<CredentialAccountDetailDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateAsync(CreateCredentialAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(Guid id, UpdateCredentialAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid id,
        ChangeCredentialAccountPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/password", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
    }

    public async Task MarkUsedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/mark-used", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
    }

    public async Task ResetUsedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/reset-used", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCredentialCachesAsync(cancellationToken);
    }

    public async Task<CredentialAccountPasswordDto> ViewPasswordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/password/view", null, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<CredentialAccountPasswordDto>(response, cancellationToken);
    }

    public async Task<CredentialAccountPasswordDto> CopyPasswordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/password/copy", null, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<CredentialAccountPasswordDto>(response, cancellationToken);
    }

    public Task<PagedApiResponse<CredentialAccountAuditDto>> GetAuditLogsAsync(
        Guid credentialAccountId,
        CredentialAccountAuditListRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"{BaseUrl}/{credentialAccountId}/audit-logs?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.CredentialAccountAudit,
            url,
            async ct =>
            {
                using var response = await _httpClient.GetAsync(url, ct);
                return await ApiResponseReader.ReadPagedSuccessAsync<CredentialAccountAuditDto>(response, ct);
            },
            ClientCacheTtl.AuditLogs,
            cancellationToken);
    }

    private async Task<PagedApiResponse<CredentialAccountDto>> FetchListAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<CredentialAccountDto>(response, cancellationToken);
    }

    private Task InvalidateCredentialCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.CredentialAccount, cancellationToken);

    private static string BuildListPath(CredentialAccountsListRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sb.Append($"&sortBy={Uri.EscapeDataString(request.SortBy)}&sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");
        if (request.IsUsed.HasValue)
            sb.Append($"&isUsed={request.IsUsed.Value.ToString().ToLowerInvariant()}");

        return sb.ToString();
    }
}
