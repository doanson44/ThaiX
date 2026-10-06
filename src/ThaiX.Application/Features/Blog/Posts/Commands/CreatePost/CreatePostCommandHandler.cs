using Ganss.Xss;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Commands.CreatePost;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Guid>
{
    private static readonly HtmlSanitizer Sanitizer = new();

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePostCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().Trim('/').ToLowerInvariant();
        var slugTaken = await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (slugTaken)
        {
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"A post with slug '{normalizedSlug}' already exists.");
        }

        var post = Post.Create(
            _currentUser.UserId,
            request.Title,
            request.Slug,
            request.Summary,
            Sanitizer.Sanitize(request.ContentHtml),
            request.CategoryId,
            request.MetaTitle,
            request.MetaDescription);

        post.SetFeaturedImage(request.FeaturedImageUrl);

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.TagIds.Count > 0)
        {
            var postTags = await _context.Tags
                .AsNoTracking()
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => PostTag.Create(post.Id, t.Id))
                .ToListAsync(cancellationToken);

            _context.PostTags.AddRange(postTags);

            await _context.SaveChangesAsync(cancellationToken);
        }

        return post.Id;
    }
}
