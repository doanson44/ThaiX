namespace ThaiX.Application.Common.Interfaces;

public interface INotificationDeliveryScheduler
{
    Task ScheduleNotificationAsync(
        Guid notificationId,
        DateTime? scheduledAtUtc,
        CancellationToken cancellationToken);
}
