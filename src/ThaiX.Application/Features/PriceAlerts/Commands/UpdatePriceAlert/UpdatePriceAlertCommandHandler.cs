using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;

public sealed class UpdatePriceAlertCommandHandler : IRequestHandler<UpdatePriceAlertCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdatePriceAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdatePriceAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.PriceAlerts
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Price alert '{request.Id}' not found.");

        alert.Update(
            request.Condition,
            request.TargetPrice,
            request.Note,
            request.IsOneTime,
            request.IsEnabled);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
