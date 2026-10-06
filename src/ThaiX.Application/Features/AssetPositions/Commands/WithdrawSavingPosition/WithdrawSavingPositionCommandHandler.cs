using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.AssetPositions.Commands.WithdrawSavingPosition;

public sealed class WithdrawSavingPositionCommandHandler : IRequestHandler<WithdrawSavingPositionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public WithdrawSavingPositionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(WithdrawSavingPositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.SavingPositions
            .FirstOrDefaultAsync(p =>
                p.Id == request.Id &&
                !p.IsDeleted &&
                p.Portfolio.OwnerId == _currentUser.UserId,
                cancellationToken)
            ?? throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"Position '{request.Id}' not found.");

        position.Withdraw(request.WithdrawalDate);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
