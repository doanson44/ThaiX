using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Notes.Commands.UpdateNote;

public sealed class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateNoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.Notes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.OwnerId == _currentUser.UserId, cancellationToken);

        if (note is null)
            throw new InvalidOperationException("Note not found.");

        if (note.IsDeleted)
            note.RestoreNote();

        note.Update(request.Title, request.Content, request.Color);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
