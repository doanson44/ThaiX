using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.SuggestUsersByContactPhones;

/// <summary>
/// Loads contact phones, gets normalized values, queries users by phone (excluding already-linked), returns DTOs.
/// </summary>
public sealed class SuggestUsersByContactPhonesQueryHandler : IRequestHandler<SuggestUsersByContactPhonesQuery, IReadOnlyList<SuggestedUserDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityUserService _identityUserService;

    public SuggestUsersByContactPhonesQueryHandler(
        IApplicationDbContext dbContext,
        IIdentityUserService identityUserService)
    {
        _dbContext = dbContext;
        _identityUserService = identityUserService;
    }

    public async Task<IReadOnlyList<SuggestedUserDto>> Handle(
        SuggestUsersByContactPhonesQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedPhones = await _dbContext.ContactPhones
            .AsNoTracking()
            .Where(p => p.ContactId == request.ContactId && p.NormalizedValue != null && p.NormalizedValue != "")
            .Select(p => p.NormalizedValue!)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (normalizedPhones.Count == 0)
            return Array.Empty<SuggestedUserDto>();

        var excludeUserIds = await _dbContext.UserContactLinks
            .AsNoTracking()
            .Select(l => l.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _identityUserService.GetUsersByNormalizedPhonesAsync(
            normalizedPhones,
            excludeUserIds,
            cancellationToken);
    }
}
