namespace ThaiX.Client.Models.Slack;

public sealed class ExecuteBotCommandTesterRequest
{
    public string RawText { get; set; } = string.Empty;
    public string Channel { get; set; } = "Slack";
    public string ExternalUserId { get; set; } = string.Empty;
    public string? ExternalChannelId { get; set; }
}
