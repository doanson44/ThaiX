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
            IOptions<BotCommandSettings> botCommandOptions,
            ITelegramNotificationService telegramNotificationService,
            TelegramNotificationFormatter telegramFormatter,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var rawText = request.Text?.Trim();
            var chatId = request.ExternalChatId?.Trim();

            if (string.IsNullOrWhiteSpace(rawText) || string.IsNullOrWhiteSpace(chatId))
            {
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
                            rawText, botCommandOptions.Value.ConversationalFallbackSystemPrompt, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(chatReply))
                        {
                            await telegramNotificationService.SendToChatAsync(chatId, chatReply, cancellationToken);
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
    public required string ExternalUserId { get; init; }
    public required string ExternalChatId { get; init; }
}
