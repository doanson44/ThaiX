using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedules;

public sealed class GetNotificationSchedulesQueryHandler
    : IRequestHandler<GetNotificationSchedulesQuery, PagedResult<NotificationScheduleListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetNotificationSchedulesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<NotificationScheduleListItemDto>> Handle(
        GetNotificationSchedulesQuery q, CancellationToken ct)
    {
        var query = _db.NotificationSchedules
            .Where(s => s.Status != ScheduleStatus.Deleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            query = query.Where(s => EF.Functions.Like(s.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(q.Status)
            && Enum.TryParse<ScheduleStatus>(q.Status, true, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        return await query
            .OrderByDescending(s => s.NextExecuteAtUtc)
            .Select(s => new NotificationScheduleListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Recurrence.Type,
                Status = s.Status,
                NextExecuteAtUtc = s.NextExecuteAtUtc,
                FailureCount = s.FailureCount,
                ExecutionCount = s.ExecutionCount,
                LastTriggeredAtUtc = s.LastTriggeredAtUtc
            })
            .ToPagedListAsync(q.PageNumber, q.PageSize, ct);
    }
}
