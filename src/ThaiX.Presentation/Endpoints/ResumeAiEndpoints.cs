using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Resumes;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;
using ThaiX.Presentation.Resources;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Thin passthrough endpoints to IAiTextGenerationService for resume writing assistance —
/// mirrors the style of AiEndpoints.cs. No MediatR/CQRS here since none of these touch the
/// database; they're stateless prompt-building + a single AI call each.
///
/// Every system prompt below forbids inventing facts, numbers, or achievements not present
/// in the caller's own input. The AI may only rephrase existing content or give advisory
/// suggestions — this is a resume, and fabricated claims are a real integrity problem.
/// </summary>
public static class ResumeAiEndpoints
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(25);

    public static void MapResumeAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/resumes/ai")
            .WithTags("Resumes")
            .RequireAuthorization(Permissions.ResumeWrite);

        group.MapPost("/polish-text", async (
            PolishResumeTextRequest request,
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

            var systemPrompt = AiPromptResources.Get("Resume.Polish.System").WithCurrentCultureInstruction();
            var contextLine = string.IsNullOrWhiteSpace(request.FieldContext)
                ? string.Empty
                : AiPromptResources.Format("Resume.Polish.Context", request.FieldContext);

            var prompt = AiPromptResources.Format("Resume.Polish.User", contextLine, text);
            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("PolishResumeText")
        .WithDescription("Rewrites a single resume field's text for clarity and professionalism.");

        group.MapPost("/generate-summary", async (
            GenerateResumeSummaryRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Resume.GenerateSummary.System").WithCurrentCultureInstruction();

            var prompt = BuildResumeFactsPrompt(
                request.FullName, request.Headline, request.Skills, request.Experience, request.Projects, request.Education);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildTextResponse(result.Text, httpContext));
        })
        .WithName("GenerateResumeSummary")
        .WithDescription("Synthesizes a professional summary from the rest of the resume.");

        group.MapPost("/optimize-for-job", async (
            OptimizeResumeForJobRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Resume.OptimizeForJob.System").WithCurrentCultureInstruction();

            var resumeFacts = BuildResumeFactsPrompt(
                null, null, request.Content.Skills, request.Content.Experience, request.Content.Projects, null, request.Content.Summary);
            var prompt = AiPromptResources.Format("Resume.OptimizeForJob.User", request.JobDescription.Trim(), resumeFacts);

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);
            return Results.Ok(BuildReportResponse(result.Text, httpContext));
        })
        .WithName("OptimizeResumeForJob")
        .WithDescription("Advises which skills/keywords to emphasize for a given job description.");

        group.MapPost("/check-consistency", async (
            CheckResumeConsistencyRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            var systemPrompt = AiPromptResources.Get("Resume.CheckConsistency.System").WithCurrentCultureInstruction();

            var resumeFacts = BuildResumeFactsPrompt(
                null, null, request.Content.Skills, request.Content.Experience, request.Content.Projects,
                request.Content.Education, request.Content.Summary);

            var result = await ai.GenerateAsync(resumeFacts, systemPrompt, cts.Token);
            return Results.Ok(BuildReportResponse(result.Text, httpContext));
        })
        .WithName("CheckResumeConsistency")
        .WithDescription("Flags spelling, grammar, and tense-consistency issues across the resume.");
    }

    private static ApiResponse<ResumeAiTextResponse> BuildTextResponse(string text, HttpContext httpContext)
    {
        var response = ApiResponse<ResumeAiTextResponse>.SuccessResult(new ResumeAiTextResponse { Text = text.Trim() });
        response.Metadata.CorrelationId = httpContext.GetCorrelationId();
        return response;
    }

    private static ApiResponse<ResumeAiReportResponse> BuildReportResponse(string report, HttpContext httpContext)
    {
        var response = ApiResponse<ResumeAiReportResponse>.SuccessResult(new ResumeAiReportResponse { Report = report.Trim() });
        response.Metadata.CorrelationId = httpContext.GetCorrelationId();
        return response;
    }

    private static string BuildResumeFactsPrompt(
        string? fullName,
        string? headline,
        IReadOnlyList<string> skills,
        IReadOnlyList<ResumeExperienceDto> experience,
        IReadOnlyList<ResumeProjectDto> projects,
        IReadOnlyList<ResumeEducationDto>? education,
        string? summary = null)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(fullName)) sb.AppendLine(AiPromptResources.Format("Resume.Facts.Name", fullName));
        if (!string.IsNullOrWhiteSpace(headline)) sb.AppendLine(AiPromptResources.Format("Resume.Facts.Headline", headline));
        if (!string.IsNullOrWhiteSpace(summary)) sb.AppendLine(AiPromptResources.Format("Resume.Facts.Summary", summary));
        if (skills.Count > 0) sb.AppendLine(AiPromptResources.Format("Resume.Facts.Skills", string.Join(", ", skills)));

        if (experience.Count > 0)
        {
            sb.AppendLine(AiPromptResources.Get("Resume.Facts.ExperienceHeading"));
            foreach (var exp in experience)
            {
                var end = string.IsNullOrWhiteSpace(exp.EndDate) ? AiPromptResources.Get("Resume.Facts.Present") : exp.EndDate;
                sb.AppendLine(AiPromptResources.Format("Resume.Facts.ExperienceItem", exp.Position, exp.Company, exp.StartDate, end, exp.Description));
            }
        }

        if (projects.Count > 0)
        {
            sb.AppendLine(AiPromptResources.Get("Resume.Facts.ProjectsHeading"));
            foreach (var proj in projects)
            {
                sb.AppendLine(AiPromptResources.Format("Resume.Facts.ProjectItem", proj.Name, proj.Description));
            }
        }

        if (education is { Count: > 0 })
        {
            sb.AppendLine(AiPromptResources.Get("Resume.Facts.EducationHeading"));
            foreach (var edu in education)
            {
                var end = string.IsNullOrWhiteSpace(edu.EndDate) ? AiPromptResources.Get("Resume.Facts.Present") : edu.EndDate;
                sb.AppendLine(AiPromptResources.Format("Resume.Facts.EducationItem", edu.Degree, edu.School, edu.StartDate, end));
            }
        }

        return sb.ToString();
    }
}

public sealed record PolishResumeTextRequest
{
    public string? Text { get; init; }
    public string? FieldContext { get; init; }
}

public sealed record GenerateResumeSummaryRequest
{
    public string? FullName { get; init; }
    public string? Headline { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<ResumeExperienceDto> Experience { get; init; } = [];
    public IReadOnlyList<ResumeProjectDto> Projects { get; init; } = [];
    public IReadOnlyList<ResumeEducationDto> Education { get; init; } = [];
}

public sealed record OptimizeResumeForJobRequest
{
    public required string JobDescription { get; init; }
    public required ResumeContentDto Content { get; init; }
}

public sealed record CheckResumeConsistencyRequest
{
    public required ResumeContentDto Content { get; init; }
}

public sealed record ResumeAiTextResponse
{
    public string Text { get; init; } = string.Empty;
}

public sealed record ResumeAiReportResponse
{
    public string Report { get; init; } = string.Empty;
}
