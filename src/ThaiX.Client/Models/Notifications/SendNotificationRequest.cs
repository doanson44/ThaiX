namespace ThaiX.Client.Models.Notifications;

public sealed class SendNotificationRequest
{
    public NotificationEventType EventType { get; set; } = NotificationEventType.SystemAlert;
    public string Text { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Severity { get; set; }
    public NotificationTarget Target { get; set; } = NotificationTarget.Both;
}
