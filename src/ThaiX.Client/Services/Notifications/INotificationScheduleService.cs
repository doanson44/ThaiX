using ThaiX.Client.Models.Api;

namespace ThaiX.Client.Services.Notifications;

public interface INotificationScheduleService
{
    Task<PagedApiResponse<NotificationScheduleListItemDto>> GetSchedulesAsync(
        int pageNumber = 1, int pageSize = 20, string? search = null, string? status = null,
        CancellationToken ct = default);

    Task<NotificationScheduleDto> GetScheduleAsync(Guid id, CancellationToken ct = default);

    Task<NotificationScheduleDto> CreateScheduleAsync(
        CreateNotificationScheduleRequest request, CancellationToken ct = default);

    Task UpdateScheduleAsync(Guid id, UpdateNotificationScheduleRequest request,
        CancellationToken ct = default);

    Task DeleteScheduleAsync(Guid id, CancellationToken ct = default);

    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task PauseAsync(Guid id, CancellationToken ct = default);
    Task ResumeAsync(Guid id, CancellationToken ct = default);
    Task DisableAsync(Guid id, CancellationToken ct = default);

    Task<List<NotificationScheduleListItemDto>> GetUpcomingAsync(
        int count = 10,
        CancellationToken ct = default);

    Task<ScheduleExecutionDto> RunNowAsync(Guid id, CancellationToken ct = default);

    Task<PagedApiResponse<ScheduleExecutionDto>> GetExecutionsAsync(
        Guid scheduleId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default);

    Task<List<DateTime>> PreviewAsync(Guid id, int count = 10,
        CancellationToken ct = default);
}
