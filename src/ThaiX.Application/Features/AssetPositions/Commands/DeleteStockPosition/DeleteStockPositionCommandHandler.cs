using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.AssetPositions.Commands.DeleteStockPosition;

public sealed class DeleteStockPositionCommandHandler : IRequestHandler<DeleteStockPositionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteStockPositionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteStockPositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.StockPositions
            .FirstOrDefaultAsync(p =>
                p.Id == request.Id &&
                !p.IsDeleted &&
                p.Portfolio.OwnerId == _currentUser.UserId,
                cancellationToken)
            ?? throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"Position '{request.Id}' not found.");

        position.SoftDelete();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
