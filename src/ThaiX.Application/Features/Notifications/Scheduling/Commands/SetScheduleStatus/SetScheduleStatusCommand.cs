using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.SetScheduleStatus;

[InvalidateCache(CacheGroups.NotificationSchedules)]
public sealed record SetScheduleStatusCommand : IAppCommand<Unit>
{
    public Guid Id { get; init; }
    public ScheduleStatus Status { get; init; }
}
