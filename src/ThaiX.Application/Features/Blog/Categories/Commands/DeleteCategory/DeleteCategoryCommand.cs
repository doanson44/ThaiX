using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Categories.Commands.DeleteCategory;

[InvalidateCache(CacheGroups.BlogCategories)]
public sealed record DeleteCategoryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
