using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notes.Queries.GetNotes;

namespace ThaiX.Application.Features.Notes.Queries.GetNoteById;

public sealed class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNoteByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<NoteDto?> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Notes
            .AsNoTracking()
            .Where(n => n.Id == request.Id && n.OwnerId == _currentUser.UserId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
