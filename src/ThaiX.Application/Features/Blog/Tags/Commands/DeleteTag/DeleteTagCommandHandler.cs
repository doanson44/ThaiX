using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Tags.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Tag '{request.Id}' not found.");

        await _context.PostTags
            .Where(x => x.TagId == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        tag.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
