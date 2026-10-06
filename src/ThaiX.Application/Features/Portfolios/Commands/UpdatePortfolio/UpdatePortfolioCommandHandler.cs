using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Portfolios.Commands.UpdatePortfolio;

public sealed class UpdatePortfolioCommandHandler : IRequestHandler<UpdatePortfolioCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePortfolioCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OwnerId == _currentUser.UserId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Portfolio '{request.Id}' not found.");

        portfolio.Update(request.Name, request.PortfolioType, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
