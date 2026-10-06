using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Categories.Commands.CreateCategory;

[InvalidateCache(CacheGroups.BlogCategories)]
public sealed record CreateCategoryCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}
