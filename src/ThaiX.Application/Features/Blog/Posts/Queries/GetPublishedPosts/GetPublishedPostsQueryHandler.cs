using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPublishedPosts;

public sealed class GetPublishedPostsQueryHandler : IRequestHandler<GetPublishedPostsQuery, PagedResult<PublishedPostListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublishedPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PublishedPostListItemDto>> Handle(
        GetPublishedPostsQuery request,
        CancellationToken cancellationToken)
    {
        var predicate = PredicateExtensions.True<Post>()
            .And(p => p.Status == PostStatus.Published);

        if (request.CategoryId.HasValue)
        {
            predicate = predicate.And(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.TagId.HasValue)
        {
            var tagId = request.TagId.Value;

            predicate = predicate.And(p =>
                _context.PostTags
                    .AsNoTracking()
                    .Any(pt => pt.PostId == p.Id && pt.TagId == tagId));
        }

        var projected = _context.Posts
            .AsNoTracking()
            .Where(predicate)
            .OrderByDescending(p => p.PublishedAt)
            .Select(post => new PublishedPostListItemDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                FeaturedImageUrl = post.FeaturedImageUrl,
                CategoryName = post.Category == null
                    ? null
                    : post.Category.Name,
                ReadTimeMinutes = post.ReadTimeMinutes,
                PublishedAt = post.PublishedAt
            });

        return await projected.ToPagedListAsync(request, cancellationToken);
    }
}