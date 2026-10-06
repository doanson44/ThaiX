using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedules;

public sealed record GetNotificationSchedulesQuery : PagedRequest, IAppQuery<PagedResult<NotificationScheduleListItemDto>>, ICacheableQuery
{
    public string? Search { get; init; }
    public string? Status { get; init; }

    public string CacheKey => CacheKeys.NotificationSchedules.List(
        Search,
        Status,
        PageNumber,
        PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.NotificationSchedules;
    public bool IsVersionedList => true;
}
