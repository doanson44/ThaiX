using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateSavingPosition;

public sealed class CreateSavingPositionCommandHandler : IRequestHandler<CreateSavingPositionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSavingPositionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateSavingPositionCommand request, CancellationToken cancellationToken)
    {
        var portfolioExists = await _context.Portfolios
            .AnyAsync(p => p.Id == request.PortfolioId && p.OwnerId == _currentUser.UserId && !p.IsDeleted, cancellationToken);

        if (!portfolioExists)
            throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Portfolio '{request.PortfolioId}' not found.");

        var position = SavingPosition.Create(
            request.PortfolioId,
            request.BankName,
            request.AccountNumber,
            request.PrincipalAmount,
            request.InterestRate,
            request.InterestType,
            request.DepositDate,
            request.MaturityDate,
            request.Note);

        _context.SavingPositions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
}
