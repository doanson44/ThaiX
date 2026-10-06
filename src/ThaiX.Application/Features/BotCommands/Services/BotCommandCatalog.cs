using System.Reflection;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandCatalog : IBotCommandCatalog
{
    private readonly IReadOnlyList<BotCommandMetadata> _all;
    private readonly Dictionary<string, BotCommandMetadata> _lookup;

    public BotCommandCatalog()
    {
        var moduleTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsClass: true }
                && typeof(IBotCommandModule).IsAssignableFrom(type))
            .ToList();

        var metadataList = new List<BotCommandMetadata>();
        foreach (var moduleType in moduleTypes)
        {
            var metadata = TryReadMetadata(moduleType);
            if (metadata is not null)
            {
                metadataList.Add(metadata);
            }
        }

        _all = metadataList
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _lookup = new Dictionary<string, BotCommandMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var metadata in _all)
        {
            _lookup[metadata.Name] = metadata;
            foreach (var alias in metadata.Aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    _lookup[alias.Trim()] = metadata;
                }
            }
        }
    }

    public IReadOnlyList<BotCommandMetadata> GetAll() => _all;

    public bool TryResolve(string nameOrAlias, out BotCommandMetadata? metadata)
    {
        if (string.IsNullOrWhiteSpace(nameOrAlias))
        {
            metadata = null;
            return false;
        }

        var key = nameOrAlias.Trim().TrimStart('/');
        return _lookup.TryGetValue(key, out metadata);
    }

    private static BotCommandMetadata? TryReadMetadata(Type moduleType)
    {
        var field = moduleType.GetField(
            "MetadataValue",
            BindingFlags.Static | BindingFlags.NonPublic);

        if (field?.FieldType == typeof(BotCommandMetadata)
            && field.GetValue(null) is BotCommandMetadata metadataFromField)
        {
            return metadataFromField;
        }

        return null;
    }
}