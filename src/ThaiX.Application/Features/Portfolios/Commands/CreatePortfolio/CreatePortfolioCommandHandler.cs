using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Commands.CreatePortfolio;

public sealed class CreatePortfolioCommandHandler : IRequestHandler<CreatePortfolioCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePortfolioCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var portfolio = Portfolio.Create(
            _currentUser.UserId,
            request.Name,
            request.PortfolioType,
            request.Description);

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync(cancellationToken);

        return portfolio.Id;
    }
}
