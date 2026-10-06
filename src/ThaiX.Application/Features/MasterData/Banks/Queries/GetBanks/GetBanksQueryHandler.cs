using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Banks.Queries.GetBanks;

/// <summary>
/// Handler for GetBanksQuery.
/// </summary>
public sealed class GetBanksQueryHandler : IRequestHandler<GetBanksQuery, PagedResult<BankListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBanksQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<BankListItemDto>> Handle(
        GetBanksQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Banks
            .AsNoTracking()
            .Include(b => b.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();
            query = query.Where(b => b.CountryCode == countryCode);
        }

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(b =>
                EF.Functions.Like(EF.Functions.Collate(b.Name, collation), pattern, "\\") ||
                EF.Functions.Like(EF.Functions.Collate(b.Code, collation), pattern, "\\"));
        }

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "name" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "countryname" => request.SortDescending ? query.OrderByDescending(b => b.Country.Name) : query.OrderBy(b => b.Country.Name),
            "countrycode" => request.SortDescending ? query.OrderByDescending(b => b.CountryCode) : query.OrderBy(b => b.CountryCode),
            _ => request.SortDescending ? query.OrderByDescending(b => b.Code) : query.OrderBy(b => b.Code)
        };

        return await query
            .Select(b => new BankListItemDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                CountryCode = b.CountryCode,
                CountryName = b.Country.Name,
                DisplayText = string.IsNullOrEmpty(b.Code) ? b.Name : "[" + b.Code + "] " + b.Name
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
