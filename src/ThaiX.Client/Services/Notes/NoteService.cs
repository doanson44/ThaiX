using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Notes;

public sealed class NoteService : INoteService
{
    private const string BaseUrl = "api/notes";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public NoteService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<NoteDto>> GetListAsync(
        NotesListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Note,
            path,
            ct => FetchListAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public Task<NoteDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Note,
            id.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<NoteDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        CreateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateNoteCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateNoteCachesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateNoteCachesAsync(cancellationToken);
    }

    public async Task TogglePinAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/pin", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateNoteCachesAsync(cancellationToken);
    }

    public async Task ToggleArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/archive", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateNoteCachesAsync(cancellationToken);
    }

    private async Task<PagedApiResponse<NoteDto>> FetchListAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<NoteDto>(response, cancellationToken);
    }

    private Task InvalidateNoteCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.Note, cancellationToken);

    private static string BuildListPath(NotesListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sb.Append($"&sortBy={Uri.EscapeDataString(request.SortBy)}&sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.IsPinned.HasValue)
            sb.Append($"&isPinned={request.IsPinned.Value.ToString().ToLowerInvariant()}");
        if (request.IsArchived.HasValue)
            sb.Append($"&isArchived={request.IsArchived.Value.ToString().ToLowerInvariant()}");
        if (request.IsDeleted.HasValue)
            sb.Append($"&isDeleted={request.IsDeleted.Value.ToString().ToLowerInvariant()}");

        return sb.ToString();
    }
}
