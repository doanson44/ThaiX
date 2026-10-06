using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Tags.Commands.CreateTag;

/// <summary>
/// Creates a new tag or returns the existing tag id when the name already exists.
/// </summary>
public sealed partial class CreateTagCommandHandler
    : IRequestHandler<CreateTagCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateTagCommand request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);

        var existingId = await _context.Tags
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId != Guid.Empty)
        {
            return existingId;
        }

        var tag = Tag.Create(
            name,
            BuildSlug(name));

        _context.Tags.Add(tag);

        await _context.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }

    private static string NormalizeName(string name)
    {
        return name.Trim();
    }

    private static string BuildSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = NonSlugCharacterRegex().Replace(slug, string.Empty);

        return slug;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9-]")]
    private static partial Regex NonSlugCharacterRegex();
}