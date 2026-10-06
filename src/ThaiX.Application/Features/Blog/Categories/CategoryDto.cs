namespace ThaiX.Application.Features.Blog.Categories;

public sealed record CategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}
