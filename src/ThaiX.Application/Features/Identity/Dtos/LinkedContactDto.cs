namespace ThaiX.Application.Features.Identity.Dtos;

/// <summary>
/// DTO for contact linked to a user (for display and unlink).
/// </summary>
public sealed record LinkedContactDto
{
    public required Guid ContactId { get; init; }
    public required string DisplayName { get; init; }
}
