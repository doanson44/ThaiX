using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;
using ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class AlertBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "alert",
        Aliases = ["al"],
        Syntax = "/alert list | /alert <symbol> <>|< <price> [stock|crypto] [--repeat]",
        Description = "Manage price alerts.",
        Category = BotCommandCategory.Alerts,
        RequiredPermissions = [Permissions.PriceAlertRead],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AlertBotCommandModule(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        if (context.ParsedCommand.Arguments.Count == 0)
        {
            return BuildHelpResult();
        }

        var first = context.ParsedCommand.Arguments[0].Trim().ToLowerInvariant();
        if (first == "list")
        {
            return await ListAlertsAsync(cancellationToken);
        }

        return await CreateAlertAsync(context, cancellationToken);
    }

    private async Task<BotCommandExecutionDto> ListAlertsAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission(Permissions.PriceAlertRead))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "PriceAlert.Read permission is required.",
                ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS
            };
        }

        var result = await _mediator.Send(new GetPriceAlertsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "symbol",
            SortDescending = false
        }, cancellationToken);

        if (result.TotalCount == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "No alerts found."
            };
        }

        var rows = result.Items
            .Select(x => (IReadOnlyList<string>)
            [
                x.Symbol,
                x.AssetType.ToString(),
                x.Condition.ToString(),
                x.TargetPrice.ToString("0.####", CultureInfo.InvariantCulture),
                x.IsEnabled ? "On" : "Off",
                x.TriggerCount.ToString(CultureInfo.InvariantCulture)
            ])
            .ToList();

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = $"Showing {result.Items.Count} of {result.TotalCount} alerts.",
            Card = new NotificationCard
            {
                Category = "Alerts",
                Title = "Price Alerts",
                Type = NotificationCardType.Alert,
                Metrics = result.Items
                    .Select(x => new NotificationMetric
                    {
                        Label = $"{x.Symbol} [{x.AssetType}]",
                        Value = $"{x.Condition} {x.TargetPrice.ToString("0.####", CultureInfo.InvariantCulture)}"
                    })
                    .ToList()
            }
        };
    }

    private async Task<BotCommandExecutionDto> CreateAlertAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission(Permissions.PriceAlertWrite))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "PriceAlert.Write permission is required.",
                ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS
            };
        }

        var args = context.ParsedCommand.Arguments;
        if (args.Count < 3)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Usage: /alert <symbol> <>|< <price> [stock|crypto] [--repeat]",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var symbol = args[0].Trim().ToUpperInvariant();
        var op = args[1].Trim();
        if (!decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var targetPrice))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Target price must be a valid number.",
                ErrorCode = ErrorCodes.INVALID_FORMAT
            };
        }

        var condition = op switch
        {
            ">" => AlertCondition.Above,
            "<" => AlertCondition.Below,
            _ => AlertCondition.Above
        };

        if (op is not ">" and not "<")
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Condition must be '>' or '<'.",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var assetType = AssetType.CryptoSpot;
        if (args.Count > 3)
        {
            var assetArg = args[3].Trim().ToLowerInvariant();
            if (assetArg is "stock" or "vn" or "vnstock")
            {
                assetType = AssetType.VnStock;
            }
            else if (assetArg is "crypto" or "spot")
            {
                assetType = AssetType.CryptoSpot;
            }
        }

        var isOneTime = !context.ParsedCommand.HasFlag("--repeat");

        var id = await _mediator.Send(new CreatePriceAlertCommand
        {
            Symbol = symbol,
            AssetType = assetType,
            Condition = condition,
            TargetPrice = targetPrice,
            IsOneTime = isOneTime
        }, cancellationToken);

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = $"Alert created: {id}. {symbol} {op} {targetPrice.ToString("0.####", CultureInfo.InvariantCulture)} ({assetType})."
        };
    }

    private static BotCommandExecutionDto BuildHelpResult()
    {
        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText =
                "Usage:\n" +
                "/alert list\n" +
                "/alert <symbol> <>|< <price> [stock|crypto] [--repeat]\n" +
                "Examples: /alert BTC > 110000 | /alert VNM < 65 stock"
        };
    }
}
