using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Categories.Commands.UpdateCategory;

[InvalidateCache(CacheGroups.BlogCategories)]
public sealed record UpdateCategoryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}
