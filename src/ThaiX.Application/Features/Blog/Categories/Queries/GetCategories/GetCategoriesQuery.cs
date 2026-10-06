using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Categories.Queries.GetCategories;

/// <summary>
/// Flat lookup list of all categories — no paging needed (categories are cheap and few).
/// </summary>
public sealed record GetCategoriesQuery : IAppQuery<IReadOnlyList<CategoryDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.Blog.CategoriesListKey;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    public string CacheGroup => CacheGroups.BlogCategories;
}
