using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notes;

namespace ThaiX.Application.Features.Notes.Commands.CreateNote;

public sealed class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateNoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = Note.Create(
            _currentUser.UserId,
            request.Title,
            request.Content,
            request.Color);

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
