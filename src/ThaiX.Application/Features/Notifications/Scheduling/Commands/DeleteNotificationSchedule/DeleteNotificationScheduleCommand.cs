using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.DeleteNotificationSchedule;

[InvalidateCache(CacheGroups.NotificationSchedules)]
public sealed record DeleteNotificationScheduleCommand : IAppCommand<Unit>
{
    public Guid Id { get; init; }
}
