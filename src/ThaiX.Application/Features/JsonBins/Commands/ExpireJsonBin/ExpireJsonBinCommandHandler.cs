using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Commands.ExpireJsonBin;

public sealed class ExpireJsonBinCommandHandler : IRequestHandler<ExpireJsonBinCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ExpireJsonBinCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ExpireJsonBinCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.JsonBins
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"JsonBin with ID '{request.Id}' not found.");
        }

        entity.MarkExpired();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
