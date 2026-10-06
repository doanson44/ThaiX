using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Portfolios.Commands.DeletePortfolio;

public sealed class DeletePortfolioCommandHandler : IRequestHandler<DeletePortfolioCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeletePortfolioCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeletePortfolioCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OwnerId == _currentUser.UserId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Portfolio '{request.Id}' not found.");

        portfolio.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
