using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.GetLinkedContactForUser;

/// <summary>
/// Returns linked contact from UserProfile by UserId (AsNoTracking).
/// </summary>
public sealed class GetLinkedContactForUserQueryHandler : IRequestHandler<GetLinkedContactForUserQuery, LinkedContactDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLinkedContactForUserQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LinkedContactDto?> Handle(GetLinkedContactForUserQuery request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (profile is null)
            return null;

        return new LinkedContactDto
        {
            ContactId = profile.ContactId,
            DisplayName = profile.FullName ?? $"{profile.ContactId}"
        };
    }
}
