using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Countries.Queries.GetCountries;

/// <summary>
/// Handler for GetCountriesQuery.
/// </summary>
public sealed class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, PagedResult<CountryListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCountriesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CountryListItemDto>> Handle(
        GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Countries.AsNoTracking().AsQueryable();

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(c =>
                EF.Functions.Like(EF.Functions.Collate(c.Name, collation), pattern, "\\") ||
                EF.Functions.Like(EF.Functions.Collate(c.Code, collation), pattern, "\\"));
        }

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "name" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            _ => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code)
        };

        return await query
            .Select(c => new CountryListItemDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                DisplayText = string.IsNullOrEmpty(c.Code) ? c.Name : "[" + c.Code + "] " + c.Name
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
