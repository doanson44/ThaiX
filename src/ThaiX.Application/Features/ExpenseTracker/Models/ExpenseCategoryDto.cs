using ThaiX.Domain.Aggregates.ExpenseTracker;
namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for expense category.
/// </summary>
public sealed record ExpenseCategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required CategoryType CategoryType { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
