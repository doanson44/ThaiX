using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Portfolios.Queries.GetPortfolios;

public sealed class GetPortfoliosQueryHandler
    : IRequestHandler<GetPortfoliosQuery, PagedResult<PortfolioListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPortfoliosQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PortfolioListItemDto>> Handle(
        GetPortfoliosQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Portfolios
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.OwnerId == _currentUser.UserId);

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(p => EF.Functions.Like(EF.Functions.Collate(p.Name, collation), pattern, "\\"));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PortfolioListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                PortfolioType = p.PortfolioType,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
