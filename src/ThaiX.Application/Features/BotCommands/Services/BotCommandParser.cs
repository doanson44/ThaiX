using System.Text;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandParser : IBotCommandParser
{
    public BotParseResult Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return BotParseResult.ParseFailed("Command text is required.");
        }

        var tokens = Tokenize(rawText.Trim());
        if (tokens.Count == 0)
        {
            return BotParseResult.ParseFailed("Command text is required.");
        }

        var commandToken = NormalizeToken(tokens[0]);
        if (string.IsNullOrWhiteSpace(commandToken))
        {
            return BotParseResult.ParseFailed("Invalid command format.");
        }

        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var args = new List<string>();

        foreach (var token in tokens.Skip(1))
        {
            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                flags.Add(token.Trim().ToLowerInvariant());
                continue;
            }

            args.Add(token);
        }

        return BotParseResult.ParseSucceeded(new BotParsedCommand
        {
            Name = commandToken,
            RawText = rawText.Trim(),
            Arguments = args,
            Flags = flags
        });
    }

    private static string NormalizeToken(string token)
    {
        var normalized = token.Trim();
        if (normalized.StartsWith('/'))
        {
            normalized = normalized[1..];
        }

        return normalized.Trim().ToLowerInvariant();
    }

    private static IReadOnlyList<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in input)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
