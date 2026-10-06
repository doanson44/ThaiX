using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.AssetPositions.Commands.UpdateStockPositionTargets;

public sealed class UpdateStockPositionTargetsCommandHandler : IRequestHandler<UpdateStockPositionTargetsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateStockPositionTargetsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateStockPositionTargetsCommand request, CancellationToken cancellationToken)
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

        position.UpdateTargets(
            request.TargetPrice,
            request.StopLoss,
            request.Note);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
