using System.Text.Json;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public enum NotificationCardType
{
    Metric = 1,
    Signal = 2,
    Alert = 3,
    Summary = 4,
    Analysis = 5,
    Error = 6
}

public sealed record NotificationMetric
{
    public required string Label { get; init; }
    public required string Value { get; init; }
}

public sealed record NotificationCard
{
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required NotificationCardType Type { get; init; }
    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;
    public IReadOnlyList<NotificationMetric> Metrics { get; init; } = [];
    public IReadOnlyList<NotificationSection> Sections { get; init; } = [];
    public IReadOnlyList<NotificationAction> Actions { get; init; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static NotificationCard? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<NotificationCard>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // DataJson is shared with non-card payloads (e.g. ad-hoc schedule data),
            // so a shape mismatch here means "no card", not a corrupt record.
            return null;
        }
    }
}
