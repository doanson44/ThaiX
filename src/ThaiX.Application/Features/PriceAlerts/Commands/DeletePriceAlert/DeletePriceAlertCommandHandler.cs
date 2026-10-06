using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PriceAlerts.Commands.DeletePriceAlert;

public sealed class DeletePriceAlertCommandHandler : IRequestHandler<DeletePriceAlertCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeletePriceAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeletePriceAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.PriceAlerts
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Price alert '{request.Id}' not found.");

        alert.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
