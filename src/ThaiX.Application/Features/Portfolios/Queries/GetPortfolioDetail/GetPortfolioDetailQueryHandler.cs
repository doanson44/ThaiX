using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Portfolios.Queries.GetPortfolioDetail;

public sealed class GetPortfolioDetailQueryHandler : IRequestHandler<GetPortfolioDetailQuery, PortfolioDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPortfolioDetailQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PortfolioDetailDto?> Handle(GetPortfolioDetailQuery request, CancellationToken cancellationToken)
    {
        return await _context.Portfolios
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.OwnerId == _currentUser.UserId && !p.IsDeleted)
            .Select(p => new PortfolioDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                PortfolioType = p.PortfolioType,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
