using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockPrices;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class PriceStockBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "ps",
        Aliases = ["prices", "price-stock"],
        Syntax = "/ps <symbol>",
        Description = "Get latest Vietnam stock close and day change.",
        Category = BotCommandCategory.MarketData,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public PriceStockBotCommandModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        if (context.ParsedCommand.Arguments.Count == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Usage: /ps <symbol>",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var symbol = context.ParsedCommand.Arguments[0].Trim().ToUpperInvariant();
        var result = await _mediator.Send(
            new GetVnDirectStockPricesQuery { Code = symbol },
            cancellationToken);

        if (!result.Success || result.Data.Count == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Failed,
                PlainText = result.Message ?? $"Could not load stock data for '{symbol}'.",
                ErrorCode = ErrorCodes.EXTERNAL_SERVICE_ERROR
            };
        }

        var latest = result.Data[0];
        var plainText = $"{latest.Code} | Vietnam Stock: price={FormatPrice(latest.Close)}, change={FormatPct(latest.PctChange)}%, vol={FormatVolumeM(latest.NmVolume)}M.";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Vietnam Stock",
                Title = latest.Code,
                Type = NotificationCardType.Metric,
                Metrics =
                [
                    new() { Label = "Price", Value = FormatPrice(latest.Close) },
                    new() { Label = "Change", Value = FormatPct(latest.PctChange) + "%" },
                    new() { Label = "Volume", Value = FormatVolumeM(latest.NmVolume) + "M" },
                    new() { Label = "Day Range", Value = FormatPrice(latest.Low) + " - " + FormatPrice(latest.High) }
                ]
            }
        };
    }

    private static string FormatPrice(decimal value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatPct(decimal value)
        => value.ToString("+0.##;-0.##", CultureInfo.InvariantCulture);

    private static string FormatVolumeM(decimal value)
        => (value / 1_000_000m).ToString("0.##", CultureInfo.InvariantCulture);
}
