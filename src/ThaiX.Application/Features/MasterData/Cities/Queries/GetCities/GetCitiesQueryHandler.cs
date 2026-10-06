using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Cities.Queries.GetCities;

/// <summary>
/// Handler for GetCitiesQuery.
/// </summary>
public sealed class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, PagedResult<CityListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCitiesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CityListItemDto>> Handle(
        GetCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Cities
            .AsNoTracking()
            .Include(c => c.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();
            query = query.Where(c => c.CountryCode == countryCode);
        }

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
            "countryname" => request.SortDescending ? query.OrderByDescending(c => c.Country.Name) : query.OrderBy(c => c.Country.Name),
            "countrycode" => request.SortDescending ? query.OrderByDescending(c => c.CountryCode) : query.OrderBy(c => c.CountryCode),
            _ => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code)
        };

        return await query
            .Select(c => new CityListItemDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CountryCode = c.CountryCode,
                CountryName = c.Country.Name,
                DisplayText = string.IsNullOrEmpty(c.Code) ? c.Name : "[" + c.Code + "] " + c.Name
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
