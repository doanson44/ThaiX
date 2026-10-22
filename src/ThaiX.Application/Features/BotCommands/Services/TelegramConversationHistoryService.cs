using System.Text.Json;
using MediatR;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinByCode;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class TelegramConversationHistoryService(IMediator mediator) : ITelegramConversationHistoryService
{
    private const int ContextMessageCount = 5;
    private const int MaxStoredMessages = 50;
    private const int MaxMessageLength = 2000;
    private const int RetentionDays = 30;
    private const string AssistantName = "ThaiX";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TelegramConversationHistorySnapshot> RecordIncomingMessageAsync(
        string chatId,
        long? messageId,
        string senderName,
        string content,
        CancellationToken cancellationToken)
    {
        using var gate = await AsyncLock.GetLockByKey(GetLockKey(chatId)).LockAsync(cancellationToken);
        var bin = await LoadAsync(chatId, cancellationToken);
        var history = bin.Messages;

        if (messageId.HasValue && history.Any(x => x.Role == "user" && x.MessageId == messageId))
        {
            return new TelegramConversationHistorySnapshot(
                true,
                history.TakeLast(ContextMessageCount).ToArray());
        }

        var recentMessages = history.TakeLast(ContextMessageCount).ToArray();
        history.Add(new TelegramConversationMessage(
            "user",
            Limit(senderName, 100),
            Limit(content, MaxMessageLength),
            messageId,
            DateTime.UtcNow));

        TrimHistory(history);
        await SaveAsync(chatId, bin.Detail, history, cancellationToken);

        return new TelegramConversationHistorySnapshot(false, recentMessages);
    }

    public async Task RecordAssistantMessageAsync(
        string chatId,
        string content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        using var gate = await AsyncLock.GetLockByKey(GetLockKey(chatId)).LockAsync(cancellationToken);
        var bin = await LoadAsync(chatId, cancellationToken);
        bin.Messages.Add(new TelegramConversationMessage(
            "assistant",
            AssistantName,
            Limit(content, MaxMessageLength),
            null,
            DateTime.UtcNow));

        TrimHistory(bin.Messages);
        await SaveAsync(chatId, bin.Detail, bin.Messages, cancellationToken);
    }

    private async Task<(JsonBinDetailDto? Detail, List<TelegramConversationMessage> Messages)> LoadAsync(
        string chatId,
        CancellationToken cancellationToken)
    {
        var detail = await mediator.Send(new GetJsonBinByCodeQuery
        {
            Code = BuildCode(chatId)
        }, cancellationToken);

        if (string.IsNullOrWhiteSpace(detail?.ContentJson))
        {
            return (detail, []);
        }

        try
        {
            var messages = JsonSerializer.Deserialize<List<TelegramConversationMessage>>(detail.ContentJson, JsonOptions);
            return (detail, messages ?? []);
        }
        catch (JsonException)
        {
            return (detail, []);
        }
    }

    private async Task SaveAsync(
        string chatId,
        JsonBinDetailDto? detail,
        List<TelegramConversationMessage> messages,
        CancellationToken cancellationToken)
    {
        var contentJson = JsonSerializer.Serialize(messages, JsonOptions);
        var code = BuildCode(chatId);
        var name = $"Telegram conversation {chatId}";
        var expiresAtUtc = DateTime.UtcNow.AddDays(RetentionDays);

        if (detail is null)
        {
            await mediator.Send(new CreateJsonBinCommand
            {
                Code = code,
                Name = name,
                Category = JsonBinCategories.Temp,
                ContentJson = contentJson,
                ContentType = "application/json",
                Compress = true,
                Tags = "telegram-conversation",
                ExpiredAtUtc = expiresAtUtc
            }, cancellationToken);

            return;
        }

        await mediator.Send(new UpdateJsonBinCommand
        {
            Id = detail.Id,
            Name = name,
            Category = JsonBinCategories.Temp,
            ContentJson = contentJson,
            ContentType = "application/json",
            Compress = true,
            Tags = "telegram-conversation",
            ExpiredAtUtc = expiresAtUtc
        }, cancellationToken);
    }

    private static string BuildCode(string chatId) => $"TELEGRAM_CHAT_{chatId.Trim()}";

    private static string GetLockKey(string chatId) => $"telegram-conversation:{chatId.Trim()}";

    private static string Limit(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..(maxLength - 3)] + "...";
    }

    private static void TrimHistory(List<TelegramConversationMessage> history)
    {
        if (history.Count > MaxStoredMessages)
        {
            history.RemoveRange(0, history.Count - MaxStoredMessages);
        }
    }
}
