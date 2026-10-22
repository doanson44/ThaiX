using System.Text.Json;
using System.Text.RegularExpressions;
using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotConversationalResponder : IBotConversationalResponder
{
    private const string MarketIntentSystemPrompt =
        "You classify a user's message for a crypto market conversation. " +
        "Return only JSON with fields needsMarketData (boolean), assetType (string), and symbol (string or null). " +
        "Use assetType=crypto only when the message clearly refers to a cryptocurrency. " +
        "Set needsMarketData=true when the user asks about price, analysis, outlook, or whether to buy, sell, or hold a coin. " +
        "If a coin is clearly mentioned in a market-related question, resolve its common ticker only when confident. " +
        "Return the base ticker such as XRP or BTC, not a guessed trading pair. " +
        "For stocks, ambiguous symbols, unrelated chat, or uncertain coin identity, set needsMarketData=false and symbol=null.";

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
            var marketDataPrompt =
                $"User message:\n{userMessage}\n\n" +
                "Verified market data retrieved from MEXC spot 24-hour ticker (values are source strings; do not alter them):\n" +
                $"Symbol: {ticker.Symbol}\n" +
                $"Last price: {ticker.LastPrice ?? "unavailable"}\n" +
                $"24-hour price change: {ticker.PriceChange ?? "unavailable"}\n" +
                $"24-hour change percent: {ticker.PriceChangePercent ?? "unavailable"}%\n" +
                $"24-hour open: {ticker.OpenPrice ?? "unavailable"}\n" +
                $"24-hour high: {ticker.HighPrice ?? "unavailable"}\n" +
                $"24-hour low: {ticker.LowPrice ?? "unavailable"}\n" +
                $"24-hour base volume: {ticker.Volume ?? "unavailable"}\n" +
                $"24-hour quote volume: {ticker.QuoteVolume ?? "unavailable"}\n" +
                $"Best bid: {ticker.BidPrice ?? "unavailable"} (quantity: {ticker.BidQty ?? "unavailable"})\n" +
                $"Best ask: {ticker.AskPrice ?? "unavailable"} (quantity: {ticker.AskQty ?? "unavailable"})\n" +
                $"Trade count: {ticker.Count?.ToString() ?? "unavailable"}\n" +
                $"Ticker close time (UTC): {FormatTimestamp(ticker.CloseTime)}\n\n" +
                "Analyze this data in the context of the user's actual question. Separate observed facts from interpretation. " +
                "Discuss plausible scenarios, key risks, and what would invalidate each scenario when relevant. " +
                "Do not claim the 24-hour ticker proves a trend, do not invent technical indicators or external facts, " +
                "and do not present a guaranteed return or personalized financial certainty. " +
                "If the data is insufficient for a conclusion, say so.";

            var analysisSystemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                ? "You are a careful cryptocurrency market analyst."
                : systemPrompt.Trim();

            analysisSystemPrompt +=
                "\n\nWhen verified market data is supplied in the user prompt, ground the analysis in those exact values. " +
                "Never fabricate current prices, indicators, news, support/resistance levels, or exchange data. " +
                "Clearly distinguish observed 24-hour statistics from hypotheses and future scenarios.";

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
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        }
        catch (ArgumentOutOfRangeException)
        {
            return "unavailable";
        }
    }
}
