using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Notes.Queries.GetNotes;

public sealed class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, PagedResult<NoteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNotesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NoteDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notes
            .AsNoTracking()
            .Where(n => n.OwnerId == request.OwnerId);

        if (request.IsPinned.HasValue)
            query = query.Where(n => n.IsPinned == request.IsPinned.Value);

        if (request.IsArchived.HasValue)
            query = query.Where(n => n.IsArchived == request.IsArchived.Value);

        if (request.IsDeleted.HasValue)
            query = request.IsDeleted.Value
                ? query.IgnoreQueryFilters().Where(n => n.IsDeleted)
                : query;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(n =>
                EF.Functions.Like(n.Title, pattern) ||
                EF.Functions.Like(n.Content, pattern));
        }

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "title" => request.SortDescending
                ? query.OrderByDescending(n => n.Title)
                : query.OrderBy(n => n.Title),
            "updatedat" => request.SortDescending
                ? query.OrderByDescending(n => n.UpdatedAt)
                : query.OrderBy(n => n.UpdatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt)
                : query.OrderByDescending(n => n.IsPinned).ThenBy(n => n.CreatedAt)
        };

        return await query
            .Select(n => new NoteDto
            {
                Id = n.Id,
                OwnerId = n.OwnerId,
                Title = n.Title,
                Content = n.Content,
                Color = n.Color,
                IsPinned = n.IsPinned,
                IsArchived = n.IsArchived,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
