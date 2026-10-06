namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for expense tag.
/// </summary>
public sealed record ExpenseTagDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
}
