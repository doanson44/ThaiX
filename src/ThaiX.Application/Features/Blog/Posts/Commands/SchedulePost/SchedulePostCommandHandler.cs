using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Posts.Commands.SchedulePost;

public sealed class SchedulePostCommandHandler : IRequestHandler<SchedulePostCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SchedulePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SchedulePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Post '{request.Id}' not found.");

        post.Schedule(request.PublishAt);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
