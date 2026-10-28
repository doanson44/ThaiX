using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;
using ThaiX.Presentation.Resources;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Thin passthrough endpoints to IAiTextGenerationService for blog writing assistance and
/// pre-publish review — mirrors the style of ResumeAiEndpoints.cs/TradingEndpoints.cs. No
/// MediatR/CQRS here since none of these touch the database; they're stateless
/// prompt-building + a single AI call each.
/// </summary>
public static class BlogAiEndpoints
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(25);

    public static void MapBlogAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/blog/ai")
            .WithTags("Blog")
            .RequireAuthorization(Permissions.BlogWrite);

        group.MapPost("/generate-title", async (
            GenerateTitleRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.GenerateTitle.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.GenerateTitle.User", request.ContentHtml);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("GenerateBlogTitle")
        .WithDescription("Suggests 3 title options based on the article content.");

        group.MapPost("/generate-summary", async (
            GenerateSummaryRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.GenerateSummary.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.GenerateSummary.User", request.Title, request.ContentHtml);
            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("GenerateBlogSummary")
        .WithDescription("Synthesizes a short summary/excerpt from the article content.");

        group.MapPost("/rewrite", async (
            RewriteTextRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var text = request.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Results.Ok(BuildTextResponse(text, httpContext));
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Format("Blog.Rewrite.System", request.Tone).WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.Rewrite.User", text);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("RewriteBlogText")
        .WithDescription("Rewrites the given text in a target tone (Professional/Casual/Persuasive/Technical).");

        group.MapPost("/improve-grammar", async (
            ImproveGrammarRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var text = request.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Results.Ok(BuildTextResponse(text, httpContext));
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.ImproveGrammar.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.ImproveGrammar.User", text);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("ImproveBlogGrammar")
        .WithDescription("Fixes spelling/grammar/tense issues in the given text.");

        group.MapPost("/generate-tags", async (
            GenerateTagsRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.GenerateTags.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.GenerateTags.User", request.Title, request.ContentHtml);
            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("GenerateBlogTags")
        .WithDescription("Suggests tags for the article as a comma-separated list.");

        group.MapPost("/generate-seo", async (
            GenerateSeoRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.GenerateSeo.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format("Blog.GenerateSeo.User", request.Title, request.Summary);
            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);

            var (metaTitle, metaDescription, keywords) = ParseSeoResponse(result.Text);

            var response = ApiResponse<GenerateSeoResponse>.SuccessResult(new GenerateSeoResponse
            {
                MetaTitle = metaTitle,
                MetaDescription = metaDescription,
                Keywords = keywords
            });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .WithName("GenerateBlogSeo")
        .WithDescription("Generates SEO title, meta description, and keywords from title/summary.");

        // POST /api/blog/ai/review — decisive pre-publish review (READY TO PUBLISH / NEEDS WORK)
        group.MapPost("/review", async (
            ReviewPostRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Blog.Review.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format(
                "Blog.Review.User",
                request.Title,
                request.Summary,
                request.CategoryName ?? "(none)",
                request.MetaTitle ?? "(none)",
                request.MetaDescription ?? "(none)",
                request.ContentHtml);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);

            var lines = result.Text.Trim().Split('\n', 2);
            var firstLine = lines[0].Trim().ToUpperInvariant();
            var verdict = firstLine.Contains("NEEDS WORK") ? "NEEDS WORK"
                : firstLine.Contains("READY TO PUBLISH") ? "READY TO PUBLISH"
                : "REVIEW";
            var report = lines.Length > 1 ? lines[1].Trim() : result.Text.Trim();

            var response = ApiResponse<ReviewPostResponse>.SuccessResult(new ReviewPostResponse
            {
                Verdict = verdict,
                Report = report
            });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .WithName("ReviewBlogPost")
        .WithDescription("Decisive AI pre-publish review (READY TO PUBLISH or NEEDS WORK) with itemized issues.");
    }

    private static ApiResponse<BlogAiTextResponse> BuildTextResponse(string text, HttpContext httpContext)
    {
        var response = ApiResponse<BlogAiTextResponse>.SuccessResult(new BlogAiTextResponse { Text = text.Trim() });
        response.Metadata.CorrelationId = httpContext.GetCorrelationId();
        return response;
    }

    private static (string? MetaTitle, string? MetaDescription, string? Keywords) ParseSeoResponse(string text)
    {
        string? metaTitle = null;
        string? metaDescription = null;
        string? keywords = null;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                metaTitle = line[6..].Trim();
            }
            else if (line.StartsWith("DESCRIPTION:", StringComparison.OrdinalIgnoreCase))
            {
                metaDescription = line[12..].Trim();
            }
            else if (line.StartsWith("KEYWORDS:", StringComparison.OrdinalIgnoreCase))
            {
                keywords = line[9..].Trim();
            }
        }

        return (metaTitle, metaDescription, keywords);
    }
}

public sealed record GenerateTitleRequest
{
    public required string ContentHtml { get; init; }
}

public sealed record GenerateSummaryRequest
{
    public required string Title { get; init; }
    public required string ContentHtml { get; init; }
}

public sealed record RewriteTextRequest
{
    public string? Text { get; init; }
    public required string Tone { get; init; }
}

public sealed record ImproveGrammarRequest
{
    public string? Text { get; init; }
}

public sealed record GenerateTagsRequest
{
    public required string Title { get; init; }
    public required string ContentHtml { get; init; }
}

public sealed record GenerateSeoRequest
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
}

public sealed record GenerateSeoResponse
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? Keywords { get; init; }
}

public sealed record ReviewPostRequest
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string ContentHtml { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? CategoryName { get; init; }
}

public sealed record ReviewPostResponse
{
    public string Verdict { get; init; } = string.Empty;
    public string Report { get; init; } = string.Empty;
}

public sealed record BlogAiTextResponse
{
    public string Text { get; init; } = string.Empty;
}
