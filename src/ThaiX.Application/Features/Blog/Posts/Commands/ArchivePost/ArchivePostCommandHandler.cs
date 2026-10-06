using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Commands.ArchivePost;

public sealed class ArchivePostCommandHandler : IRequestHandler<ArchivePostCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ArchivePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ArchivePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Post '{request.Id}' not found.");

        post.Archive();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
