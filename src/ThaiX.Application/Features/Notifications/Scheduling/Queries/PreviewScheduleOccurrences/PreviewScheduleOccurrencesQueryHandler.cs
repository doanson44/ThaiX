using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.PreviewScheduleOccurrences;

public sealed class PreviewScheduleOccurrencesQueryHandler
    : IRequestHandler<PreviewScheduleOccurrencesQuery, List<DateTime>?>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public PreviewScheduleOccurrencesQueryHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<List<DateTime>?> Handle(
        PreviewScheduleOccurrencesQuery q, CancellationToken ct)
    {
        var schedule = await _db.NotificationSchedules
            .FirstOrDefaultAsync(s => s.Id == q.ScheduleId && s.Status != ScheduleStatus.Deleted, ct);

        if (schedule is null) return null;

        return _calculator.PreviewOccurrences(schedule, q.Count, _dateTime.UtcNow).ToList();
    }
}
