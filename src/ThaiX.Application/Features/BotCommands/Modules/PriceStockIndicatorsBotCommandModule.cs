using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Trading;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class PriceStockIndicatorsBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "psi",
        Aliases = ["ta-s", "indicators-stock"],
        Syntax = "/psi <symbol> [timeframe]",
        Description = "Get stock technical analysis snapshot.",
        Category = BotCommandCategory.TechnicalAnalysis,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public PriceStockIndicatorsBotCommandModule(IMediator mediator)
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
                PlainText = "Usage: /psi <symbol> [timeframe]",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var symbol = context.ParsedCommand.Arguments[0].Trim().ToUpperInvariant();
        var timeframe = context.ParsedCommand.Arguments.Count > 1
            ? context.ParsedCommand.Arguments[1].Trim()
            : "Day1";

        var useLongTerm = timeframe.Equals("Week1", StringComparison.OrdinalIgnoreCase)
                          || timeframe.Equals("Month1", StringComparison.OrdinalIgnoreCase);

        var suggestion = await _mediator.Send(new GetTradeSuggestionQuery
        {
            Symbol = symbol,
            MarketType = MarketType.Stock,
            Timeframe = timeframe,
            UseLongTermTimeframe = useLongTerm,
            MarketRegime = MarketRegime.RiskOn,
            EventRisk = EventRisk.Low
        }, cancellationToken);

        var plainText =
            $"{symbol} [{timeframe}]: signal={suggestion.Signal}, trend={suggestion.Trend}, momentum={suggestion.Momentum}, confidence={suggestion.Confidence.ToString("0.##", CultureInfo.InvariantCulture)}.";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Analysis",
                Title = $"{symbol} [{timeframe}]",
                Type = NotificationCardType.Analysis,
                Metrics =
                [
                    new() { Label = "Trend", Value = suggestion.Trend },
                    new() { Label = "Momentum", Value = suggestion.Momentum },
                    new() { Label = "Setup", Value = suggestion.Setup },
                    new() { Label = "Signal", Value = suggestion.Signal },
                    new() { Label = "Confidence", Value = $"{suggestion.Confidence.ToString("0.##", CultureInfo.InvariantCulture)}%" },
                    new() { Label = "Entry", Value = FormatNullableDecimal(suggestion.EntryPrice) },
                    new() { Label = "Stop Loss", Value = FormatNullableDecimal(suggestion.StopLoss) },
                    new() { Label = "Target 1", Value = FormatNullableDecimal(suggestion.TakeProfit1) },
                    new() { Label = "Target 2", Value = FormatNullableDecimal(suggestion.TakeProfit2) }
                ]
            }
        };
    }

    private static string FormatNullableDecimal(decimal? value)
        => value.HasValue
            ? value.Value.ToString("0.####", CultureInfo.InvariantCulture)
            : "n/a";
}
