using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandRegistry : IBotCommandRegistry
{
    private readonly IReadOnlyList<IBotCommandModule> _modules;
    private readonly IReadOnlyDictionary<string, IBotCommandModule> _aliasMap;

    public BotCommandRegistry(IEnumerable<IBotCommandModule> modules)
    {
        _modules = modules
            .OrderBy(m => m.Metadata.Category)
            .ThenBy(m => m.Metadata.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var map = new Dictionary<string, IBotCommandModule>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in _modules)
        {
            var aliases = module.Metadata.Aliases
                .Append(module.Metadata.Name)
                .Select(NormalizeAlias)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var alias in aliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                if (!map.TryAdd(alias, module))
                {
                    throw new InvalidOperationException($"Duplicate bot command alias '{alias}' detected.");
                }
            }
        }

        _aliasMap = map;
    }

    public bool TryResolve(string commandOrAlias, out IBotCommandModule? module)
    {
        var alias = NormalizeAlias(commandOrAlias);
        if (_aliasMap.TryGetValue(alias, out var resolved))
        {
            module = resolved;
            return true;
        }

        module = null;
        return false;
    }

    public IReadOnlyList<IBotCommandModule> GetAll() => _modules;

    private static string NormalizeAlias(string alias)
    {
        var normalized = alias.Trim();
        if (normalized.StartsWith('/'))
        {
            normalized = normalized[1..];
        }

        return normalized.ToLowerInvariant();
    }
}
