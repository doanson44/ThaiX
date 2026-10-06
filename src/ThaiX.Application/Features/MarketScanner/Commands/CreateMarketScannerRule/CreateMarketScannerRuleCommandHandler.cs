using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Features.MarketScanner.Commands.CreateMarketScannerRule;

/// <summary>
/// Handler for CreateMarketScannerRuleCommand.
/// </summary>
public sealed class CreateMarketScannerRuleCommandHandler
    : IRequestHandler<CreateMarketScannerRuleCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateMarketScannerRuleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(
        CreateMarketScannerRuleCommand request,
        CancellationToken cancellationToken)
    {
        var rule = MarketScannerRule.Create(
            request.Name,
            request.SignalType,
            request.Window,
            request.Threshold);

        _dbContext.MarketScannerRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}
