using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed record NotificationTemplate
{
    public required string Key { get; init; }

    public required NotificationKind Kind { get; init; }

    public required string DefaultSubject { get; init; }
}
