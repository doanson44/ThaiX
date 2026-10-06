using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.DeleteCity;

/// <summary>
/// Handler for DeleteCityCommand.
/// </summary>
public sealed class DeleteCityCommandHandler : IRequestHandler<DeleteCityCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteCityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
    {
        var city = await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"City with ID '{request.Id}' not found.");

        city.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
