using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferDetail;

/// <summary>
/// Handler for GetTransferDetailQuery.
/// </summary>
public sealed class GetTransferDetailQueryHandler : IRequestHandler<GetTransferDetailQuery, ExpenseTransferDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTransferDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseTransferDto> Handle(GetTransferDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.Transfers
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseTransferDto
            {
                Id = x.Id,
                SourceWalletId = x.SourceWalletId,
                TargetWalletId = x.TargetWalletId,
                Amount = x.Amount,
                TransferredOn = x.TransferredOn,
                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transfer '{request.Id}' not found.");
    }
}
