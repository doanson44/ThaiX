using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagDetail;

/// <summary>
/// Handler for GetExpenseTagDetailQuery.
/// </summary>
public sealed class GetExpenseTagDetailQueryHandler : IRequestHandler<GetExpenseTagDetailQuery, ExpenseTagDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseTagDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseTagDto> Handle(GetExpenseTagDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.ExpenseTags
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseTagDto
            {
                Id = x.Id,
                Name = x.Name,
                ColorHex = x.ColorHex
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Tag '{request.Id}' not found.");
    }
}
