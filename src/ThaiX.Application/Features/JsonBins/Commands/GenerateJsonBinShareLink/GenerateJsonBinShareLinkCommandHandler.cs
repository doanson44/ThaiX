using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Commands.GenerateJsonBinShareLink;

public sealed class GenerateJsonBinShareLinkCommandHandler
    : IRequestHandler<GenerateJsonBinShareLinkCommand, GenerateJsonBinShareLinkResult>
{
    private readonly IApplicationDbContext _context;

    public GenerateJsonBinShareLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GenerateJsonBinShareLinkResult> Handle(
        GenerateJsonBinShareLinkCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.JsonBins
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"JsonBin with ID '{request.Id}' not found.");
        }

        if (entity.ExpiredAtUtc.HasValue && entity.ExpiredAtUtc.Value <= DateTime.UtcNow)
        {
            throw new OperationFailedException(
                ErrorCodes.OPERATION_NOT_ALLOWED,
                "Cannot share an expired JsonBin.");
        }

        var token = entity.EnableShare(request.ShareExpiresAtUtc);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenerateJsonBinShareLinkResult
        {
            Id = entity.Id,
            Code = entity.Code,
            Token = token,
            RelativePath = $"/api/public/json-bins/share/{token}",
            ShareExpiresAtUtc = entity.ShareExpiresAtUtc
        };
    }
}
