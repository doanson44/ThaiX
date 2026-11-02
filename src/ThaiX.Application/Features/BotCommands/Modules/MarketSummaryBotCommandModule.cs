using System.Globalization;
using MediatR;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class MarketSummaryBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "market-summary",
        Aliases = ["market-today", "vn-market", "tong-hop-thi-truong"],
        Syntax = "/market-summary",
        Description = "Summarize broad market conditions using VIX, VNINDEX technical indicators, MEXC gold, and BTC spot market data.",
        Category = BotCommandCategory.TechnicalAnalysis,
        RequiredPermissions = [Permissions.TradingView],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public MarketSummaryBotCommandModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(
        BotCommandContext context,
        CancellationToken cancellationToken)
    {
        var vnIndexTask = SendSafelyAsync(
            new GetInvestingVnIndexAnalysisQuery(),
            ex => new InvestingVnIndexAnalysisResponse { Success = false, Message = ex.Message },
            cancellationToken);
        var goldTask = SendSafelyAsync(
            new GetMexcGoldAnalysisQuery(),
            ex => new MexcGoldAnalysisResponse { Symbol = "GOLD(XAUT)USDT", Success = false, Message = ex.Message },
            cancellationToken);

        var vixSafeTask = SendSafelyAsync(
            new GetYahooFinanceVixQuery(),
            ex => new YahooFinanceVixResponse { Symbol = "^VIX", Success = false, Message = ex.Message },
            cancellationToken);
        var btcTask = SendSafelyAsync(
            new GetMexcSpotTicker24HrBySymbolQuery { Symbol = "BTCUSDT" },
            ex => new MexcSpotTicker24HrBySymbolResponse { Symbol = "BTCUSDT", Success = false, Message = ex.Message },
            cancellationToken);

        await Task.WhenAll(vixSafeTask, vnIndexTask, goldTask, btcTask);

        var vix = await vixSafeTask;
        var vnIndex = await vnIndexTask;
        var gold = await goldTask;
        var btc = await btcTask;

        if (!vix.Success && !vnIndex.Success && !gold.Success && (!btc.Success || btc.Data is null))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Failed,
                PlainText = "Market summary is unavailable because VIX, VNINDEX, MEXC gold, and BTC data could not be retrieved. " +
                    $"VIX: {vix.Message ?? "unavailable"}. VNINDEX: {vnIndex.Message ?? "unavailable"}. " +
                    $"Gold: {gold.Message ?? "unavailable"}. BTC: {btc.Message ?? "unavailable"}.",
                ErrorCode = ErrorCodes.EXTERNAL_SERVICE_ERROR
            };
        }

        var lines = new List<string>
        {
            "Vietnam market summary source data. Interpret the following verified source values; do not invent missing market context."
        };

        if (vix.Success)
        {
            lines.Add(
                $"VIX (^VIX): value={Format(vix.CurrentValue ?? vix.CurrentPrice)}, " +
                $"previousClose={Format(vix.PreviousClose)}, dayHigh={Format(vix.DayHigh)}, " +
                $"dayLow={Format(vix.DayLow)}, regime={vix.Regime ?? "unavailable"}, " +
                $"percentile={vix.Percentile.ToString("P1", CultureInfo.InvariantCulture)}, " +
                $"spike={vix.IsSpike}, ema10={Format(vix.Ema10)}, ema20={Format(vix.Ema20)}, " +
                $"momentum3d={FormatPercent(vix.Momentum3D)}, momentum5d={FormatPercent(vix.Momentum5D)}, " +
                $"updatedAt={FormatTimestamp(vix.LastUpdateTime)}.");
        }
        else
        {
            lines.Add($"VIX: unavailable. Source message: {vix.Message ?? "no data returned"}.");
        }

        if (vnIndex.Success)
        {
            lines.Add(
                $"VNINDEX (daily): latestTime={FormatTimestamp(vnIndex.LatestTime)}, " +
                $"open={Format(vnIndex.Open)}, high={Format(vnIndex.High)}, low={Format(vnIndex.Low)}, " +
                $"close={Format(vnIndex.Close)}, volume={Format(vnIndex.Volume)}, " +
                $"trend={vnIndex.Trend ?? "unavailable"}, momentum={vnIndex.Momentum ?? "unavailable"}, " +
                $"setup={vnIndex.Setup ?? "unavailable"}, signal={vnIndex.Signal ?? "unavailable"}, " +
                $"confidence={Format(vnIndex.Confidence)}%, ema9={Format(vnIndex.Ema9)}, " +
                $"ema21={Format(vnIndex.Ema21)}, ema50={Format(vnIndex.Ema50)}, " +
                $"rsi14={Format(vnIndex.Rsi14)}, atr14={Format(vnIndex.Atr14)}, " +
                $"resistance20d={Format(vnIndex.HighestHigh20)}, support20d={Format(vnIndex.LowestLow20)}, " +
                $"averageVolume20d={Format(vnIndex.AverageVolume20)}.");
        }
        else
        {
            lines.Add($"VNINDEX: unavailable. Source message: {vnIndex.Message ?? "no data returned"}.");
        }

        if (gold.Success)
        {
            lines.Add(
                $"MEXC gold ({gold.Symbol}): price={Format(gold.Price)}, open={Format(gold.BopPrice)}, " +
                $"change={Format(gold.Change)}, changePct={Format(gold.ChangePct)}%, high={Format(gold.High)}, " +
                $"low={Format(gold.Low)}, volume={Format(gold.Volume)}, quoteVolume={Format(gold.QuoteVolume)}, " +
                $"dailyCandleDate={gold.LastUpdated}.");
        }
        else
        {
            lines.Add($"MEXC gold: unavailable. Source message: {gold.Message ?? "no data returned"}.");
        }

        if (btc.Success && btc.Data is not null)
        {
            lines.Add(
                $"Bitcoin spot ({btc.Data.Symbol}): last={btc.Data.LastPrice ?? "unavailable"}, " +
                $"change24h={btc.Data.PriceChange ?? "unavailable"} ({btc.Data.PriceChangePercent ?? "unavailable"}%), " +
                $"high24h={btc.Data.HighPrice ?? "unavailable"}, low24h={btc.Data.LowPrice ?? "unavailable"}, " +
                $"quoteVolume24h={btc.Data.QuoteVolume ?? "unavailable"}, " +
                $"updatedAt={FormatTimestamp(btc.Data.CloseTime > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(btc.Data.CloseTime) : null)}.");
        }
        else
        {
            lines.Add($"Bitcoin spot (BTCUSDT): unavailable. Source message: {btc.Message ?? "no data returned"}.");
        }

        var metrics = new List<NotificationMetric>();
        if (vix.Success)
        {
            metrics.Add(new NotificationMetric { Label = "VIX", Value = Format(vix.CurrentValue ?? vix.CurrentPrice) });
            metrics.Add(new NotificationMetric { Label = "VIX Regime", Value = vix.Regime ?? "unavailable" });
            metrics.Add(new NotificationMetric { Label = "VIX Percentile", Value = vix.Percentile.ToString("P1", CultureInfo.InvariantCulture) });
            metrics.Add(new NotificationMetric { Label = "VIX Spike", Value = vix.IsSpike ? "Yes" : "No" });
        }

        if (vnIndex.Success)
        {
            metrics.Add(new NotificationMetric { Label = "VNINDEX Close", Value = Format(vnIndex.Close) });
            metrics.Add(new NotificationMetric { Label = "Trend", Value = vnIndex.Trend ?? "unavailable" });
            metrics.Add(new NotificationMetric { Label = "Momentum", Value = vnIndex.Momentum ?? "unavailable" });
            metrics.Add(new NotificationMetric { Label = "Signal", Value = vnIndex.Signal ?? "unavailable" });
            metrics.Add(new NotificationMetric { Label = "RSI 14", Value = Format(vnIndex.Rsi14) });
            metrics.Add(new NotificationMetric { Label = "EMA 9 / 21 / 50", Value = $"{Format(vnIndex.Ema9)} / {Format(vnIndex.Ema21)} / {Format(vnIndex.Ema50)}" });
            metrics.Add(new NotificationMetric { Label = "Resistance 20D", Value = Format(vnIndex.HighestHigh20) });
            metrics.Add(new NotificationMetric { Label = "Support 20D", Value = Format(vnIndex.LowestLow20) });
        }

        if (gold.Success)
        {
            metrics.Add(new NotificationMetric { Label = "Gold Price", Value = Format(gold.Price) });
            metrics.Add(new NotificationMetric { Label = "Gold Daily Change", Value = $"{Format(gold.Change)} ({Format(gold.ChangePct)}%)" });
            metrics.Add(new NotificationMetric { Label = "Gold Daily Range", Value = $"{Format(gold.Low)} - {Format(gold.High)}" });
        }

        if (btc.Success && btc.Data is not null)
        {
            metrics.Add(new NotificationMetric { Label = "BTC Price (USDT)", Value = btc.Data.LastPrice ?? "unavailable" });
            metrics.Add(new NotificationMetric { Label = "BTC Change 24h", Value = $"{btc.Data.PriceChange ?? "unavailable"} ({btc.Data.PriceChangePercent ?? "unavailable"}%)" });
            metrics.Add(new NotificationMetric { Label = "BTC High / Low 24h", Value = $"{btc.Data.HighPrice ?? "unavailable"} / {btc.Data.LowPrice ?? "unavailable"}" });
        }

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = string.Join(Environment.NewLine, lines),
            Card = new NotificationCard
            {
                Category = "Market Summary",
                Title = "Market Overview: VIX + VNINDEX + Gold + BTC",
                Type = NotificationCardType.Summary,
                Metrics = metrics
            }
        };
    }

    private async Task<TResponse> SendSafelyAsync<TResponse>(
        IRequest<TResponse> query,
        Func<Exception, TResponse> failureFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _mediator.Send(query, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return failureFactory(ex);
        }
    }

    private static string Format(decimal? value)
        => value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "unavailable";

    private static string FormatPercent(decimal? value)
        => value?.ToString("+0.##%;-0.##%;0%", CultureInfo.InvariantCulture) ?? "unavailable";

    private static string FormatTimestamp(DateTime? value)
        => value.HasValue
            ? value.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture)
            : "unavailable";

    private static string FormatTimestamp(DateTimeOffset? value)
        => value.HasValue
            ? value.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture)
            : "unavailable";
}
