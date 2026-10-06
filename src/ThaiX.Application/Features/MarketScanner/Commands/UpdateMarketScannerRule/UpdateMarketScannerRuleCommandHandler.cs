using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerRule;

/// <summary>
/// Handler for UpdateMarketScannerRuleCommand.
/// </summary>
public sealed class UpdateMarketScannerRuleCommandHandler : IRequestHandler<UpdateMarketScannerRuleCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateMarketScannerRuleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(
        UpdateMarketScannerRuleCommand request,
        CancellationToken cancellationToken)
    {
        var rule = await _dbContext.MarketScannerRules
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Market scanner rule '{request.Id}' not found.");

        rule.Update(request.Name, request.Window, request.Threshold, request.IsEnabled);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
