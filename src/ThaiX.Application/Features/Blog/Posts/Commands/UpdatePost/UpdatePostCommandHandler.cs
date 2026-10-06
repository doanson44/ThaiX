using Ganss.Xss;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Commands.UpdatePost;

public sealed class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Unit>
{
    private static readonly HtmlSanitizer Sanitizer = new();

    private readonly IApplicationDbContext _context;

    public UpdatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        UpdatePostCommand request,
        CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken)
            ?? throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"Post '{request.Id}' not found.");

        var normalizedSlug = request.Slug
            .Trim()
            .Trim('/')
            .ToLowerInvariant();

        var slugExists = await _context.Posts
            .AsNoTracking()
            .AnyAsync(
                p => p.Id != request.Id && p.Slug == normalizedSlug,
                cancellationToken);

        if (slugExists)
        {
            throw new OperationFailedException(
                ErrorCodes.DUPLICATE_ENTRY,
                $"A post with slug '{normalizedSlug}' already exists.");
        }

        post.UpdateContent(
            request.Title,
            request.Slug,
            request.Summary,
            Sanitizer.Sanitize(request.ContentHtml),
            request.CategoryId,
            request.MetaTitle,
            request.MetaDescription);

        post.SetFeaturedImage(request.FeaturedImageUrl);

        var existingPostTags = await _context.PostTags
            .Where(pt => pt.PostId == post.Id)
            .ToListAsync(cancellationToken);

        var existingTagIds = existingPostTags
            .Select(pt => pt.TagId)
            .ToHashSet();

        var requestedTagIds = await _context.Tags
            .AsNoTracking()
            .Where(t => request.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToHashSetAsync(cancellationToken);

        var postTagsToRemove = existingPostTags
            .Where(pt => !requestedTagIds.Contains(pt.TagId));

        _context.PostTags.RemoveRange(postTagsToRemove);

        var postTagsToAdd = requestedTagIds
            .Except(existingTagIds)
            .Select(tagId => PostTag.Create(post.Id, tagId));

        _context.PostTags.AddRange(postTagsToAdd);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}