using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetUpcomingSchedules;

public sealed class GetUpcomingSchedulesQueryHandler
    : IRequestHandler<GetUpcomingSchedulesQuery, List<UpcomingScheduleDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUpcomingSchedulesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<UpcomingScheduleDto>> Handle(
        GetUpcomingSchedulesQuery q, CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;
        return await _db.NotificationSchedules
            .Where(s => s.Status == ScheduleStatus.Active && s.NextExecuteAtUtc > utcNow)
            .OrderBy(s => s.NextExecuteAtUtc)
            .Take(q.Count)
            .Select(s => new UpcomingScheduleDto
            {
                ScheduleId = s.Id,
                ScheduleName = s.Name,
                NextExecuteAtUtc = s.NextExecuteAtUtc!.Value,
                Type = s.Recurrence.Type
            })
            .ToListAsync(ct);
    }
}
