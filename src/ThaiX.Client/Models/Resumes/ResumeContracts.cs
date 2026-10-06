namespace ThaiX.Client.Models.Resumes;

public sealed class ResumeDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Headline { get; init; } = string.Empty;
    public string? MetaDescription { get; init; }
    public bool IsPublished { get; init; }
    public ResumeContentDto Content { get; init; } = new();
}

public sealed class ResumeContentDto
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<ResumeExperienceDto> Experience { get; set; } = [];
    public List<ResumeProjectDto> Projects { get; set; } = [];
    public List<ResumeEducationDto> Education { get; set; } = [];
    public List<ResumeLinkDto> Links { get; set; } = [];
}

public sealed class ResumeExperienceDto
{
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime? StartDateValue
    {
        get => DateTime.TryParse(StartDate, out var d) ? d : null;
        set => StartDate = value?.ToString("yyyy-MM-dd") ?? "";
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime? EndDateValue
    {
        get => string.IsNullOrWhiteSpace(EndDate) ? null :
              DateTime.TryParse(EndDate, out var d) ? d : null;
        set => EndDate = value?.ToString("yyyy-MM-dd");
    }
}

public sealed class ResumeProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TechStack { get; set; }
    public string? LinkUrl { get; set; }
}

public sealed class ResumeEducationDto
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
    public string? Description { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime? StartDateValue
    {
        get => DateTime.TryParse(StartDate, out var d) ? d : null;
        set => StartDate = value?.ToString("yyyy-MM-dd") ?? "";
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime? EndDateValue
    {
        get => string.IsNullOrWhiteSpace(EndDate) ? null :
              DateTime.TryParse(EndDate, out var d) ? d : null;
        set => EndDate = value?.ToString("yyyy-MM-dd");
    }
}

public sealed class ResumeLinkDto
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class UpsertResumeRequest
{
    public string Slug { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public bool IsPublished { get; set; }
    public ResumeContentDto Content { get; set; } = new();
}

public sealed class PolishResumeTextRequest
{
    public string Text { get; set; } = string.Empty;
    public string? FieldContext { get; set; }
}

public sealed class GenerateResumeSummaryRequest
{
    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<ResumeExperienceDto> Experience { get; set; } = [];
    public List<ResumeProjectDto> Projects { get; set; } = [];
    public List<ResumeEducationDto> Education { get; set; } = [];
}

public sealed class OptimizeResumeForJobRequest
{
    public string JobDescription { get; set; } = string.Empty;
    public ResumeContentDto Content { get; set; } = new();
}

public sealed class CheckResumeConsistencyRequest
{
    public ResumeContentDto Content { get; set; } = new();
}

public sealed class ResumeAiTextResponse
{
    public string Text { get; init; } = string.Empty;
}

public sealed class ResumeAiReportResponse
{
    public string Report { get; init; } = string.Empty;
}
