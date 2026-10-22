using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services.Notifications;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class TelegramEndpoints
{
    public static void MapTelegramEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/telegram")
            .WithTags("Telegram");

        group.MapGet("/commands/channels", (
            IOptions<TelegramSettings> telegramOptions,
            HttpContext httpContext) =>
        {
            var channels = telegramOptions.Value.Channels
                .SelectMany(group => group.Value, (group, kv) => new { Group = group.Key, UserKey = kv.Key, ChatId = kv.Value })
                .Where(x => !string.IsNullOrWhiteSpace(x.ChatId))
                .Select(x => new ChannelInfoDto(
                    x.ChatId.Trim(),
                    $"{x.Group}/{x.UserKey} ({x.ChatId.Trim()})"))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var response = ApiResponse<IReadOnlyList<ChannelInfoDto>>.SuccessResult(channels);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("GetTelegramCommandChannels")
        .WithDescription("Get configured Telegram channels available for command testing.");

        group.MapPost("/webhook", async (
            [FromBody] ExecuteTelegramWebhookRequest request,
            IMediator mediator,
            IBotUserMappingService userMappingService,
            IBotCommandIntentResolver botCommandIntentResolver,
            IBotCommandResponseNarrator botCommandResponseNarrator,
            IBotConversationalResponder botConversationalResponder,
            ITelegramConversationHistoryService conversationHistoryService,
            IOptions<BotCommandSettings> botCommandOptions,
            ITelegramNotificationService telegramNotificationService,
            TelegramNotificationFormatter telegramFormatter,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var rawText = request.Text?.Trim();
            var chatId = request.ExternalChatId?.Trim();

            if (string.IsNullOrWhiteSpace(chatId))
            {
                return Results.Ok();
            }

            var isGroup = string.Equals(request.ChatType, "group", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.ChatType, "supergroup", StringComparison.OrdinalIgnoreCase);
            var isSlashCommand = !string.IsNullOrWhiteSpace(rawText) && rawText.StartsWith('/');
            var isAddressedToBot = isSlashCommand || request.IsBotMentioned || request.IsReplyToBot;
            var originalMessage = request.OriginalText?.Trim();
            var conversation = new TelegramConversationHistorySnapshot(false, []);

            if (isGroup && !string.IsNullOrWhiteSpace(originalMessage ?? rawText))
            {
                try
                {
                    conversation = await conversationHistoryService.RecordIncomingMessageAsync(
                        chatId,
                        request.MessageId,
                        request.SenderName ?? request.ExternalUserId,
                        originalMessage ?? rawText!,
                        cancellationToken);

                    if (conversation.IsDuplicate)
                    {
                        return Results.Ok();
                    }
                }
                catch
                {
                    // Conversation history is best-effort and must not block bot commands.
                }
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return Results.Ok();
            }

            if (isGroup && !isAddressedToBot)
            {
                return Results.Ok();
            }

            // Explicit group mentions and replies are conversational by default.
            // Slash commands remain executable even when sent as a reply to the bot.
            var isExplicitConversation = isGroup
                && (request.IsBotMentioned || request.IsReplyToBot)
                && !rawText.StartsWith('/');

            if (isExplicitConversation)
            {
                using var chatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                chatCts.CancelAfter(TimeSpan.FromSeconds(30));

                try
                {
                    var chatReply = await botConversationalResponder.RespondAsync(
                        BuildConversationPrompt(conversation.RecentMessages, rawText),
                        botCommandOptions.Value.ConversationalFallbackSystemPrompt,
                        chatCts.Token);

                    if (!string.IsNullOrWhiteSpace(chatReply))
                    {
                        await telegramNotificationService.SendToChatAsync(chatId, chatReply, cancellationToken);
                        await RecordAssistantMessageBestEffortAsync(
                            conversationHistoryService, isGroup, chatId, chatReply, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    await telegramNotificationService.SendToChatAsync(
                        chatId,
                        "I could not complete that response in time. Please try again.",
                        CancellationToken.None);
                }
                catch (Exception)
                {
                    await telegramNotificationService.SendToChatAsync(
                        chatId,
                        "I could not generate a response right now. Please try again.",
                        CancellationToken.None);
                }

                return Results.Ok();
            }

            // If not a slash command, use AI intent resolution
            var isNaturalLanguage = !rawText.StartsWith('/');
            if (isNaturalLanguage)
            {
                using var aiCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                aiCts.CancelAfter(TimeSpan.FromSeconds(10));

                try
                {
                    var resolvedText = await botCommandIntentResolver.ResolveCommandTextAsync(
                        rawText, aiCts.Token);
                    if (!string.IsNullOrWhiteSpace(resolvedText))
                    {
                        rawText = resolvedText;
                    }
                    else if (botCommandOptions.Value.EnableConversationalFallback)
                    {
                        // AI confidently found no matching command — chat instead of
                        // letting the parser report "Unknown command".
                        var chatReply = await botConversationalResponder.RespondAsync(
                            BuildConversationPrompt(conversation.RecentMessages, rawText),
                            botCommandOptions.Value.ConversationalFallbackSystemPrompt,
                            cancellationToken);
                        if (!string.IsNullOrWhiteSpace(chatReply))
                        {
                            await telegramNotificationService.SendToChatAsync(chatId, chatReply, cancellationToken);
                            await RecordAssistantMessageBestEffortAsync(
                                conversationHistoryService, isGroup, chatId, chatReply, cancellationToken);
                        }

                        return Results.Ok();
                    }
                }
                catch (OperationCanceledException)
                {
                    // AI timed out — parser handles raw text
                }
            }

            var originalUser = httpContext.User;

            try
            {
                var mappedUser = await userMappingService.ResolveAsync("Telegram", request.ExternalUserId, cancellationToken);
                if (mappedUser is not null)
                {
                    httpContext.User = SlackEndpoints.BuildPrincipal(mappedUser);
                }

                var result = await mediator.Send(new ExecuteBotCommandCommand
                {
                    RawText = rawText!,
                    Channel = "Telegram",
                    ExternalChannelId = request.ExternalChatId
                }, cancellationToken);

                // Fallback: if /pc query failed, try /ps (handles generic "lấy giá VNM" where AI defaults to crypto)
                if (result.Status != BotCommandExecutionStatus.Completed
                    && rawText.StartsWith("/pc ", StringComparison.Ordinal))
                {
                    var fallbackText = rawText.Replace("/pc ", "/ps ");
                    result = await mediator.Send(new ExecuteBotCommandCommand
                    {
                        RawText = fallbackText,
                        Channel = "Telegram",
                        ExternalChannelId = request.ExternalChatId
                    }, cancellationToken);
                }

                // AI narration is only performed for natural language messages, not direct slash commands.
                if (isNaturalLanguage)
                {
                    result = await botCommandResponseNarrator.NarrateAsync(
                        result,
                        new BotCommandNarrationOptions
                        {
                            Enabled = botCommandOptions.Value.EnableAiNarration,
                            MaxSourceChars = botCommandOptions.Value.AiNarrationMaxSourceChars,
                            SystemPrompt = botCommandOptions.Value.AiNarrationSystemPrompt
                        },
                        cancellationToken);
                }

                var responseText = FormatResponseText(telegramFormatter, result);
                await telegramNotificationService.SendRenderedToChatAsync(chatId, responseText, cancellationToken);
                await RecordAssistantMessageBestEffortAsync(
                    conversationHistoryService, isGroup, chatId, responseText, cancellationToken);
            }
            catch (Exception)
            {
                await telegramNotificationService.SendToChatAsync(
                    chatId,
                    "Command processing failed. Please try again.",
                    CancellationToken.None);
            }
            finally
            {
                httpContext.User = originalUser;
            }

            return Results.Ok();
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.BotCommandExecute)
        .WithName("ExecuteTelegramWebhook")
        .WithDescription("Execute a Telegram bot command through the unified command engine.");
    }

    private static async Task RecordAssistantMessageBestEffortAsync(
        ITelegramConversationHistoryService historyService,
        bool isGroup,
        string chatId,
        string content,
        CancellationToken cancellationToken)
    {
        if (!isGroup)
        {
            return;
        }

        try
        {
            await historyService.RecordAssistantMessageAsync(chatId, content, cancellationToken);
        }
        catch
        {
            // Conversation history is best-effort and must not block bot responses.
        }
    }

    private static string BuildConversationPrompt(
        IReadOnlyList<TelegramConversationMessage> recentMessages,
        string currentMessage)
    {
        if (recentMessages.Count == 0)
        {
            return currentMessage;
        }

        var context = string.Join(
            "\n",
            recentMessages.Select(x => $"{x.SenderName}: {x.Content}"));

        return $"Recent group conversation (oldest first):\n{context}\n\nCurrent message:\n{currentMessage}";
    }

    private static string FormatResponseText(TelegramNotificationFormatter formatter, BotCommandExecutionDto result)
    {
        if (result.Card is not null)
        {
            return formatter.Format(result.Card);
        }

        if (!string.IsNullOrWhiteSpace(result.PlainText))
        {
            return result.PlainText;
        }

        return result.Status == BotCommandExecutionStatus.Completed
            ? "Command executed."
            : "Command failed.";
    }
}

public sealed record ExecuteTelegramWebhookRequest
{
    public string? Text { get; init; }
    public string? OriginalText { get; init; }
    public string? ChatType { get; init; }
    public long? MessageId { get; init; }
    public string? SenderName { get; init; }
    public bool IsBotMentioned { get; init; }
    public bool IsReplyToBot { get; init; }
    public required string ExternalUserId { get; init; }
    public required string ExternalChatId { get; init; }
}
