using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class BotCommandEndpoints
{
    public static void MapBotCommandEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/bot-commands")
            .WithTags("Bot Commands");

        group.MapPost("/execute", async (
            [FromBody] ExecuteBotCommandTesterRequest request,
            IMediator mediator,
            IBotUserMappingService userMappingService,
            IBotCommandResponseNarrator botCommandResponseNarrator,
            IOptions<BotCommandSettings> botCommandOptions,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var rawText = (request.RawText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawText))
            {
                var response = ApiResponse<BotCommandExecutionDto>.ErrorResult(
                    ErrorCodes.INVALID_REQUEST,
                    "RawText is required.");
                response.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(response);
            }

            var originalUser = httpContext.User;

            try
            {
                // Resolve user mapping for the channel
                var mappedUser = await userMappingService.ResolveAsync(
                    request.Channel,
                    request.ExternalUserId,
                    cancellationToken);

                if (mappedUser is not null)
                {
                    httpContext.User = SlackEndpoints.BuildPrincipal(mappedUser);
                }

                var result = await mediator.Send(new ExecuteBotCommandCommand
                {
                    RawText = rawText,
                    Channel = request.Channel,
                    ExternalChannelId = request.ExternalChannelId
                }, cancellationToken);

                // Fallback: if /pc query failed, try /ps
                if (result.Status != BotCommandExecutionStatus.Completed
                    && rawText.StartsWith("/pc ", StringComparison.Ordinal))
                {
                    var fallbackText = rawText.Replace("/pc ", "/ps ");
                    result = await mediator.Send(new ExecuteBotCommandCommand
                    {
                        RawText = fallbackText,
                        Channel = request.Channel,
                        ExternalChannelId = request.ExternalChannelId ?? string.Empty
                    }, cancellationToken);
                }

                // AI narration is only performed for natural language queries, not direct slash commands.
                if (!rawText.StartsWith('/'))
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

                var apiResponse = ApiResponse<BotCommandExecutionDto>.SuccessResult(result);
                apiResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.Ok(apiResponse);
            }
            finally
            {
                httpContext.User = originalUser;
            }
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("ExecuteBotCommandTester")
        .WithDescription("Execute a bot command for testing purposes (Slack or Telegram). Returns the result directly.");
    }
}

public sealed record ExecuteBotCommandTesterRequest
{
    public required string RawText { get; init; }
    public required string Channel { get; init; }
    public required string ExternalUserId { get; init; }
    public string? ExternalChannelId { get; init; }
}
