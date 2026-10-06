using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.UpdateDistrict;

/// <summary>
/// Handler for UpdateDistrictCommand.
/// </summary>
public sealed class UpdateDistrictCommandHandler : IRequestHandler<UpdateDistrictCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateDistrictCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await _dbContext.Districts
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"District with ID '{request.Id}' not found.");

        var cityCode = request.CityCode.Trim().ToUpperInvariant();
        var city = await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Code == cityCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"City with code '{cityCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var duplicateExists = await _dbContext.Districts
            .AnyAsync(d => d.Code == codeNormalized && d.Id != request.Id, cancellationToken);

        if (duplicateExists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"District with code '{codeNormalized}' already exists.");

        district.UpdateDetails(request.Code, request.Name, city.Code, city.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
