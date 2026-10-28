using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services.Notifications;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;
using ThaiX.Presentation.Resources;

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
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(TelegramEndpoints));
            var rawText = request.Text?.Trim();
            var originalUserMessage = rawText;
            var wasNaturalLanguageInput = !string.IsNullOrWhiteSpace(rawText) && !rawText.StartsWith('/');
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
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to record incoming Telegram conversation message. CorrelationId={CorrelationId}, MessageId={MessageId}",
                        httpContext.GetCorrelationId(),
                        request.MessageId);
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

            // For explicit mentions and replies, resolve supported actions before falling back to open chat.
            // The resolver uses conversation history only to resolve references in the current message.
            var isExplicitConversation = isGroup
                && (request.IsBotMentioned || request.IsReplyToBot)
                && !rawText.StartsWith('/');

            if (isExplicitConversation)
            {
                string? resolvedCommand = null;
                using (var intentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    intentCts.CancelAfter(TimeSpan.FromSeconds(10));

                    try
                    {
                        resolvedCommand = await botCommandIntentResolver.ResolveCommandTextAsync(
                            BuildConversationPrompt(conversation.RecentMessages, rawText),
                            intentCts.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.LogWarning(
                            ex,
                            "Telegram conversation intent resolution timed out; falling back to conversation response. CorrelationId={CorrelationId}, MessageId={MessageId}",
                            httpContext.GetCorrelationId(),
                            request.MessageId);
                        // Fall back to conversation when intent resolution times out.
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(
                            ex,
                            "Telegram bot intent resolution failed; falling back to conversation response. CorrelationId={CorrelationId}, MessageId={MessageId}",
                            httpContext.GetCorrelationId(),
                            request.MessageId);
                        // Intent resolution is best-effort; open conversation remains available.
                    }
                }

                if (!string.IsNullOrWhiteSpace(resolvedCommand))
                {
                    rawText = resolvedCommand;
                }
                else
                {
                    using var chatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    chatCts.CancelAfter(TimeSpan.FromSeconds(30));

                    try
                    {
                        var chatReply = await botConversationalResponder.RespondAsync(
                            BuildConversationPrompt(conversation.RecentMessages, originalUserMessage ?? rawText),
                            botCommandOptions.Value.ConversationalFallbackSystemPrompt,
                            chatCts.Token);

                        if (!string.IsNullOrWhiteSpace(chatReply))
                        {
                            await telegramNotificationService.SendToChatAsync(chatId, chatReply, cancellationToken);
                            await RecordAssistantMessageBestEffortAsync(
                                conversationHistoryService, isGroup, chatId, chatReply, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.LogWarning(
                            ex,
                            "Telegram conversational response timed out or was canceled. CorrelationId={CorrelationId}, MessageId={MessageId}",
                            httpContext.GetCorrelationId(),
                            request.MessageId);
                        await telegramNotificationService.SendToChatAsync(
                            chatId,
                            "I could not complete that response in time. Please try again.",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to generate Telegram conversational response. CorrelationId={CorrelationId}, MessageId={MessageId}",
                            httpContext.GetCorrelationId(),
                            request.MessageId);
                        await telegramNotificationService.SendToChatAsync(
                            chatId,
                            "I could not generate a response right now. Please try again.",
                            CancellationToken.None);
                    }

                    return Results.Ok();
                }
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
                catch (OperationCanceledException ex)
                {
                    logger.LogWarning(
                        ex,
                        "Telegram bot intent resolution timed out; parser will handle raw text. CorrelationId={CorrelationId}, MessageId={MessageId}",
                        httpContext.GetCorrelationId(),
                        request.MessageId);
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
                if (wasNaturalLanguageInput)
                {
                    result = await botCommandResponseNarrator.NarrateAsync(
                        result,
                        new BotCommandNarrationOptions
                        {
                            Enabled = botCommandOptions.Value.EnableAiNarration,
                            MaxSourceChars = botCommandOptions.Value.AiNarrationMaxSourceChars,
                            SystemPrompt = botCommandOptions.Value.AiNarrationSystemPrompt,
                            UserMessage = originalUserMessage ?? string.Empty
                        },
                        cancellationToken);
                }

                var responseText = wasNaturalLanguageInput
                    && botCommandOptions.Value.EnableAiNarration
                    && result.Status == BotCommandExecutionStatus.Completed
                    && !string.IsNullOrWhiteSpace(result.PlainText)
                        ? result.PlainText
                        : FormatResponseText(telegramFormatter, result);
                await telegramNotificationService.SendRenderedToChatAsync(chatId, responseText, cancellationToken);
                await RecordAssistantMessageBestEffortAsync(
                    conversationHistoryService, isGroup, chatId, responseText, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Telegram command processing failed. CorrelationId={CorrelationId}, MessageId={MessageId}, ChatType={ChatType}, IsNaturalLanguageInput={IsNaturalLanguageInput}, IsSlashCommand={IsSlashCommand}",
                    httpContext.GetCorrelationId(),
                    request.MessageId,
                    request.ChatType,
                    wasNaturalLanguageInput,
                    isSlashCommand);
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
            recentMessages.Select(x => AiPromptResources.Format("Telegram.ConversationMessage", x.SenderName, x.Content)));

        return AiPromptResources.Format("Telegram.ConversationHistory", context, currentMessage);
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
