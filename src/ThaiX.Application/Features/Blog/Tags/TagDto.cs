namespace ThaiX.Application.Features.Blog.Tags;

public sealed record TagDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
}
