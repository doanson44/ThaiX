using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Commands.UnpublishPost;

public sealed class UnpublishPostCommandHandler : IRequestHandler<UnpublishPostCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UnpublishPostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnpublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Post '{request.Id}' not found.");

        post.Unpublish();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
