using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Notes.Commands.TogglePinNote;

public sealed class TogglePinNoteCommandHandler : IRequestHandler<TogglePinNoteCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TogglePinNoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(TogglePinNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.OwnerId == _currentUser.UserId, cancellationToken);

        if (note is null)
            throw new InvalidOperationException("Note not found.");

        note.TogglePin();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
