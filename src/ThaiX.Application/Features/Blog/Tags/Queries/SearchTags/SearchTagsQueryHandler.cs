using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Blog.Tags.Queries.SearchTags;

public sealed class SearchTagsQueryHandler : IRequestHandler<SearchTagsQuery, PagedResult<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TagDto>> Handle(SearchTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tags
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.Search);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(t => EF.Functions.Like(EF.Functions.Collate(t.Name, collation), pattern, "\\"));
        }

        var projected = query
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug
            });

        return await projected.ToPagedListAsync(request, cancellationToken);
    }
}
