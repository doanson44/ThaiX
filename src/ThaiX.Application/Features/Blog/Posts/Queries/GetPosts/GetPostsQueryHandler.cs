using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPosts;

public sealed class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, PagedResult<PostListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PostListItemDto>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var predicate = PredicateExtensions.True<Post>();

        if (request.Status.HasValue)
        {
            predicate = predicate.And(p => p.Status == request.Status.Value);
        }

        if (request.CategoryId.HasValue)
        {
            predicate = predicate.And(p => p.CategoryId == request.CategoryId.Value);
        }

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);

        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;

            predicate = predicate.And(p =>
                EF.Functions.Like(
                    EF.Functions.Collate(p.Title, collation),
                    pattern,
                    "\\"));
        }

        var query = _context.Posts
            .AsNoTracking()
            .Where(predicate);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "createdat"
            : request.SortBy.Trim().ToLowerInvariant();

        query = sortBy switch
        {
            "title" => request.SortDescending
                ? query.OrderByDescending(p => p.Title)
                : query.OrderBy(p => p.Title),

            "status" => request.SortDescending
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),

            "publishedat" => request.SortDescending
                ? query.OrderByDescending(p => p.PublishedAt)
                : query.OrderBy(p => p.PublishedAt),

            _ => request.SortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var projected = query.Select(post => new PostListItemDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Status = post.Status,
            CategoryName = post.Category == null
                ? null
                : post.Category.Name,
            PublishedAt = post.PublishedAt,
            ScheduledAt = post.ScheduledAt,
            ReadTimeMinutes = post.ReadTimeMinutes,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        });

        return await projected.ToPagedListAsync(request, cancellationToken);
    }
}