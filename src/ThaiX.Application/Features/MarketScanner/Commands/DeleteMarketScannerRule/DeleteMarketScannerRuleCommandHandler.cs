using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MarketScanner.Commands.DeleteMarketScannerRule;

/// <summary>
/// Handler for DeleteMarketScannerRuleCommand.
/// </summary>
public sealed class DeleteMarketScannerRuleCommandHandler : IRequestHandler<DeleteMarketScannerRuleCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteMarketScannerRuleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(
        DeleteMarketScannerRuleCommand request,
        CancellationToken cancellationToken)
    {
        var rule = await _dbContext.MarketScannerRules
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Market scanner rule '{request.Id}' not found.");

        rule.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
