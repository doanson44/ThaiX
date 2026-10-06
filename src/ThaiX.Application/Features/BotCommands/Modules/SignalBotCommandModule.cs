using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Trading;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class SignalBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "signal",
        Aliases = ["sg"],
        Syntax = "/signal <symbol> [timeframe] [--stock|--crypto]",
        Description = "Generate a trading signal from the technical engine.",
        Category = BotCommandCategory.TradingSignals,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public SignalBotCommandModule(IMediator mediator)
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
                PlainText = "Usage: /signal <symbol> [timeframe] [--stock|--crypto]",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var marketType = ResolveMarketType(context.ParsedCommand);
        var timeframe = ResolveTimeframe(context.ParsedCommand, marketType);
        var symbol = NormalizeSymbol(context.ParsedCommand.Arguments[0], marketType);
        var useLongTerm = marketType == MarketType.Stock
                          && (timeframe.Equals("Week1", StringComparison.OrdinalIgnoreCase)
                              || timeframe.Equals("Month1", StringComparison.OrdinalIgnoreCase));

        var suggestion = await _mediator.Send(new GetTradeSuggestionQuery
        {
            Symbol = symbol,
            MarketType = marketType,
            Timeframe = timeframe,
            UseLongTermTimeframe = useLongTerm,
            MarketRegime = MarketRegime.RiskOn,
            EventRisk = EventRisk.Low
        }, cancellationToken);

        var plainText =
            $"{symbol} [{timeframe}] => signal={suggestion.Signal}, trend={suggestion.Trend}, setup={suggestion.Setup}, confidence={suggestion.Confidence.ToString("0.##", CultureInfo.InvariantCulture)}.";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Signal",
                Title = symbol,
                Type = NotificationCardType.Signal,
                Metrics =
                [
                    new() { Label = "Direction", Value = suggestion.Signal },
                    new() { Label = "Confidence", Value = $"{suggestion.Confidence.ToString("0.##", CultureInfo.InvariantCulture)}%" },
                    new() { Label = "Entry", Value = FormatNullableDecimal(suggestion.EntryPrice) },
                    new() { Label = "Stop Loss", Value = FormatNullableDecimal(suggestion.StopLoss) },
                    new() { Label = "Target 1", Value = FormatNullableDecimal(suggestion.TakeProfit1) },
                    new() { Label = "Target 2", Value = FormatNullableDecimal(suggestion.TakeProfit2) }
                ]
            }
        };
    }

    private static MarketType ResolveMarketType(BotParsedCommand command)
    {
        if (command.HasFlag("--stock"))
        {
            return MarketType.Stock;
        }

        if (command.HasFlag("--crypto"))
        {
            return MarketType.CryptoSpot;
        }

        return MarketType.CryptoSpot;
    }

    private static string ResolveTimeframe(BotParsedCommand command, MarketType marketType)
    {
        if (command.Arguments.Count > 1)
        {
            return command.Arguments[1].Trim();
        }

        return marketType == MarketType.Stock ? "Day1" : "Hour4";
    }

    private static string NormalizeSymbol(string symbol, MarketType marketType)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        if (marketType == MarketType.Stock)
        {
            return normalized;
        }

        if (!normalized.EndsWith("USDT", StringComparison.OrdinalIgnoreCase) && normalized.All(char.IsLetter))
        {
            normalized += "USDT";
        }

        return normalized;
    }

    private static string FormatNullableDecimal(decimal? value)
        => value.HasValue
            ? value.Value.ToString("0.####", CultureInfo.InvariantCulture)
            : "n/a";
}
