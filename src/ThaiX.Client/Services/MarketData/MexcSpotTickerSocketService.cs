using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.ExternalData;

namespace ThaiX.Client.Services.MarketData;

/// <summary>
/// Connects to the MEXC Spot WebSocket endpoint and subscribes to the
/// spot@public.miniTickers.v3.api.pb channel. Messages are pushed every ~3 seconds
/// in protobuf binary format (PushDataV3ApiWrapper / PublicMiniTickersV3Api).
/// JSON text frames (subscription ack, errors) are also handled.
/// </summary>
public sealed class MexcSpotTickerSocketService : IMexcSpotTickerSocketService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(2);

    // Official MEXC Spot WS endpoint (v3 API)
    private const string WsEndpoint = "wss://wbs-api.mexc.com/ws";

    // Delivers all spot mini-tickers every ~3 s in protobuf binary.
    // Timezone suffix is required; UTC+8 matches MEXC's native timezone.
    private const string SubscribeChannel = "spot@public.miniTickers.v3.api.pb@UTC+8";

    private readonly Dictionary<string, MexcSpotTicker24HrDto> _tickerMap = new(StringComparer.OrdinalIgnoreCase);
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _streamCts;
    private Task? _streamTask;
    private bool _blockedNotified;

    public event Action<IReadOnlyCollection<MexcSpotTicker24HrDto>>? TickersUpdated;
    public event Action<string>? StreamError;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_streamTask is { IsCompleted: false })
        {
            return Task.CompletedTask;
        }

        _streamCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _streamTask = RunStreamAsync(_streamCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_streamCts is not null)
        {
            _streamCts.Cancel();
            _streamCts.Dispose();
            _streamCts = null;
        }

        if (_streamTask is not null)
        {
            try { await _streamTask; } catch { /* cancellation during teardown */ }
            _streamTask = null;
        }

        if (_socket is not null)
        {
            _socket.Dispose();
            _socket = null;
        }
    }

    private async Task RunStreamAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _socket = new ClientWebSocket();
                await _socket.ConnectAsync(new Uri(WsEndpoint), cancellationToken);
                await SendJsonAsync(
                    new { method = "SUBSCRIPTION", @params = new[] { SubscribeChannel } },
                    cancellationToken);
                await ReceiveLoopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    StreamError?.Invoke(ex.Message);
                }
            }
            finally
            {
                if (_socket is not null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(ReconnectDelay, cancellationToken);
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        // Allocate a larger buffer because miniTickers for all symbols can be large.
        var buffer = new byte[256 * 1024];

        while (_socket is { State: WebSocketState.Open } && !cancellationToken.IsCancellationRequested)
        {
            using var messageStream = new MemoryStream();
            WebSocketReceiveResult result;
            WebSocketMessageType messageType;

            do
            {
                result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close) return;
                messageStream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            messageType = result.MessageType;
            var rawBytes = messageStream.ToArray();

            if (messageType == WebSocketMessageType.Binary)
            {
                ProcessBinaryFrame(rawBytes);
            }
            else
            {
                await ProcessTextFrameAsync(rawBytes, cancellationToken);
            }
        }
    }

    // Binary frame: parse as protobuf PushDataV3ApiWrapper -> PublicMiniTickersV3Api
    private void ProcessBinaryFrame(byte[] rawBytes)
    {
        List<MexcSpotProtoDecoder.MiniTickerItem> items;

        // Some frames carry wrapper + nested miniTickers, while others carry
        // PublicMiniTickersV3Api directly. Support both variants.
        if (MexcSpotProtoDecoder.TryParseWrapper(rawBytes, out var wrapper) && wrapper.HasMiniTickers)
        {
            items = MexcSpotProtoDecoder.ParseMiniTickers(wrapper.MiniTickersBytes.Span);
        }
        else
        {
            items = MexcSpotProtoDecoder.ParseMiniTickers(rawBytes);
        }

        if (items.Count == 0) return;

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.Symbol)) continue;

            var incoming = MapMiniTicker(item);
            UpsertTicker(item.Symbol, incoming);
        }

        EmitOrdered();
    }

    // Text frame: handle subscription ack / errors / JSON-encoded ticker data.
    private async Task ProcessTextFrameAsync(byte[] rawBytes, CancellationToken cancellationToken)
    {
        var rawText = Encoding.UTF8.GetString(rawBytes).Trim();
        if (string.Equals(rawText, "PING", StringComparison.OrdinalIgnoreCase))
        {
            await SendTextAsync("PONG", cancellationToken);
            return;
        }

        // Distinguish binary-that-was-sent-as-text vs true JSON by checking first byte.
        if (rawBytes.Length == 0 || rawBytes[0] != (byte)'{') return;

        JsonDocument? document;
        try { document = JsonDocument.Parse(rawBytes); }
        catch (JsonException) { return; }

        using (document)
        {
            var root = document.RootElement;

            // Handle "Blocked" or other server error messages.
            if (root.TryGetProperty("msg", out var msgProp))
            {
                var msgText = msgProp.GetString();
                if (!_blockedNotified &&
                    !string.IsNullOrWhiteSpace(msgText) &&
                    msgText.Contains("Blocked", StringComparison.OrdinalIgnoreCase))
                {
                    _blockedNotified = true;
                    StreamError?.Invoke(msgText);
                }

                if (!string.IsNullOrWhiteSpace(msgText) &&
                    msgText.Contains("PING", StringComparison.OrdinalIgnoreCase))
                {
                    await SendTextAsync("PONG", cancellationToken);
                }

                return;
            }

            if (root.TryGetProperty("ping", out var pingElement) && pingElement.TryGetInt64(out var pingValue))
            {
                await SendJsonAsync(new { pong = pingValue }, cancellationToken);
                return;
            }

            // JSON-encoded miniTickers (docs show this format as an illustration).
            // Property name follows camelCase proto field: "publicMiniTickers"
            if (root.TryGetProperty("publicMiniTickers", out var publicMiniTickers) &&
                publicMiniTickers.TryGetProperty("items", out var itemsElement) &&
                itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    var symbol = GetJsonString(item, "symbol");
                    if (string.IsNullOrEmpty(symbol)) continue;

                    var incoming = new MexcSpotTicker24HrDto
                    {
                        Symbol = NormalizeSymbol(symbol),
                        LastPrice = NullIfEmpty(GetJsonString(item, "price")),
                        Rate = NullIfEmpty(GetJsonString(item, "rate")),
                        ZonedRate = NullIfEmpty(GetJsonString(item, "zonedRate")),
                        HighPrice = NullIfEmpty(GetJsonString(item, "high")),
                        LowPrice = NullIfEmpty(GetJsonString(item, "low")),
                        QuoteVolume = NullIfEmpty(GetJsonString(item, "volume")),
                        Volume = NullIfEmpty(GetJsonString(item, "quantity")),
                        LastCloseRate = NullIfEmpty(GetJsonString(item, "lastCloseRate")),
                        LastCloseZonedRate = NullIfEmpty(GetJsonString(item, "lastCloseZonedRate")),
                        LastCloseHigh = NullIfEmpty(GetJsonString(item, "lastCloseHigh")),
                        LastCloseLow = NullIfEmpty(GetJsonString(item, "lastCloseLow"))
                    };

                    UpsertTicker(symbol, incoming);
                }

                EmitOrdered();
            }
        }
    }

    private void EmitOrdered()
    {
        var ordered = _tickerMap.Values
            .OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase)
            .ToList();
        TickersUpdated?.Invoke(ordered);
    }

    private async Task SendJsonAsync<T>(T payload, CancellationToken cancellationToken)
    {
        if (_socket is not { State: WebSocketState.Open }) return;
        var text = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(text);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task SendTextAsync(string payload, CancellationToken cancellationToken)
    {
        if (_socket is not { State: WebSocketState.Open }) return;
        var bytes = Encoding.UTF8.GetBytes(payload);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static string GetJsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop)) return string.Empty;
        return prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : prop.ToString();
    }

    private static MexcSpotTicker24HrDto MapMiniTicker(MexcSpotProtoDecoder.MiniTickerItem item)
    {
        var lastPrice = TryParseDecimal(item.Price);
        var rate = TryParseDecimal(item.Rate);
        var lastCloseRate = TryParseDecimal(item.LastCloseRate);

        decimal? priceChange = null;
        decimal? openPrice = null;
        decimal? prevClosePrice = null;
        string? priceChangePercent = null;

        if (lastPrice.HasValue && rate.HasValue)
        {
            // MEXC spot proto rates are fractional (e.g. 0.0129 = 1.29%).
            priceChange = lastPrice.Value * rate.Value;
            openPrice = lastPrice.Value - priceChange.Value;
            priceChangePercent = FormatDecimal(rate.Value * 100m);
        }

        if (lastPrice.HasValue && lastCloseRate.HasValue)
        {
            var denominator = 1m + lastCloseRate.Value;
            if (denominator != 0m)
            {
                prevClosePrice = lastPrice.Value / denominator;
            }
        }

        return new MexcSpotTicker24HrDto
        {
            Symbol = item.Symbol,
            LastPrice = NullIfEmpty(item.Price),
            Rate = NullIfEmpty(item.Rate),
            ZonedRate = NullIfEmpty(item.ZonedRate),
            PriceChange = FormatDecimal(priceChange),
            PriceChangePercent = priceChangePercent,
            PrevClosePrice = FormatDecimal(prevClosePrice),
            OpenPrice = FormatDecimal(openPrice),
            HighPrice = NullIfEmpty(item.High),
            LowPrice = NullIfEmpty(item.Low),
            QuoteVolume = NullIfEmpty(item.Volume),
            Volume = NullIfEmpty(item.Quantity),
            LastCloseRate = NullIfEmpty(item.LastCloseRate),
            LastCloseZonedRate = NullIfEmpty(item.LastCloseZonedRate),
            LastCloseHigh = NullIfEmpty(item.LastCloseHigh),
            LastCloseLow = NullIfEmpty(item.LastCloseLow)
        };
    }

    private static decimal? TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string? FormatDecimal(decimal? value)
    {
        if (!value.HasValue) return null;
        return value.Value.ToString("0.########", CultureInfo.InvariantCulture);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrEmpty(value) ? null : value;

    private void UpsertTicker(string symbol, MexcSpotTicker24HrDto incoming)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);

        if (_tickerMap.TryGetValue(normalizedSymbol, out var existing))
        {
            _tickerMap[normalizedSymbol] = MergeSpotTicker(existing, incoming with { Symbol = normalizedSymbol });
        }
        else
        {
            _tickerMap[normalizedSymbol] = incoming with { Symbol = normalizedSymbol };
        }
    }

    private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();

    private static MexcSpotTicker24HrDto MergeSpotTicker(MexcSpotTicker24HrDto existing, MexcSpotTicker24HrDto incoming)
    {
        return existing with
        {
            Symbol = incoming.Symbol,
            PriceChange = incoming.PriceChange ?? existing.PriceChange,
            PriceChangePercent = incoming.PriceChangePercent ?? existing.PriceChangePercent,
            PrevClosePrice = incoming.PrevClosePrice ?? existing.PrevClosePrice,
            LastPrice = incoming.LastPrice ?? existing.LastPrice,
            BidPrice = incoming.BidPrice ?? existing.BidPrice,
            BidQty = incoming.BidQty ?? existing.BidQty,
            AskPrice = incoming.AskPrice ?? existing.AskPrice,
            AskQty = incoming.AskQty ?? existing.AskQty,
            OpenPrice = incoming.OpenPrice ?? existing.OpenPrice,
            HighPrice = incoming.HighPrice ?? existing.HighPrice,
            LowPrice = incoming.LowPrice ?? existing.LowPrice,
            Volume = incoming.Volume ?? existing.Volume,
            QuoteVolume = incoming.QuoteVolume ?? existing.QuoteVolume,
            OpenTime = incoming.OpenTime > 0 ? incoming.OpenTime : existing.OpenTime,
            CloseTime = incoming.CloseTime > 0 ? incoming.CloseTime : existing.CloseTime,
            Count = incoming.Count ?? existing.Count,
            CompositeScore = incoming.CompositeScore ?? existing.CompositeScore,
            ScoreBreakdown = incoming.ScoreBreakdown ?? existing.ScoreBreakdown,
            Rate = incoming.Rate ?? existing.Rate,
            ZonedRate = incoming.ZonedRate ?? existing.ZonedRate,
            LastCloseRate = incoming.LastCloseRate ?? existing.LastCloseRate,
            LastCloseZonedRate = incoming.LastCloseZonedRate ?? existing.LastCloseZonedRate,
            LastCloseHigh = incoming.LastCloseHigh ?? existing.LastCloseHigh,
            LastCloseLow = incoming.LastCloseLow ?? existing.LastCloseLow
        };
    }
}
