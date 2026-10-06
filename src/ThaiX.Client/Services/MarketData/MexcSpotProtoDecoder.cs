using System.Text;

namespace ThaiX.Client.Services.MarketData;

/// <summary>
/// Zero-dependency protobuf binary decoder for MEXC Spot WebSocket streams.
/// Only decodes PushDataV3ApiWrapper and PublicMiniTickersV3Api / PublicMiniTickerV3Api.
/// See: https://github.com/mexcdevelop/websocket-proto
/// </summary>
internal static class MexcSpotProtoDecoder
{
    // Field numbers in PushDataV3ApiWrapper (oneof body)
    private const int WrapperFieldChannel = 1;
    private const int WrapperFieldSymbol = 3;
    private const int WrapperFieldSendTime = 6;
    private const int WrapperFieldPublicMiniTickers = 310;

    // Field numbers in PublicMiniTickersV3Api
    private const int MiniTickersFieldItems = 1;

    // Wire types
    private const int WireVarint = 0;
    private const int WireLenDelimited = 2;

    internal readonly struct WrapperResult
    {
        public string Channel { get; init; }
        public string Symbol { get; init; }
        public long SendTime { get; init; }
        public bool HasMiniTickers { get; init; }
        public ReadOnlyMemory<byte> MiniTickersBytes { get; init; }
    }

    internal readonly struct MiniTickerItem
    {
        public string Symbol { get; init; }
        public string Price { get; init; }
        public string Rate { get; init; }
        public string ZonedRate { get; init; }
        public string High { get; init; }
        public string Low { get; init; }
        public string Volume { get; init; }
        public string Quantity { get; init; }
        public string LastCloseRate { get; init; }
        public string LastCloseZonedRate { get; init; }
        public string LastCloseHigh { get; init; }
        public string LastCloseLow { get; init; }
    }

    public static bool TryParseWrapper(ReadOnlySpan<byte> data, out WrapperResult result)
    {
        result = default;

        string channel = string.Empty;
        string symbol = string.Empty;
        long sendTime = 0;
        bool hasMiniTickers = false;
        ReadOnlyMemory<byte> miniTickersBytes = ReadOnlyMemory<byte>.Empty;

        int pos = 0;
        while (pos < data.Length)
        {
            if (!TryReadVarint(data, ref pos, out long rawTag)) break;
            int fieldNumber = (int)(rawTag >> 3);
            int wireType = (int)(rawTag & 0x7);

            if (wireType == WireVarint)
            {
                if (!TryReadVarint(data, ref pos, out long val)) return false;
                if (fieldNumber == WrapperFieldSendTime) sendTime = val;
            }
            else if (wireType == WireLenDelimited)
            {
                if (!TryReadVarint(data, ref pos, out long len)) return false;
                int length = (int)len;
                if (pos + length > data.Length) return false;

                var slice = data.Slice(pos, length);
                switch (fieldNumber)
                {
                    case WrapperFieldChannel:
                        channel = Encoding.UTF8.GetString(slice);
                        break;
                    case WrapperFieldSymbol:
                        symbol = Encoding.UTF8.GetString(slice);
                        break;
                    case WrapperFieldPublicMiniTickers:
                        hasMiniTickers = true;
                        miniTickersBytes = slice.ToArray();
                        break;
                }

                pos += length;
            }
            else
            {
                if (!SkipField(data, ref pos, wireType)) return false;
            }
        }

        result = new WrapperResult
        {
            Channel = channel,
            Symbol = symbol,
            SendTime = sendTime,
            HasMiniTickers = hasMiniTickers,
            MiniTickersBytes = miniTickersBytes
        };
        return true;
    }

    public static List<MiniTickerItem> ParseMiniTickers(ReadOnlySpan<byte> data)
    {
        var items = new List<MiniTickerItem>();
        int pos = 0;

        while (pos < data.Length)
        {
            if (!TryReadVarint(data, ref pos, out long rawTag)) break;
            int fieldNumber = (int)(rawTag >> 3);
            int wireType = (int)(rawTag & 0x7);

            if (wireType == WireLenDelimited)
            {
                if (!TryReadVarint(data, ref pos, out long len)) break;
                int length = (int)len;
                if (pos + length > data.Length) break;

                if (fieldNumber == MiniTickersFieldItems)
                {
                    items.Add(ParseSingleTicker(data.Slice(pos, length)));
                }

                pos += length;
            }
            else
            {
                if (!SkipField(data, ref pos, wireType)) break;
            }
        }

        return items;
    }

    private static MiniTickerItem ParseSingleTicker(ReadOnlySpan<byte> data)
    {
        string symbol = string.Empty, price = string.Empty, rate = string.Empty;
        string zonedRate = string.Empty, high = string.Empty, low = string.Empty;
        string volume = string.Empty, quantity = string.Empty;
        string lastCloseRate = string.Empty, lastCloseZonedRate = string.Empty;
        string lastCloseHigh = string.Empty, lastCloseLow = string.Empty;

        int pos = 0;
        while (pos < data.Length)
        {
            if (!TryReadVarint(data, ref pos, out long rawTag)) break;
            int fieldNumber = (int)(rawTag >> 3);
            int wireType = (int)(rawTag & 0x7);

            if (wireType == WireLenDelimited)
            {
                if (!TryReadVarint(data, ref pos, out long len)) break;
                int length = (int)len;
                if (pos + length > data.Length) break;

                string str = Encoding.UTF8.GetString(data.Slice(pos, length));
                pos += length;

                switch (fieldNumber)
                {
                    case 1: symbol = str; break;
                    case 2: price = str; break;
                    case 3: rate = str; break;
                    case 4: zonedRate = str; break;
                    case 5: high = str; break;
                    case 6: low = str; break;
                    case 7: volume = str; break;
                    case 8: quantity = str; break;
                    case 9: lastCloseRate = str; break;
                    case 10: lastCloseZonedRate = str; break;
                    case 11: lastCloseHigh = str; break;
                    case 12: lastCloseLow = str; break;
                }
            }
            else
            {
                if (!SkipField(data, ref pos, wireType)) break;
            }
        }

        return new MiniTickerItem
        {
            Symbol = symbol,
            Price = price,
            Rate = rate,
            ZonedRate = zonedRate,
            High = high,
            Low = low,
            Volume = volume,
            Quantity = quantity,
            LastCloseRate = lastCloseRate,
            LastCloseZonedRate = lastCloseZonedRate,
            LastCloseHigh = lastCloseHigh,
            LastCloseLow = lastCloseLow
        };
    }

    private static bool TryReadVarint(ReadOnlySpan<byte> data, ref int pos, out long value)
    {
        value = 0;
        int shift = 0;
        while (pos < data.Length)
        {
            byte b = data[pos++];
            value |= (long)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) return true;
            shift += 7;
            if (shift >= 64) return false;
        }
        return false;
    }

    private static bool SkipField(ReadOnlySpan<byte> data, ref int pos, int wireType)
    {
        switch (wireType)
        {
            case 0:
                while (pos < data.Length)
                {
                    byte b = data[pos++];
                    if ((b & 0x80) == 0) return true;
                }
                return false;
            case 1:
                pos += 8;
                return pos <= data.Length;
            case 2:
                if (!TryReadVarint(data, ref pos, out long len)) return false;
                pos += (int)len;
                return pos <= data.Length;
            case 5:
                pos += 4;
                return pos <= data.Length;
            default:
                return false;
        }
    }
}
