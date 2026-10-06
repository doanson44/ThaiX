using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Commands.RevokeJsonBinShareLink;

public sealed class RevokeJsonBinShareLinkCommandHandler
    : IRequestHandler<RevokeJsonBinShareLinkCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RevokeJsonBinShareLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RevokeJsonBinShareLinkCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.JsonBins
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"JsonBin with ID '{request.Id}' not found.");
        }

        entity.RevokeShare();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
