using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Districts.Queries.GetDistricts;

/// <summary>
/// Handler for GetDistrictsQuery.
/// </summary>
public sealed class GetDistrictsQueryHandler : IRequestHandler<GetDistrictsQuery, PagedResult<DistrictListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDistrictsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<DistrictListItemDto>> Handle(
        GetDistrictsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Districts
            .AsNoTracking()
            .Include(d => d.City)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CityCode))
        {
            var cityCode = request.CityCode.Trim().ToUpperInvariant();
            query = query.Where(d => d.CityCode == cityCode);
        }

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(d =>
                EF.Functions.Like(EF.Functions.Collate(d.Name, collation), pattern, "\\") ||
                EF.Functions.Like(EF.Functions.Collate(d.Code, collation), pattern, "\\"));
        }

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "name" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "cityname" => request.SortDescending ? query.OrderByDescending(d => d.City.Name) : query.OrderBy(d => d.City.Name),
            "citycode" => request.SortDescending ? query.OrderByDescending(d => d.CityCode) : query.OrderBy(d => d.CityCode),
            _ => request.SortDescending ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code)
        };

        return await query
            .Select(d => new DistrictListItemDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                CityCode = d.CityCode,
                CityName = d.City.Name,
                DisplayText = string.IsNullOrEmpty(d.Code) ? d.Name : "[" + d.Code + "] " + d.Name
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
