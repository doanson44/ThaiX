using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Notifications;

public sealed class NotificationScheduleService : INotificationScheduleService
{
    private const string BasePath = "api/notification-schedules";
    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public NotificationScheduleService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<NotificationScheduleListItemDto>> GetSchedulesAsync(
        int pageNumber = 1, int pageSize = 20, string? search = null, string? status = null,
        CancellationToken ct = default)
    {
        var qs = $"pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) qs += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(status)) qs += $"&status={Uri.EscapeDataString(status)}";
        var path = $"{BasePath}?{qs}";

        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.NotificationSchedule,
            path,
            async cancellationToken =>
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                return await ApiResponseReader.ReadPagedSuccessAsync<NotificationScheduleListItemDto>(response, cancellationToken);
            },
            cancellationToken: ct);
    }

    public Task<NotificationScheduleDto> GetScheduleAsync(Guid id, CancellationToken ct = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.NotificationSchedule,
            id.ToString(),
            async cancellationToken =>
            {
                using var response = await _httpClient.GetAsync($"{BasePath}/{id}", cancellationToken);
                return (await ApiResponseReader.ReadSuccessDataAsync<NotificationScheduleDto>(response, cancellationToken))!;
            },
            cancellationToken: ct);
    }

    public async Task<NotificationScheduleDto> CreateScheduleAsync(
        CreateNotificationScheduleRequest request, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        var result = (await ApiResponseReader.ReadSuccessDataAsync<NotificationScheduleDto>(response, ct))!;
        await InvalidateScheduleCachesAsync(ct);
        return result;
    }

    public async Task UpdateScheduleAsync(Guid id, UpdateNotificationScheduleRequest request,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BasePath}/{id}", request, ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public async Task DeleteScheduleAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BasePath}/{id}", ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsync($"{BasePath}/{id}/activate", null, ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public async Task PauseAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsync($"{BasePath}/{id}/pause", null, ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public async Task ResumeAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsync($"{BasePath}/{id}/resume", null, ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsync($"{BasePath}/{id}/disable", null, ct);
        response.EnsureSuccessStatusCode();
        await InvalidateScheduleCachesAsync(ct);
    }

    public Task<List<NotificationScheduleListItemDto>> GetUpcomingAsync(
        int count = 10,
        CancellationToken ct = default)
    {
        var path = $"{BasePath}/upcoming?count={count}";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.NotificationScheduleUpcoming,
            path,
            async cancellationToken =>
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                return await ApiResponseReader.ReadSuccessDataAsync<List<NotificationScheduleListItemDto>>(
                    response, cancellationToken) ?? [];
            },
            ClientCacheTtl.Polling,
            ct);
    }

    public async Task<ScheduleExecutionDto> RunNowAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsync($"{BasePath}/{id}/run-now", null, ct);
        var result = (await ApiResponseReader.ReadSuccessDataAsync<ScheduleExecutionDto>(response, ct))!;
        await _cache.InvalidateGroupAsync(CacheGroups.NotificationScheduleExecutions, ct);
        return result;
    }

    public Task<PagedApiResponse<ScheduleExecutionDto>> GetExecutionsAsync(
        Guid scheduleId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var path = $"{BasePath}/{scheduleId}/executions?pageNumber={pageNumber}&pageSize={pageSize}";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.NotificationScheduleExecutions,
            path,
            async cancellationToken =>
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                return await ApiResponseReader.ReadPagedSuccessAsync<ScheduleExecutionDto>(response, cancellationToken);
            },
            ClientCacheTtl.Polling,
            ct);
    }

    public Task<List<DateTime>> PreviewAsync(Guid id, int count = 10,
        CancellationToken ct = default)
    {
        var path = $"{BasePath}/{id}/preview?count={count}";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.NotificationSchedule,
            $"preview:{id}:{count}",
            async cancellationToken =>
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                return await ApiResponseReader.ReadSuccessDataAsync<List<DateTime>>(response, cancellationToken) ?? new();
            },
            cancellationToken: ct);
    }

    private async Task InvalidateScheduleCachesAsync(CancellationToken ct)
    {
        await _cache.InvalidateGroupAsync(CacheGroups.NotificationSchedule, ct);
        await _cache.InvalidateGroupAsync(CacheGroups.NotificationScheduleUpcoming, ct);
    }
}
