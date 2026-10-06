using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletDetail;

/// <summary>
/// Handler for GetWalletDetailQuery.
/// </summary>
public sealed class GetWalletDetailQueryHandler : IRequestHandler<GetWalletDetailQuery, ExpenseWalletDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWalletDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseWalletDto> Handle(GetWalletDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.Wallets
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseWalletDto
            {
                Id = x.Id,
                Name = x.Name,
                WalletType = x.WalletType,
                Currency = x.Currency,
                CurrentBalance = x.CurrentBalance
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.Id}' not found.");
    }
}
