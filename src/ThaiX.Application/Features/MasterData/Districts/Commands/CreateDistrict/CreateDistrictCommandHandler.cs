using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.CreateDistrict;

/// <summary>
/// Handler for CreateDistrictCommand.
/// </summary>
public sealed class CreateDistrictCommandHandler : IRequestHandler<CreateDistrictCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateDistrictCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        var cityCode = request.CityCode.Trim().ToUpperInvariant();
        var city = await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Code == cityCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"City with code '{cityCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var exists = await _dbContext.Districts
            .AnyAsync(d => d.Code == codeNormalized, cancellationToken);

        if (exists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"District with code '{codeNormalized}' already exists.");

        var district = District.Create(request.Code, request.Name, city.Code, city.Id);

        _dbContext.Districts.Add(district);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return district.Id;
    }
}
