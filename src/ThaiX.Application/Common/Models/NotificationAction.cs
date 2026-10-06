namespace ThaiX.Application.Common.Models;

public sealed record NotificationAction
{
    public required string Label { get; init; }

    public required string Url { get; init; }
}
