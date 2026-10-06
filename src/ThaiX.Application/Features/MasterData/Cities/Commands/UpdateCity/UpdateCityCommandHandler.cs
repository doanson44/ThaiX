using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.UpdateCity;

/// <summary>
/// Handler for UpdateCityCommand.
/// </summary>
public sealed class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
    {
        var city = await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"City with ID '{request.Id}' not found.");

        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with code '{countryCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var duplicateExists = await _dbContext.Cities
            .AnyAsync(c => c.Code == codeNormalized && c.Id != request.Id, cancellationToken);

        if (duplicateExists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"City with code '{codeNormalized}' already exists.");

        city.UpdateDetails(request.Code, request.Name, country.Code, country.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
