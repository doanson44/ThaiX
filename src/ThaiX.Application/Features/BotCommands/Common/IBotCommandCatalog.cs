namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandCatalog
{
    IReadOnlyList<BotCommandMetadata> GetAll();

    bool TryResolve(string nameOrAlias, out BotCommandMetadata? metadata);
}