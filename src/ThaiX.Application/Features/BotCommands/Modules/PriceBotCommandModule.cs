using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockPrices;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.LookupSymbol;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

/// <summary>
/// /p command: auto-detects the asset type of a symbol then delegates to the appropriate price source.
/// "/p btc"  -> resolves to CryptoSpot (BTCUSDT), returns MEXC 24h ticker.
/// "/p vnm"  -> resolves to VnStock (VNM), returns VnDirect latest close.
/// </summary>
public sealed class PriceBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "p",
        Aliases = ["price"],
        Syntax = "/p <symbol>",
        Description = "Get price for any symbol. Auto-detects asset type (VnStock or CryptoSpot).",
        Category = BotCommandCategory.MarketData,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public PriceBotCommandModule(IMediator mediator)
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
                PlainText = "Usage: /p <symbol>  (e.g. /p btc  or  /p vnm)",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        var rawSymbol = context.ParsedCommand.Arguments[0].Trim();

        // Resolve symbol to asset type via LookupSymbolQuery.
        var lookup = await _mediator.Send(new LookupSymbolQuery(rawSymbol), cancellationToken);
        if (lookup is null)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Failed,
                PlainText = $"Symbol '{rawSymbol.ToUpperInvariant()}' not found in VnStock or CryptoSpot.",
                ErrorCode = ErrorCodes.RESOURCE_NOT_FOUND
            };
        }

        return lookup.AssetType == nameof(AssetType.VnStock)
            ? await FetchVnStockAsync(lookup.Symbol, cancellationToken)
            : await FetchCryptoSpotAsync(lookup.Symbol, cancellationToken);
    }

    private async Task<BotCommandExecutionDto> FetchVnStockAsync(string symbol, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVnDirectStockPricesQuery { Code = symbol }, cancellationToken);

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
        };    // closes return new BotCommandExecutionDto
    }        // closes FetchVnStockAsync

    private async Task<BotCommandExecutionDto> FetchCryptoSpotAsync(string symbol, CancellationToken cancellationToken)
    {
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
        var plainText = $"{data.Symbol} [CryptoSpot]: last={data.LastPrice ?? "n/a"}, change24h={data.PriceChangePercent ?? "n/a"}%, high={data.HighPrice ?? "n/a"}, low={data.LowPrice ?? "n/a"}.";

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

    private static string FormatPrice(decimal value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatPct(decimal value)
        => value.ToString("+0.##;-0.##", CultureInfo.InvariantCulture);

    private static string FormatVolumeM(decimal value)
        => (value / 1_000_000m).ToString("0.##", CultureInfo.InvariantCulture);
}
