using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Commands.RestorePost;

public sealed class RestorePostCommandHandler : IRequestHandler<RestorePostCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RestorePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RestorePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Deleted post '{request.Id}' not found.");

        post.RestoreEntity();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
