using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.PreviewScheduleOccurrences;

public sealed record PreviewScheduleOccurrencesQuery : IAppQuery<List<DateTime>?>
{
    public Guid ScheduleId { get; init; }
    public int Count { get; init; } = 10;
}
