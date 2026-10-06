using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

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

            var systemPrompt = (
                "You are a blog editor. Based on the article content below, suggest 3 concise, " +
                "compelling titles (under 70 characters each). Return ONLY the 3 titles, one per " +
                "line, no numbering, no preamble, no explanation.")
                .WithCurrentCultureInstruction();

            var result = await ai.GenerateAsync($"Article content:\n{request.ContentHtml}", systemPrompt, cts.Token);
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

            var systemPrompt = (
                "You are a blog editor. Write a concise 2-3 sentence summary/excerpt for this " +
                "article, based only on the content given. Return ONLY the summary text, no " +
                "preamble, quotes, or explanation.")
                .WithCurrentCultureInstruction();

            var prompt = $"Title: {request.Title}\n\nArticle content:\n{request.ContentHtml}";
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

            var systemPrompt = (
                $"You are a professional editor. Rewrite the given text in a {request.Tone} tone, " +
                "preserving HTML tags/formatting and every fact/claim exactly as given — never invent " +
                "new information. Return ONLY the rewritten text with no preamble, quotes, or " +
                "explanation.")
                .WithCurrentCultureInstruction();

            var result = await ai.GenerateAsync($"Text to rewrite:\n{text}", systemPrompt, cts.Token);
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

            var systemPrompt = (
                "You are a meticulous proofreader. Fix spelling, grammar, and tense-consistency " +
                "issues in the given text, preserving HTML tags/formatting and all facts/claims " +
                "exactly as given. Return ONLY the corrected text with no preamble, quotes, or " +
                "explanation.")
                .WithCurrentCultureInstruction();

            var result = await ai.GenerateAsync($"Text to correct:\n{text}", systemPrompt, cts.Token);
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

            var systemPrompt = (
                "You are a blog editor. Suggest 3-6 relevant tags for this article, based only on " +
                "the content given. Return ONLY a comma-separated list of tags, no preamble, no " +
                "numbering, no explanation.")
                .WithCurrentCultureInstruction();

            var prompt = $"Title: {request.Title}\n\nArticle content:\n{request.ContentHtml}";
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

            var systemPrompt = (
                "You are an SEO specialist. Based on the article title and summary below, produce " +
                "SEO metadata. Respond in EXACTLY this 3-line format (do not translate the labels " +
                "\"TITLE:\", \"DESCRIPTION:\", \"KEYWORDS:\" even if asked to respond in another " +
                "language below):\n" +
                "TITLE: <SEO title, under 60 characters>\n" +
                "DESCRIPTION: <meta description, under 155 characters>\n" +
                "KEYWORDS: <comma-separated keywords>")
                .WithCurrentCultureInstruction();

            var prompt = $"Title: {request.Title}\n\nSummary: {request.Summary}";
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

            var systemPrompt = (
                "You are a decisive editor-in-chief reviewing a blog post before it goes live. " +
                "You MUST commit to ONE clear verdict — never hedge, never say \"it depends\".\n\n" +
                "Respond in exactly this format:\n" +
                "Line 1: exactly the literal English text \"READY TO PUBLISH\" or \"NEEDS WORK\" and " +
                "nothing else — do NOT translate this line even if asked to respond in another " +
                "language below.\n" +
                "Line 2 onward: itemized concrete issues found — grammar, missing/weak SEO fields, " +
                "readability (paragraph length, missing headings), tag/category mismatch. If none, " +
                "say so plainly. Be direct — this is for a real publishing decision, not a " +
                "disclaimer-filled report.")
                .WithCurrentCultureInstruction();

            var prompt =
                $"Title: {request.Title}\n" +
                $"Summary: {request.Summary}\n" +
                $"Category: {request.CategoryName ?? "(none)"}\n" +
                $"Meta Title: {request.MetaTitle ?? "(none)"}\n" +
                $"Meta Description: {request.MetaDescription ?? "(none)"}\n\n" +
                $"Content:\n{request.ContentHtml}";

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
