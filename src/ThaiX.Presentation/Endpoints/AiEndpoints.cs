using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ai")
            .WithTags("AI")
            .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin);

        group.MapPost("/generate", async (
            [FromBody] GenerateAiTextRequest request,
            IAiTextGenerationService aiTextGenerationService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var prompt = request.Prompt?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                var error = ApiResponse<GenerateAiTextResponse>.ErrorResult(
                    ErrorCodes.INVALID_REQUEST,
                    "Prompt is required.");
                error.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(error);
            }

            var result = await aiTextGenerationService.GenerateAsync(
                prompt,
                request.SystemPrompt?.Trim(),
                cancellationToken);

            var response = ApiResponse<GenerateAiTextResponse>.SuccessResult(new GenerateAiTextResponse
            {
                Provider = result.Provider,
                Model = result.Model,
                Text = result.Text
            });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .WithName("GenerateAiText")
        .WithDescription("Generate text from configured AI providers with automatic fallback.");
    }
}

public sealed record GenerateAiTextRequest
{
    public string Prompt { get; init; } = string.Empty;
    public string? SystemPrompt { get; init; }
}

public sealed record GenerateAiTextResponse
{
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}