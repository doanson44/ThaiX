using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using DomainPermissions = ThaiX.Domain.Common.Constants.Permissions;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class HelpBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "help",
        Aliases = ["h"],
        Syntax = "/help [command]",
        Description = "Show all available commands and usage.",
        Category = BotCommandCategory.System,
        RequiredPermissions = [],
        SupportsAsync = false
    };

    private readonly IBotCommandCatalog _catalog;
    private readonly ICurrentUserService _currentUser;

    public HelpBotCommandModule(IBotCommandCatalog catalog, ICurrentUserService currentUser)
    {
        _catalog = catalog;
        _currentUser = currentUser;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        if (context.ParsedCommand.Arguments.Count > 0)
        {
            return Task.FromResult(DescribeSingleCommand(context.ParsedCommand.Arguments[0]));
        }

        var visible = _catalog.GetAll()
            .Where(CanUse)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (visible.Count == 0)
        {
            return Task.FromResult(new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "No commands are available for this account."
            });
        }

        var rows = visible
            .Select(x => (IReadOnlyList<string>)
            [
                "/" + x.Name,
                x.Aliases.Count == 0
                    ? "-"
                    : string.Join(", ", x.Aliases.Select(a => "/" + a)),
                x.Category.ToString(),
                x.Syntax
            ])
            .ToList();

        var plainText = BuildHelpPlainText(visible);

        return Task.FromResult(new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "System",
                Title = "Help",
                Type = NotificationCardType.Summary,
                Metrics = visible.Select(cmd => new NotificationMetric
                {
                    Label = $"/{cmd.Name}",
                    Value = cmd.Description
                }).ToList()
            }
        });
    }

    private BotCommandExecutionDto DescribeSingleCommand(string nameOrAlias)
    {
        if (!_catalog.TryResolve(nameOrAlias, out var metadata) || metadata is null)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = $"Unknown command '{nameOrAlias}'.",
                ErrorCode = Application.Common.Constants.ErrorCodes.INVALID_REQUEST
            };
        }

        if (!CanUse(metadata))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "You do not have permission to view this command.",
                ErrorCode = Application.Common.Constants.ErrorCodes.INSUFFICIENT_PERMISSIONS
            };
        }

        var aliases = metadata.Aliases.Count == 0
            ? "-"
            : string.Join(", ", metadata.Aliases.Select(x => "/" + x));

        var detail =
            $"Command: /{metadata.Name}\n" +
            $"Description: {metadata.Description}\n" +
            $"Category: {metadata.Category}\n" +
            $"Syntax: {metadata.Syntax}\n" +
            $"Aliases: {aliases}\n" +
            $"SupportsAsync: {metadata.SupportsAsync}";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = detail,
            Card = new NotificationCard
            {
                Category = "System",
                Title = $"/{metadata.Name}",
                Type = NotificationCardType.Summary,
                Metrics =
                [
                    new() { Label = "Description", Value = metadata.Description },
                    new() { Label = "Category", Value = metadata.Category.ToString() },
                    new() { Label = "Syntax", Value = metadata.Syntax },
                    new() { Label = "Aliases", Value = metadata.Aliases.Count == 0 ? "none" : string.Join(", ", metadata.Aliases) },
                    new() { Label = "SupportsAsync", Value = metadata.SupportsAsync.ToString() }
                ]
            }
        };
    }

    private static string BuildHelpPlainText(IReadOnlyList<BotCommandMetadata> visible)
    {
        var lines = new List<string>
        {
            $"*Available commands ({visible.Count}):*",
            string.Empty
        };

        foreach (var cmd in visible)
        {
            var aliases = cmd.Aliases.Count == 0
                ? string.Empty
                : $" ({string.Join(", ", cmd.Aliases)})";

            lines.Add($"  /{cmd.Name}{aliases}  —  {cmd.Description}");
        }

        lines.Add(string.Empty);
        lines.Add("Use /help <command> for details.");

        return string.Join("\n", lines);
    }

    private bool CanUse(BotCommandMetadata metadata)
    {
        if (metadata.RequiredPermissions.Count == 0)
        {
            return true;
        }

        // System M2M clients with BotCommand.Execute bypass per-command permission checks.
        if (_currentUser.HasAllPermissions(DomainPermissions.BotCommandExecute))
        {
            return true;
        }

        return _currentUser.IsAuthenticated
               && _currentUser.HasAllPermissions(metadata.RequiredPermissions.ToArray());
    }
}
