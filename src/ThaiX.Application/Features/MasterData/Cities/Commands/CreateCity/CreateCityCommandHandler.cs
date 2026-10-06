using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.CreateCity;

/// <summary>
/// Handler for CreateCityCommand.
/// </summary>
public sealed class CreateCityCommandHandler : IRequestHandler<CreateCityCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with code '{countryCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var exists = await _dbContext.Cities
            .AnyAsync(c => c.Code == codeNormalized, cancellationToken);

        if (exists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"City with code '{codeNormalized}' already exists.");

        var city = City.Create(request.Code, request.Name, country.Code, country.Id);

        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return city.Id;
    }
}
