using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Application.Features.Notifications.EventHandlers;

public sealed class NotificationReadyForDeliveryEventHandler
    : INotificationHandler<NotificationReadyForDeliveryEvent>
{
    private readonly INotificationDeliveryScheduler _scheduler;

    public NotificationReadyForDeliveryEventHandler(INotificationDeliveryScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public Task Handle(
        NotificationReadyForDeliveryEvent notification,
        CancellationToken cancellationToken)
    {
        return _scheduler.ScheduleNotificationAsync(
            notification.NotificationId,
            notification.ScheduledAtUtc,
            cancellationToken);
    }
}
