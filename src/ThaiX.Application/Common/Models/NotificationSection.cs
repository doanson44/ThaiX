namespace ThaiX.Application.Common.Models;

public sealed record NotificationSection
{
    public string? Title { get; init; }

    public required string Text { get; init; }
}
