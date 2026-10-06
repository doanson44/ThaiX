namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandRegistry
{
    bool TryResolve(string commandOrAlias, out IBotCommandModule? module);
    IReadOnlyList<IBotCommandModule> GetAll();
}
