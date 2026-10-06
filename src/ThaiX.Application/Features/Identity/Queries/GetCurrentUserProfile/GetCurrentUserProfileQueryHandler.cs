using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.GetCurrentUserProfile;

/// <summary>
/// Reads UserProfile by UserId (AsNoTracking, DTO projection).
/// </summary>
public sealed class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurrentUserProfileQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto?> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .Select(p => new UserProfileDto
            {
                FullName = p.FullName,
                AvatarUrl = p.AvatarUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        var slug = await _dbContext.ResumeProfiles
            .Where(r => r.OwnerId == request.UserId)
            .Select(r => r.Slug)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is not null)
            return profile with { ResumeSlug = slug };

        // UserProfile row may not exist yet, but we still return ResumeSlug if available
        if (slug is not null)
            return new UserProfileDto { ResumeSlug = slug };

        return null;
    }
}
