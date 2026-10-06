using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Contacts.Queries.GetContacts;

/// <summary>
/// Handler for GetContactsQuery.
/// </summary>
public sealed class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, PagedResult<ContactListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContactsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ContactListItemDto>> Handle(
        GetContactsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Contacts
            .AsNoTracking()
            .Include(c => c.Emails)
            .Include(c => c.Phones)
            .AsQueryable();

        // Filter by archived status
        if (request.IsArchived.HasValue)
            query = query.Where(c => c.IsArchived == request.IsArchived.Value);

        // Filter by tag
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim().ToLower();
            query = query.Where(c => c.Tags.Any(t => t.Name.ToLower() == tag));
        }

        // Search
        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(c =>
                EF.Functions.Like(EF.Functions.Collate(c.FullName.FirstName, collation), pattern, "\\") ||
                EF.Functions.Like(EF.Functions.Collate(c.FullName.LastName, collation), pattern, "\\") ||
                (c.Company != null && EF.Functions.Like(EF.Functions.Collate(c.Company, collation), pattern, "\\")) ||
                c.Emails.Any(e => EF.Functions.Like(EF.Functions.Collate(e.Value, collation), pattern, "\\")) ||
                c.Phones.Any(p => EF.Functions.Like(EF.Functions.Collate(p.Value, collation), pattern, "\\")));
        }

        // Sort
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "firstname" => request.SortDescending
                ? query.OrderByDescending(c => c.FullName.FirstName)
                : query.OrderBy(c => c.FullName.FirstName),
            "lastname" => request.SortDescending
                ? query.OrderByDescending(c => c.FullName.LastName)
                : query.OrderBy(c => c.FullName.LastName),
            "company" => request.SortDescending
                ? query.OrderByDescending(c => c.Company)
                : query.OrderBy(c => c.Company),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(c => c.FullName.LastName)
                : query.OrderBy(c => c.FullName.LastName)
        };

        return await query
            .Select(c => new ContactListItemDto
            {
                Id = c.Id,
                DisplayName = c.FullName.FirstName + " " + c.FullName.LastName,
                FirstName = c.FullName.FirstName,
                LastName = c.FullName.LastName,
                PrimaryEmail = c.Emails.Where(e => e.IsPrimary).Select(e => e.Value).FirstOrDefault(),
                PrimaryPhone = c.Phones.Where(p => p.IsPrimary).Select(p => p.Value).FirstOrDefault(),
                Company = c.Company,
                JobTitle = c.JobTitle,
                AvatarUrl = c.AvatarUrl,
                IsArchived = c.IsArchived,
                CreatedAt = c.CreatedAt,
                LastUpdated = c.UpdatedAt ?? c.CreatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
