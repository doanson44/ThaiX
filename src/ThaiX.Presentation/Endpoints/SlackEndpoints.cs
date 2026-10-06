using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Queries.GetBotCommandExecutionStatus;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services.Notifications;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public sealed record ChannelInfoDto(string Id, string Name);

public static class SlackEndpoints
{
    public static void MapSlackEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/slack")
            .WithTags("Slack");

        group.MapGet("/commands/channels", (
            IOptions<SlackSettings> slackOptions,
            HttpContext httpContext) =>
        {
            var channels = slackOptions.Value.Channels
                .SelectMany(group => group.Value, (group, kv) => new { Group = group.Key, UserKey = kv.Key, ChannelId = kv.Value })
                .Where(x => !string.IsNullOrWhiteSpace(x.ChannelId))
                .Select(x => new ChannelInfoDto(
                    x.ChannelId.Trim(),
                    $"{x.Group}/{x.UserKey} ({x.ChannelId.Trim()})"))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var response = ApiResponse<IReadOnlyList<ChannelInfoDto>>.SuccessResult(channels);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("GetSlackCommandChannels")
        .WithDescription("Get configured Slack channels available for command testing.");

        group.MapPost("/commands/execute", async (
            [FromBody] ExecuteSlackBotCommandRequest request,
            IMediator mediator,
            IBotUserMappingService userMappingService,
            IBotCommandIntentResolver botCommandIntentResolver,
            IBotCommandResponseNarrator botCommandResponseNarrator,
            IBotConversationalResponder botConversationalResponder,
            IOptions<BotCommandSettings> botCommandOptions,
            ISlackNotificationService slackNotificationService,
            SlackNotificationFormatter slackFormatter,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var rawText = BuildRawCommandText(request);
            var isMention = IsMentionTextRequest(request);
            var originalUser = httpContext.User;

            try
            {
                // AI intent resolution for natural language (mention text without slash command)
                if (isMention)
                {
                    using var aiCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    aiCts.CancelAfter(TimeSpan.FromSeconds(10));

                    try
                    {
                        var resolvedText = await botCommandIntentResolver.ResolveCommandTextAsync(
                            request.Text ?? string.Empty,
                            aiCts.Token);

                        if (!string.IsNullOrWhiteSpace(resolvedText))
                        {
                            rawText = resolvedText;
                        }
                        else if (botCommandOptions.Value.EnableConversationalFallback)
                        {
                            // AI confidently found no matching command — chat instead of
                            // letting the parser report "Unknown command".
                            var chatReply = await botConversationalResponder.RespondAsync(
                                request.Text ?? string.Empty,
                                botCommandOptions.Value.ConversationalFallbackSystemPrompt,
                                cancellationToken);

                            if (!string.IsNullOrWhiteSpace(chatReply))
                            {
                                if (!string.IsNullOrWhiteSpace(request.ResponseUrl))
                                {
                                    await slackNotificationService.SendToResponseUrlAsync(
                                        request.ResponseUrl, chatReply, cancellationToken);
                                }
                                else
                                {
                                    await slackNotificationService.SendToChannelAsync(
                                        request.ExternalChannelId, chatReply, request.ThreadTs, cancellationToken);
                                }
                            }

                            return Results.Ok();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // AI timed out — fall through, parser handles raw text
                    }
                }

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    return Results.Ok();
                }

                var mappedUser = await userMappingService.ResolveAsync("Slack", request.ExternalUserId, cancellationToken);
                if (mappedUser is not null)
                {
                    httpContext.User = BuildPrincipal(mappedUser);
                }

                var result = await mediator.Send(new ExecuteBotCommandCommand
                {
                    RawText = rawText,
                    Channel = "Slack",
                    ExternalChannelId = request.ExternalChannelId
                }, cancellationToken);

                // Fallback: if /pc query failed, try /ps (handles generic "lấy giá VNM" where AI defaults to crypto)
                if (result.Status != BotCommandExecutionStatus.Completed
                    && rawText.StartsWith("/pc ", StringComparison.Ordinal))
                {
                    var fallbackText = rawText.Replace("/pc ", "/ps ");
                    result = await mediator.Send(new ExecuteBotCommandCommand
                    {
                        RawText = fallbackText,
                        Channel = "Slack",
                        ExternalChannelId = request.ExternalChannelId
                    }, cancellationToken);
                }

                // AI narration is only performed for natural language mentions, not direct slash commands.
                if (isMention)
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

                var responseText = FormatResponseText(slackFormatter, result);

                if (!string.IsNullOrWhiteSpace(request.ResponseUrl))
                {
                    await slackNotificationService.SendToResponseUrlAsync(request.ResponseUrl, responseText, cancellationToken);
                }
                else
                {
                    await slackNotificationService.SendToChannelAsync(
                        request.ExternalChannelId,
                        responseText,
                        request.ThreadTs,
                        cancellationToken);
                }
            }
            catch (Exception)
            {
                // Log at infrastructure level — silently succeed for fire-and-forget
            }
            finally
            {
                httpContext.User = originalUser;
            }

            return Results.Ok();
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.BotCommandExecute)
        .WithName("ExecuteSlackBotCommand")
        .WithDescription("Execute bot command text through the unified command engine.");

        group.MapGet("/commands/executions/{executionId}", async (
            string executionId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var status = await mediator.Send(new GetBotCommandExecutionStatusQuery
            {
                ExecutionId = executionId
            }, cancellationToken);

            if (status is null)
            {
                var notFound = ApiResponse<BotAsyncExecutionStatusDto>.ErrorResult(
                    ErrorCodes.RESOURCE_NOT_FOUND,
                    "Execution was not found.");
                notFound.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.NotFound(notFound);
            }

            var response = ApiResponse<BotAsyncExecutionStatusDto>.SuccessResult(status);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.BotCommandExecute)
        .WithName("GetSlackBotCommandExecutionStatus")
        .WithDescription("Get async execution status for a queued bot command.");
    }

    private static string BuildRawCommandText(ExecuteSlackBotCommandRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RawText))
        {
            return request.RawText.Trim();
        }

        var command = request.Command?.Trim() ?? string.Empty;
        var text = request.Text?.Trim() ?? string.Empty;
        var combined = string.IsNullOrWhiteSpace(text)
            ? command
            : $"{command} {text}";

        return combined.Trim();
    }

    private static bool IsMentionTextRequest(ExecuteSlackBotCommandRequest request)
    {
        return string.IsNullOrWhiteSpace(request.RawText)
               && string.IsNullOrWhiteSpace(request.Command)
               && !string.IsNullOrWhiteSpace(request.Text);
    }

    private static string FormatResponseText(SlackNotificationFormatter formatter, BotCommandExecutionDto result)
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

    internal static ClaimsPrincipal BuildPrincipal(BotMappedUser mappedUser)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, mappedUser.UserId.ToString()),
            new(ClaimTypes.Email, mappedUser.Email)
        };

        claims.AddRange(mappedUser.Permissions.Select(permission =>
            new Claim(ClaimTypeConstants.Permission, permission)));

        var identity = new ClaimsIdentity(claims, "SlackBot");
        return new ClaimsPrincipal(identity);
    }

}


public sealed record ExecuteSlackBotCommandRequest
{
    public string? RawText { get; init; }
    public string? Command { get; init; }
    public string? Text { get; init; }
    public required string ExternalUserId { get; init; }
    public required string ExternalChannelId { get; init; }
    public string? ThreadTs { get; init; }
    public string? ResponseUrl { get; init; }
}
