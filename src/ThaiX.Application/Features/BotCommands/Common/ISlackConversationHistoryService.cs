namespace ThaiX.Application.Features.BotCommands.Common;

public interface ISlackConversationHistoryService
{
    Task<SlackConversationHistorySnapshot> RecordIncomingMessageAsync(
        string channelId, string? threadTs, string? messageTs, string senderName, string content,
        CancellationToken cancellationToken);

    Task RecordAssistantMessageAsync(
        string channelId, string? threadTs, string content, CancellationToken cancellationToken);
}

public sealed record SlackConversationHistorySnapshot(bool IsDuplicate, IReadOnlyList<SlackConversationMessage> RecentMessages);
public sealed record SlackConversationMessage(string Role, string SenderName, string Content, string? MessageId, DateTime TimestampUtc);
