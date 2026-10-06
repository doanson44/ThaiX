using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Commands.PublishPost;

public sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public PublishPostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Post '{request.Id}' not found.");

        post.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
