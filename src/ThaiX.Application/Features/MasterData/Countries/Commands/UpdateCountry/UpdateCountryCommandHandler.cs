using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.UpdateCountry;

/// <summary>
/// Handler for UpdateCountryCommand.
/// </summary>
public sealed class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCountryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with ID '{request.Id}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();

        var duplicateExists = await _dbContext.Countries
            .AnyAsync(c => c.Code == codeNormalized && c.Id != request.Id, cancellationToken);

        if (duplicateExists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"Country with code '{codeNormalized}' already exists.");

        country.UpdateDetails(request.Code, request.Name);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
