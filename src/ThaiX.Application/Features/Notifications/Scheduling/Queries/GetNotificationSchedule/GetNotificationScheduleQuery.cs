using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedule;

public sealed record GetNotificationScheduleQuery : IAppQuery<NotificationScheduleDto?>, ICacheableQuery
{
    public Guid Id { get; init; }

    public string CacheKey => CacheKeys.NotificationSchedules.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.NotificationSchedules;
    public bool IsVersionedList => false;
}
