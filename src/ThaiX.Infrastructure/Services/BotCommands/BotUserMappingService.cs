using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Infrastructure.Services.BotCommands;

public sealed class BotUserMappingService : IBotUserMappingService
{
    private readonly BotCommandSettings _settings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<BotUserMappingService> _logger;

    public BotUserMappingService(
        IOptions<BotCommandSettings> options,
        UserManager<ApplicationUser> userManager,
        IIdentityUserService identityUserService,
        ILogger<BotUserMappingService> logger)
    {
        _settings = options.Value;
        _userManager = userManager;
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<BotMappedUser?> ResolveAsync(
        string platform,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(externalUserId))
        {
            return null;
        }

        var userId = ResolveUserId(platform, externalUserId);
        if (userId == Guid.Empty)
        {
            _logger.LogDebug(
                "Bot user mapping not found. Platform={Platform}, ExternalUserId={ExternalUserId}",
                platform,
                externalUserId);
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogWarning(
                "Bot user mapping points to missing user. Platform={Platform}, ExternalUserId={ExternalUserId}, UserId={UserId}",
                platform,
                externalUserId,
                userId);
            return null;
        }

        var permissions = await _identityUserService.GetUserPermissionsAsync(userId, cancellationToken);

        return new BotMappedUser
        {
            UserId = userId,
            Email = user.Email ?? string.Empty,
            Permissions = permissions
        };
    }

    private Guid ResolveUserId(string platform, string externalUserId)
    {
        if (_settings.UserMappings.TryGetValue(platform, out var platformMap)
            && platformMap.TryGetValue(externalUserId, out var mappedValue)
            && Guid.TryParse(mappedValue, out var mappedGuid))
        {
            return mappedGuid;
        }

        if (_settings.AllowGuidPassthroughUserMapping && Guid.TryParse(externalUserId, out var passthrough))
        {
            return passthrough;
        }

        return Guid.Empty;
    }
}
