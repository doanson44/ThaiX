using System.Text.Json;

namespace ThaiX.Application.Features.Resumes;

public sealed record ResumeDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string FullName { get; init; }
    public required string Headline { get; init; }
    public string? MetaDescription { get; init; }
    public required bool IsPublished { get; init; }
    public required ResumeContentDto Content { get; init; }
}

/// <summary>
/// Structured resume content, serialized as a single JSON document on ResumeProfile.ContentJson.
/// Dates are free text (e.g. "2022-01", "Present") since resumes commonly show fuzzy ranges.
/// </summary>
public sealed record ResumeContentDto
{
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<ResumeExperienceDto> Experience { get; init; } = [];
    public IReadOnlyList<ResumeProjectDto> Projects { get; init; } = [];
    public IReadOnlyList<ResumeEducationDto> Education { get; init; } = [];
    public IReadOnlyList<ResumeLinkDto> Links { get; init; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static ResumeContentDto FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ResumeContentDto();
        }

        return JsonSerializer.Deserialize<ResumeContentDto>(json, JsonOptions) ?? new ResumeContentDto();
    }
}

public sealed record ResumeExperienceDto
{
    public string Company { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string StartDate { get; init; } = string.Empty;
    public string? EndDate { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed record ResumeProjectDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? TechStack { get; init; }
    public string? LinkUrl { get; init; }
}

public sealed record ResumeEducationDto
{
    public string School { get; init; } = string.Empty;
    public string Degree { get; init; } = string.Empty;
    public string StartDate { get; init; } = string.Empty;
    public string? EndDate { get; init; }
    public string? Description { get; init; }
}

public sealed record ResumeLinkDto
{
    public string Label { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
