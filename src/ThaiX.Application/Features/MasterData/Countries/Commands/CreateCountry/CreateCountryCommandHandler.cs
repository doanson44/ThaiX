using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.CreateCountry;

/// <summary>
/// Handler for CreateCountryCommand.
/// </summary>
public sealed class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCountryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        var codeNormalized = request.Code.Trim().ToUpperInvariant();

        var exists = await _dbContext.Countries
            .AnyAsync(c => c.Code == codeNormalized, cancellationToken);

        if (exists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"Country with code '{codeNormalized}' already exists.");

        var country = Country.Create(request.Code, request.Name);

        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return country.Id;
    }
}
