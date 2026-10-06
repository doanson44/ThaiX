namespace ThaiX.Application.Features.Identity.Dtos;

/// <summary>
/// DTO for current user profile (header display).
/// </summary>
public sealed record UserProfileDto
{
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? ResumeSlug { get; init; }
}
