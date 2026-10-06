namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandParser
{
    BotParseResult Parse(string rawText);
}
