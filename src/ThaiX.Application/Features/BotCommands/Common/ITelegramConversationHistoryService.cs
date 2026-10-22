namespace ThaiX.Application.Features.BotCommands.Common;

public interface ITelegramConversationHistoryService
{
    Task<TelegramConversationHistorySnapshot> RecordIncomingMessageAsync(
        string chatId,
        long? messageId,
        string senderName,
        string content,
        CancellationToken cancellationToken);

    Task RecordAssistantMessageAsync(
        string chatId,
        string content,
        CancellationToken cancellationToken);
}

public sealed record TelegramConversationHistorySnapshot(
    bool IsDuplicate,
    IReadOnlyList<TelegramConversationMessage> RecentMessages);

public sealed record TelegramConversationMessage(
    string Role,
    string SenderName,
    string Content,
    long? MessageId,
    DateTime TimestampUtc);
