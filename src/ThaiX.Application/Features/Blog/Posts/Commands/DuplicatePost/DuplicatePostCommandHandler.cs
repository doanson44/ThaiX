using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Commands.DuplicatePost;

public sealed class DuplicatePostCommandHandler : IRequestHandler<DuplicatePostCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DuplicatePostCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        DuplicatePostCommand request,
        CancellationToken cancellationToken)
    {
        var source = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken)
            ?? throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"Post '{request.Id}' not found.");

        var sourceTagIds = await _context.PostTags
            .AsNoTracking()
            .Where(pt => pt.PostId == source.Id)
            .Select(pt => pt.TagId)
            .ToListAsync(cancellationToken);

        var slug = await BuildUniqueCopySlugAsync(
            source.Slug,
            cancellationToken);

        var copy = source.Duplicate(
            _currentUser.UserId,
            slug);

        _context.Posts.Add(copy);

        _context.PostTags.AddRange(
            sourceTagIds.Select(tagId => PostTag.Create(copy.Id, tagId)));

        await _context.SaveChangesAsync(cancellationToken);

        return copy.Id;
    }

    private async Task<string> BuildUniqueCopySlugAsync(
        string sourceSlug,
        CancellationToken cancellationToken)
    {
        var prefix = $"{sourceSlug}-copy";

        var existingSlugs = await _context.Posts
            .AsNoTracking()
            .Where(p => p.Slug.StartsWith(prefix))
            .Select(p => p.Slug)
            .ToListAsync(cancellationToken);

        var maxSuffix = existingSlugs
            .Select(slug => GetCopySuffix(slug, prefix))
            .DefaultIfEmpty(0)
            .Max();

        return maxSuffix == 0
            ? prefix
            : $"{prefix}-{maxSuffix + 1}";
    }

    private static int GetCopySuffix(string slug, string prefix)
    {
        if (slug.Equals(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        var suffix = slug[prefix.Length..];

        if (!suffix.StartsWith("-", StringComparison.Ordinal))
        {
            return 0;
        }

        return int.TryParse(suffix[1..], out var number)
            ? number
            : 0;
    }
}