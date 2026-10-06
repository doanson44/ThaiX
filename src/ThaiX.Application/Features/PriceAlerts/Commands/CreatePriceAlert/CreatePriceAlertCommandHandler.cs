using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;

public sealed class CreatePriceAlertCommandHandler : IRequestHandler<CreatePriceAlertCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePriceAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePriceAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = PriceAlert.Create(
            request.Symbol,
            request.AssetType,
            request.Condition,
            request.TargetPrice,
            request.Note,
            request.IsOneTime);

        _context.PriceAlerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken);

        return alert.Id;
    }
}
