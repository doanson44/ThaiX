using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPostById;

public sealed class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPostByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostDto?> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PostDto
            {
                Id = p.Id,
                AuthorId = p.AuthorId,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary,
                ContentHtml = p.ContentHtml,
                FeaturedImageUrl = p.FeaturedImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category == null
                    ? null
                    : p.Category.Name,
                Status = p.Status,
                PublishedAt = p.PublishedAt,
                ScheduledAt = p.ScheduledAt,
                MetaTitle = p.MetaTitle,
                MetaDescription = p.MetaDescription,
                ReadTimeMinutes = p.ReadTimeMinutes,
                TagIds = p.PostTags
                    .OrderBy(pt => pt.Tag.Name)
                    .Select(pt => pt.TagId)
                    .ToList(),
                TagNames = p.PostTags
                    .OrderBy(pt => pt.Tag.Name)
                    .Select(pt => pt.Tag.Name)
                    .ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}