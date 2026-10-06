using System.Net.Http.Json;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Notifications;

public sealed class NotificationApiService : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendNotificationAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/notifications/send", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<NotificationDetailDto> GetNotificationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/notifications/{id}", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<NotificationDetailDto>(response, cancellationToken);
    }

    public async Task<List<UserNotificationPreferenceDto>> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/notifications/preferences/{userId}",
            cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<List<UserNotificationPreferenceDto>>(
            response,
            cancellationToken);
    }

    public async Task UpsertPreferenceAsync(
        UpsertUserNotificationPreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            "api/notifications/preferences",
            request,
            cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }
}
