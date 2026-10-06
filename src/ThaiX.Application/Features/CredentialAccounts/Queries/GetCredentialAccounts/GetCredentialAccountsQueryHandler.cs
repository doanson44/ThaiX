using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccounts;

public sealed class GetCredentialAccountsQueryHandler
    : IRequestHandler<GetCredentialAccountsQuery, PagedResult<CredentialAccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetCredentialAccountsQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PagedResult<CredentialAccountDto>> Handle(
        GetCredentialAccountsQuery request,
        CancellationToken cancellationToken)
    {
        Expression<Func<CredentialAccount, bool>> predicate = PredicateExtensions.True<CredentialAccount>();

        if (request.IsUsed.HasValue)
        {
            predicate = predicate.And(x => x.IsUsed == request.IsUsed.Value);
        }

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            predicate = predicate.And(x =>
                EF.Functions.Like(EF.Functions.Collate(x.Username, collation), pattern, "\\") ||
                (x.Description != null && EF.Functions.Like(EF.Functions.Collate(x.Description, collation), pattern, "\\")));
        }

        var query = _context.CredentialAccounts
            .AsNoTracking()
            .Where(predicate);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "username" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "usedat" => request.SortDescending ? query.OrderByDescending(x => x.UsedAt) : query.OrderBy(x => x.UsedAt),
            "usedby" => request.SortDescending ? query.OrderByDescending(x => x.UsedBy) : query.OrderBy(x => x.UsedBy),
            "usagecount" => request.SortDescending ? query.OrderByDescending(x => x.UsageCount) : query.OrderBy(x => x.UsageCount),
            "createdat" => request.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(x => x.Username) : query.OrderBy(x => x.Username)
        };

        var result = await query
            .Select(x => new CredentialAccountDto
            {
                Id = x.Id,
                Username = x.Username,
                Description = x.Description,
                IsUsed = x.IsUsed,
                UsedAt = x.UsedAt,
                UsedBy = x.UsedBy,
                UsageCount = x.UsageCount,
                LastUsedAgo = string.Empty
            })
            .ToPagedListAsync(request, cancellationToken);

        return result with
        {
            Items = result.Items
                .Select(x => x with { LastUsedAgo = CredentialAccountLastUsedFormatter.Format(x.UsedAt, _dateTimeProvider.UtcNow) })
                .ToList()
        };
    }
}
