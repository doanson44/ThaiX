using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetScheduleExecutions;

public sealed record GetScheduleExecutionsQuery : PagedRequest, IAppQuery<PagedResult<ScheduleExecutionDto>>, ICacheableQuery
{
    public Guid ScheduleId { get; init; }

    public string CacheKey => CacheKeys.NotificationSchedules.Executions(
        ScheduleId,
        PageNumber,
        PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
    public string CacheGroup => CacheGroups.NotificationSchedules;
    public bool IsVersionedList => false;
}
