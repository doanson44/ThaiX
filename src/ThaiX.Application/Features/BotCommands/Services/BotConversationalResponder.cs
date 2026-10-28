using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Application.Resources;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotConversationalResponder : IBotConversationalResponder
{
    private static string MarketIntentSystemPrompt =>
        BotCommandAiPromptResources.Get("Conversational.MarketIntentSystem");

    private static readonly Regex ValidSymbolPattern = new("^[A-Z0-9]{2,15}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CryptoMentionPattern = new(
        @"\b(coin|crypto|cryptocurrency|token|bitcoin|btc|ethereum|eth|xrp|ripple|solana|bnb|binance coin|dogecoin|doge|ada|cardano|avax|avalanche|chainlink|polkadot|ltc|litecoin|trx|tron|shib|shiba|sui|pepe|bch|xlm|uni|hbar|apt|arb|usdt|usdc)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex AmbiguousCryptoTickerPattern = new(
        @"\b(SOL|LINK|DOT|TON|NEAR|OP)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IAiTextGenerationService _aiTextGenerationService;
    private readonly IMediator _mediator;

    public BotConversationalResponder(
        IAiTextGenerationService aiTextGenerationService,
        IMediator mediator)
    {
        _aiTextGenerationService = aiTextGenerationService;
        _mediator = mediator;
    }

    public async Task<string?> RespondAsync(string message, string systemPrompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var userMessage = message.Trim();

        try
        {
            if (!CryptoMentionPattern.IsMatch(userMessage) && !AmbiguousCryptoTickerPattern.IsMatch(userMessage))
            {
                return await GenerateConversationalResponseAsync(userMessage, systemPrompt, cancellationToken);
            }

            var intentResult = await _aiTextGenerationService.GenerateAsync(
                userMessage,
                MarketIntentSystemPrompt,
                cancellationToken);

            if (!TryReadMarketIntent(intentResult.Text, out var symbol))
            {
                return await GenerateConversationalResponseAsync(userMessage, systemPrompt, cancellationToken);
            }

            var marketDataResult = await _mediator.Send(
                new GetMexcSpotTicker24HrBySymbolQuery { Symbol = symbol },
                cancellationToken);

            if (!marketDataResult.Success || marketDataResult.Data is null)
            {
                return $"I could not retrieve current market data for {symbol}. I do not want to invent a live price or give a price-based analysis without verified data. Please try again shortly.";
            }

            var ticker = marketDataResult.Data;
            var marketDataPrompt = BotCommandAiPromptResources.Format(
                "Conversational.MarketDataUser",
                userMessage,
                ticker.Symbol,
                ticker.LastPrice ?? "unavailable",
                ticker.PriceChange ?? "unavailable",
                ticker.PriceChangePercent ?? "unavailable",
                ticker.OpenPrice ?? "unavailable",
                ticker.HighPrice ?? "unavailable",
                ticker.LowPrice ?? "unavailable",
                ticker.Volume ?? "unavailable",
                ticker.QuoteVolume ?? "unavailable",
                ticker.BidPrice ?? "unavailable",
                ticker.BidQty ?? "unavailable",
                ticker.AskPrice ?? "unavailable",
                ticker.AskQty ?? "unavailable",
                ticker.Count?.ToString(CultureInfo.InvariantCulture) ?? "unavailable",
                FormatTimestamp(ticker.CloseTime));

            var analysisSystemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                ? BotCommandAiPromptResources.Get("Conversational.MarketAnalysisSystemFallback")
                : systemPrompt.Trim();

            analysisSystemPrompt += BotCommandAiPromptResources.Get("Conversational.MarketAnalysisSystemAppend");

            var analysisResult = await _aiTextGenerationService.GenerateAsync(
                marketDataPrompt,
                analysisSystemPrompt,
                cancellationToken);

            return string.IsNullOrWhiteSpace(analysisResult.Text) ? null : analysisResult.Text.Trim();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GenerateConversationalResponseAsync(
        string message,
        string systemPrompt,
        CancellationToken cancellationToken)
    {
        var result = await _aiTextGenerationService.GenerateAsync(message, systemPrompt, cancellationToken);
        return string.IsNullOrWhiteSpace(result.Text) ? null : result.Text.Trim();
    }

    private static bool TryReadMarketIntent(string text, out string symbol)
    {
        symbol = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd <= jsonStart)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(text[jsonStart..(jsonEnd + 1)]);
            var root = document.RootElement;
            if (!root.TryGetProperty("needsMarketData", out var needsMarketData)
                || needsMarketData.ValueKind != JsonValueKind.True
                || !root.TryGetProperty("assetType", out var assetType)
                || !string.Equals(assetType.GetString(), "crypto", StringComparison.OrdinalIgnoreCase)
                || !root.TryGetProperty("symbol", out var symbolElement)
                || symbolElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var candidate = symbolElement.GetString()?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            if (candidate.EndsWith("USDT", StringComparison.Ordinal))
            {
                symbol = candidate;
            }
            else
            {
                if (!ValidSymbolPattern.IsMatch(candidate))
                {
                    return false;
                }

                symbol = candidate + "USDT";
            }

            return ValidSymbolPattern.IsMatch(symbol);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string FormatTimestamp(long unixMilliseconds)
    {
        if (unixMilliseconds <= 0)
        {
            return "unavailable";
        }

        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
        }
        catch (ArgumentOutOfRangeException)
        {
            return "unavailable";
        }
    }
}
