namespace ThaiX.Application.Features.Identity.Dtos;

/// <summary>
/// DTO for suggesting users to link to a contact (e.g. by matching phone number).
/// </summary>
public sealed record SuggestedUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}
