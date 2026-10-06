using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetUpcomingSchedules;

public sealed record GetUpcomingSchedulesQuery : IAppQuery<List<UpcomingScheduleDto>>, ICacheableQuery
{
    public int Count { get; init; } = 20;

    public string CacheKey => CacheKeys.NotificationSchedules.Upcoming(Count);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
    public string CacheGroup => CacheGroups.NotificationSchedules;
    public bool IsVersionedList => false;
}
