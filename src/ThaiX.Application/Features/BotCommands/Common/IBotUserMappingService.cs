namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotUserMappingService
{
    Task<BotMappedUser?> ResolveAsync(string platform, string externalUserId, CancellationToken cancellationToken);
}
