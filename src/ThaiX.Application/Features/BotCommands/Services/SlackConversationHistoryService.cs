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

public sealed class SlackConversationHistoryService(IMediator mediator) : ISlackConversationHistoryService
{
    private const int ContextMessageCount = 5;
    private const int MaxStoredMessages = 50;
    private const int MaxMessageLength = 2000;
    private const int RetentionDays = 30;
    private const string AssistantName = "ThaiX";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SlackConversationHistorySnapshot> RecordIncomingMessageAsync(
        string channelId, string? threadTs, string? messageTs, string senderName, string content,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(channelId, threadTs);
        using var gate = await AsyncLock.GetLockByKey(GetLockKey(channelId, threadTs)).LockAsync(cancellationToken);
        var bin = await LoadAsync(key, cancellationToken);
        var history = bin.Messages;

        if (!string.IsNullOrWhiteSpace(messageTs) && history.Any(x => x.Role == "user" && x.MessageId == messageTs))
        {
            return new SlackConversationHistorySnapshot(true, history.TakeLast(ContextMessageCount).ToArray());
        }

        IReadOnlyList<SlackConversationMessage> recentMessages = history.TakeLast(ContextMessageCount).ToArray();
        if (history.Count == 0 && !string.IsNullOrWhiteSpace(threadTs))
        {
            var channelHistory = await LoadAsync(BuildKey(channelId, null), cancellationToken);
            recentMessages = channelHistory.Messages.TakeLast(ContextMessageCount).ToArray();
        }

        history.Add(new SlackConversationMessage("user", Limit(senderName, 100), Limit(content, MaxMessageLength), messageTs, DateTime.UtcNow));
        TrimHistory(history);
        await SaveAsync(key, bin.Detail, history, cancellationToken);
        return new SlackConversationHistorySnapshot(false, recentMessages);
    }

    public async Task RecordAssistantMessageAsync(
        string channelId, string? threadTs, string content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var key = BuildKey(channelId, threadTs);
        using var gate = await AsyncLock.GetLockByKey(GetLockKey(channelId, threadTs)).LockAsync(cancellationToken);
        var bin = await LoadAsync(key, cancellationToken);
        bin.Messages.Add(new SlackConversationMessage("assistant", AssistantName, Limit(content, MaxMessageLength), null, DateTime.UtcNow));
        TrimHistory(bin.Messages);
        await SaveAsync(key, bin.Detail, bin.Messages, cancellationToken);
    }

    private async Task<(JsonBinDetailDto? Detail, List<SlackConversationMessage> Messages)> LoadAsync(
        string key, CancellationToken cancellationToken)
    {
        var detail = await mediator.Send(new GetJsonBinByCodeQuery { Code = key }, cancellationToken);
        if (string.IsNullOrWhiteSpace(detail?.ContentJson))
        {
            return (detail, []);
        }

        try
        {
            var messages = JsonSerializer.Deserialize<List<SlackConversationMessage>>(detail.ContentJson, JsonOptions);
            return (detail, messages ?? []);
        }
        catch (JsonException)
        {
            return (detail, []);
        }
    }

    private async Task SaveAsync(
        string key, JsonBinDetailDto? detail, List<SlackConversationMessage> messages, CancellationToken cancellationToken)
    {
        var contentJson = JsonSerializer.Serialize(messages, JsonOptions);
        var expiresAtUtc = DateTime.UtcNow.AddDays(RetentionDays);
        var name = $"Slack conversation {key}";

        if (detail is null)
        {
            await mediator.Send(new CreateJsonBinCommand
            {
                Code = key, Name = name, Category = JsonBinCategories.Temp, ContentJson = contentJson,
                ContentType = "application/json", Compress = true, Tags = "slack-conversation", ExpiredAtUtc = expiresAtUtc
            }, cancellationToken);
            return;
        }

        await mediator.Send(new UpdateJsonBinCommand
        {
            Id = detail.Id, Name = name, Category = JsonBinCategories.Temp, ContentJson = contentJson,
            ContentType = "application/json", Compress = true, Tags = "slack-conversation", ExpiredAtUtc = expiresAtUtc
        }, cancellationToken);
    }

    private static string BuildKey(string channelId, string? threadTs) =>
        string.IsNullOrWhiteSpace(threadTs)
            ? $"SLACK_CHANNEL_{channelId.Trim()}"
            : $"SLACK_THREAD_{channelId.Trim()}_{threadTs.Trim()}";

    private static string GetLockKey(string channelId, string? threadTs) =>
        $"slack-conversation:{channelId.Trim()}:{threadTs?.Trim() ?? "channel"}";

    private static string Limit(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..(maxLength - 3)] + "...";
    }

    private static void TrimHistory(List<SlackConversationMessage> history)
    {
        if (history.Count > MaxStoredMessages)
        {
            history.RemoveRange(0, history.Count - MaxStoredMessages);
        }
    }
}
