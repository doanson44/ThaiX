using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Users;

public sealed class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public UserService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<UserListItemDto>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildGetUsersPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.User,
            path,
            ct => FetchPagedUsersAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.UserPermissionCatalog,
            "all",
            async ct =>
            {
                using var response = await _httpClient.GetAsync("api/users/permissions", ct);
                var data = await ApiResponseReader.ReadSuccessDataAsync<List<string>>(response, ct);
                return (IReadOnlyList<string>)data;
            },
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.UserPermissions,
            userId.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"api/users/{userId}/permissions", ct);
                var data = await ApiResponseReader.ReadSuccessDataAsync<List<string>>(response, ct);
                return (IReadOnlyList<string>)(data ?? []);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/users", request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateUserCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateUserAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateUserCachesAsync(cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserPermissions, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserLinkedContact, cancellationToken);
    }

    public async Task SetLockoutAsync(
        Guid userId,
        SetUserLockoutRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/lockout", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateUserCachesAsync(cancellationToken);
    }

    public async Task SetPermissionsAsync(
        Guid userId,
        SetUserPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/permissions", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserPermissions, cancellationToken);
        await InvalidateUserCachesAsync(cancellationToken);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(
        Guid userId,
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"api/users/{userId}/reset-password", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<ResetPasswordResult>(response, cancellationToken);
    }

    public Task<LinkedContactDto?> GetLinkedContactAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.UserLinkedContact,
            userId.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"api/users/{userId}/linked-contact", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<LinkedContactDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task UnlinkContactFromUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/users/{userId}/linked-contact", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserLinkedContact, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Contact, cancellationToken);
        await InvalidateUserCachesAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var entry = await _cache.GetOrCreateGroupedAsync(
            CacheGroups.CurrentUserProfile,
            "me",
            FetchCurrentUserProfileEntryAsync,
            cancellationToken: cancellationToken);
        return entry.Profile;
    }

    private async Task<UserProfileCacheEntry> FetchCurrentUserProfileEntryAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("api/users/me/profile", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new UserProfileCacheEntry { Profile = null };

        var profile = await ApiResponseReader.ReadSuccessDataAsync<UserProfileDto>(response, cancellationToken);
        return new UserProfileCacheEntry { Profile = profile };
    }

    private async Task<PagedApiResponse<UserListItemDto>> FetchPagedUsersAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<UserListItemDto>(response, cancellationToken);
    }

    private Task InvalidateUserCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.User, cancellationToken);

    private sealed class UserProfileCacheEntry
    {
        public UserProfileDto? Profile { get; init; }
    }

    private static string BuildGetUsersPath(GetUsersRequest request)
    {
        var queryParts = new List<string>
        {
            $"pageNumber={request.PageNumber.ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={request.PageSize.ToString(CultureInfo.InvariantCulture)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy)}",
            $"sortDescending={request.SortDescending.ToString().ToLowerInvariant()}"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            queryParts.Add($"search={Uri.EscapeDataString(request.SearchTerm.Trim())}");
        }

        if (request.IsActive.HasValue)
        {
            queryParts.Add($"isActive={request.IsActive.Value.ToString().ToLowerInvariant()}");
        }

        var builder = new StringBuilder("api/users/?");
        builder.Append(string.Join("&", queryParts));

        return builder.ToString();
    }
}
