using MediatR;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class PriceCryptoBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "pc",
        Aliases = ["pricec", "price-crypto"],
        Syntax = "/pc <symbol>",
        Description = "Get latest crypto price and 24h stats.",
        Category = BotCommandCategory.MarketData,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public PriceCryptoBotCommandModule(IMediator mediator)
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
                PlainText = "Usage: /pc <symbol>",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var symbol = NormalizeCryptoSymbol(context.ParsedCommand.Arguments[0]);
        var result = await _mediator.Send(
            new GetMexcSpotTicker24HrBySymbolQuery { Symbol = symbol },
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Failed,
                PlainText = result.Message ?? $"Could not load market data for '{symbol}'.",
                ErrorCode = ErrorCodes.EXTERNAL_SERVICE_ERROR
            };
        }

        var data = result.Data;
        var plainText =
            $"{data.Symbol}: last={data.LastPrice ?? "n/a"}, change24h={data.PriceChangePercent ?? "n/a"}%, high={data.HighPrice ?? "n/a"}, low={data.LowPrice ?? "n/a"}.";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Crypto Spot",
                Title = data.Symbol,
                Type = NotificationCardType.Metric,
                Metrics =
                [
                    new() { Label = "Price", Value = data.LastPrice ?? "n/a" },
                    new() { Label = "Change", Value = $"{data.PriceChangePercent ?? "n/a"}%" },
                    new() { Label = "High 24h", Value = data.HighPrice ?? "n/a" },
                    new() { Label = "Low 24h", Value = data.LowPrice ?? "n/a" },
                    new() { Label = "Volume", Value = data.Volume ?? "n/a" }
                ]
            }
        };
    }

    private static string NormalizeCryptoSymbol(string symbol)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        if (!normalized.EndsWith("USDT", StringComparison.OrdinalIgnoreCase) && normalized.All(char.IsLetter))
        {
            normalized += "USDT";
        }

        return normalized;
    }
}
