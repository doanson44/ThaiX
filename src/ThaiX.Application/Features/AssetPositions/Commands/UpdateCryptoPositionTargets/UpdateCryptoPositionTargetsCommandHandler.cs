using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.AssetPositions.Commands.UpdateCryptoPositionTargets;

public sealed class UpdateCryptoPositionTargetsCommandHandler : IRequestHandler<UpdateCryptoPositionTargetsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCryptoPositionTargetsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateCryptoPositionTargetsCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.CryptoPositions
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
