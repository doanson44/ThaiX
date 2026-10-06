using MediatR;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetScheduleExecutions;

public sealed class GetScheduleExecutionsQueryHandler
    : IRequestHandler<GetScheduleExecutionsQuery, PagedResult<ScheduleExecutionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetScheduleExecutionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ScheduleExecutionDto>> Handle(
        GetScheduleExecutionsQuery q, CancellationToken ct)
    {
        return await _db.NotificationScheduleExecutions
            .Where(e => e.ScheduleId == q.ScheduleId)
            .OrderByDescending(e => e.TriggeredAtUtc)
            .Select(e => new ScheduleExecutionDto
            {
                Id = e.Id,
                ScheduleId = e.ScheduleId,
                Status = e.Status,
                OccurrenceTimeUtc = e.OccurrenceTimeUtc,
                OccurrenceOrdinal = e.OccurrenceOrdinal,
                TriggeredAtUtc = e.TriggeredAtUtc,
                CompletedAtUtc = e.CompletedAtUtc,
                ErrorMessage = e.ErrorMessage
            })
            .ToPagedListAsync(q.PageNumber, q.PageSize, ct);
    }
}
