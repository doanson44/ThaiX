using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.DeleteDistrict;

/// <summary>
/// Handler for DeleteDistrictCommand.
/// </summary>
public sealed class DeleteDistrictCommandHandler : IRequestHandler<DeleteDistrictCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteDistrictCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await _dbContext.Districts
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"District with ID '{request.Id}' not found.");

        district.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
