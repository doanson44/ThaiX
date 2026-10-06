using ThaiX.Client.Models.Notifications;

namespace ThaiX.Client.Services.Notifications;

public interface INotificationService
{
    Task SendNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);

    Task<NotificationDetailDto> GetNotificationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<UserNotificationPreferenceDto>> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpsertPreferenceAsync(
        UpsertUserNotificationPreferenceRequest request,
        CancellationToken cancellationToken = default);
}
