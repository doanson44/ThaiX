using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagDetail;

/// <summary>
/// Handler for GetTransactionTagDetailQuery.
/// </summary>
public sealed class GetTransactionTagDetailQueryHandler : IRequestHandler<GetTransactionTagDetailQuery, ExpenseTransactionTagDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTransactionTagDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseTransactionTagDto> Handle(GetTransactionTagDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.TransactionTags
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseTransactionTagDto
            {
                Id = x.Id,
                TransactionId = x.TransactionId,
                TagId = x.TagId
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction tag '{request.Id}' not found.");
    }
}
