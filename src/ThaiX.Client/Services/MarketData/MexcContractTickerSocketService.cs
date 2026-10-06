using System.Globalization;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.ExternalData;

namespace ThaiX.Client.Services.MarketData;

public sealed class MexcContractTickerSocketService : IMexcContractTickerSocketService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(2);

    private readonly Dictionary<string, MexcContractTickerDto> _tickerMap = new(StringComparer.OrdinalIgnoreCase);

    private ClientWebSocket? _socket;
    private CancellationTokenSource? _streamCts;
    private Task? _streamTask;

    public event Action<IReadOnlyCollection<MexcContractTickerDto>>? TickersUpdated;
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
            try
            {
                await _streamTask;
            }
            catch
            {
                // Ignore stream cancellation errors during teardown.
            }

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
                await _socket.ConnectAsync(new Uri("wss://contract.mexc.com/edge"), cancellationToken);
                await SendJsonAsync(new { method = "sub.tickers", param = new { }, gzip = false }, cancellationToken);
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
        var buffer = new byte[16 * 1024];

        while (_socket is { State: WebSocketState.Open } && !cancellationToken.IsCancellationRequested)
        {
            using var messageStream = new MemoryStream();
            WebSocketReceiveResult? result;

            do
            {
                result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                messageStream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            var payloadBytes = messageStream.ToArray();
            var textBytes = TryDecompressGzip(payloadBytes) ?? payloadBytes;
            var message = Encoding.UTF8.GetString(textBytes);
            await ProcessMessageAsync(message, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (string.Equals(message.Trim(), "ping", StringComparison.OrdinalIgnoreCase))
        {
            await SendTextAsync("pong", cancellationToken);
            return;
        }

        JsonDocument? document;
        try
        {
            document = JsonDocument.Parse(message);
        }
        catch (JsonException)
        {
            return;
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.TryGetProperty("ping", out var pingElement) && pingElement.TryGetInt64(out var pingValue))
            {
                await SendJsonAsync(new { pong = pingValue }, cancellationToken);
                return;
            }

            if (root.TryGetProperty("channel", out var statusChannelElement) &&
                string.Equals(statusChannelElement.GetString(), "rs.sub.tickers", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!root.TryGetProperty("data", out var dataElement) ||
                dataElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            // Initial snapshot frames include channel=push.tickers, but incremental
            // updates may only include { data:[...] } without channel.
            if (root.TryGetProperty("channel", out var channelElement))
            {
                var channel = channelElement.GetString();
                if (!string.IsNullOrWhiteSpace(channel) &&
                    !channel.Contains("push.tickers", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            var rootTimestamp = root.TryGetProperty("ts", out var tsElement) && tsElement.TryGetInt64(out var ts)
                ? ts
                : (long?)null;

            foreach (var item in dataElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var symbol = GetString(item, "symbol");
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    continue;
                }

                var normalizedSymbol = NormalizeSymbol(symbol);

                var itemTimestamp = item.TryGetProperty("timestamp", out var itemTsElement) && itemTsElement.TryGetInt64(out var itemTs)
                    ? itemTs
                    : rootTimestamp;

                var incoming = new MexcContractTickerDto
                {
                    Symbol = normalizedSymbol,
                    LastPrice = ParseNullableDecimal(item, "lastPrice"),
                    Bid1 = ParseNullableDecimal(item, "maxBidPrice"),
                    Ask1 = ParseNullableDecimal(item, "minAskPrice"),
                    Volume24 = ParseNullableDecimal(item, "volume24"),
                    Amount24 = ParseNullableDecimal(item, "amount24"),
                    RiseFallRate = ParseNullableDecimal(item, "riseFallRate"),
                    High24Price = ParseNullableDecimal(item, "high24Price"),
                    Low24Price = ParseNullableDecimal(item, "lower24Price"),
                    IndexPrice = ParseNullableDecimal(item, "indexPrice"),
                    FairPrice = ParseNullableDecimal(item, "fairPrice"),
                    Timestamp = itemTimestamp
                };

                if (_tickerMap.TryGetValue(normalizedSymbol, out var existing))
                {
                    _tickerMap[normalizedSymbol] = MergeContractTicker(existing, incoming);
                }
                else
                {
                    _tickerMap[normalizedSymbol] = incoming;
                }
            }

            var ordered = _tickerMap.Values
                .OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase)
                .ToList();

            TickersUpdated?.Invoke(ordered);
        }
    }

    private async Task SendJsonAsync<T>(T payload, CancellationToken cancellationToken)
    {
        if (_socket is not { State: WebSocketState.Open })
        {
            return;
        }

        var text = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(text);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task SendTextAsync(string payload, CancellationToken cancellationToken)
    {
        if (_socket is not { State: WebSocketState.Open })
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(payload);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static byte[]? TryDecompressGzip(byte[] bytes)
    {
        if (bytes.Length < 2 || bytes[0] != 0x1F || bytes[1] != 0x8B)
        {
            return null;
        }

        try
        {
            using var input = new MemoryStream(bytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var propertyElement))
        {
            return string.Empty;
        }

        return propertyElement.ValueKind == JsonValueKind.String
            ? propertyElement.GetString() ?? string.Empty
            : propertyElement.ToString();
    }

    private static decimal? ParseNullableDecimal(JsonElement element, string propertyName)
    {
        var value = GetString(element, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();

    private static MexcContractTickerDto MergeContractTicker(MexcContractTickerDto existing, MexcContractTickerDto incoming)
    {
        return existing with
        {
            Symbol = incoming.Symbol,
            ContractId = incoming.ContractId ?? existing.ContractId,
            LastPrice = incoming.LastPrice ?? existing.LastPrice,
            Bid1 = incoming.Bid1 ?? existing.Bid1,
            Ask1 = incoming.Ask1 ?? existing.Ask1,
            High24Price = incoming.High24Price ?? existing.High24Price,
            Low24Price = incoming.Low24Price ?? existing.Low24Price,
            Volume24 = incoming.Volume24 ?? existing.Volume24,
            Amount24 = incoming.Amount24 ?? existing.Amount24,
            HoldVol = incoming.HoldVol ?? existing.HoldVol,
            RiseFallRate = incoming.RiseFallRate ?? existing.RiseFallRate,
            RiseFallValue = incoming.RiseFallValue ?? existing.RiseFallValue,
            IndexPrice = incoming.IndexPrice ?? existing.IndexPrice,
            FairPrice = incoming.FairPrice ?? existing.FairPrice,
            FundingRate = incoming.FundingRate ?? existing.FundingRate,
            MaxBidPrice = incoming.MaxBidPrice ?? existing.MaxBidPrice,
            MinAskPrice = incoming.MinAskPrice ?? existing.MinAskPrice,
            RiseFallRates = incoming.RiseFallRates ?? existing.RiseFallRates,
            RiseFallRatesOfTimezone = incoming.RiseFallRatesOfTimezone ?? existing.RiseFallRatesOfTimezone,
            Timestamp = incoming.Timestamp ?? existing.Timestamp,
            CompositeScore = incoming.CompositeScore ?? existing.CompositeScore,
            ScoreBreakdown = incoming.ScoreBreakdown ?? existing.ScoreBreakdown
        };
    }
}
