using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.RunScheduleNow;

[InvalidateCache(CacheGroups.NotificationSchedules)]
public sealed record RunScheduleNowCommand : IAppCommand<ScheduleExecutionDto>
{
    public Guid Id { get; init; }
}
