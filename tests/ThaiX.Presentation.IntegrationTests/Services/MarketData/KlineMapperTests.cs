using System.Text.Json;
using ThaiX.Application.Trading;
using ThaiX.Infrastructure.Services.MarketData;

namespace ThaiX.Presentation.IntegrationTests.Services.MarketData;

public sealed class KlineMapperTests
{
    [Fact]
    public void Map_WhenCryptoPayloadIsProvided_ReturnsCryptoKlines()
    {
        KlineRawResponse raw = new MexcContractKlineRawResponse
        {
            Success = true,
            Code = 0,
            Data = new MexcContractKlineRawData
            {
                Time = [1_700_000_000],
                Open = [100m],
                High = [110m],
                Low = [90m],
                Close = [105m],
                Vol = [12m]
            }
        };

        var result = KlineMapper.Map(raw);

        result.Should().ContainSingle()
            .Which.Close.Should().Be(105m);
    }

    [Fact]
    public void Map_WhenSpotCryptoPayloadIsProvided_ReturnsSpotKlines()
    {
        var rows = JsonSerializer.Deserialize<List<List<JsonElement>>>(
            """[[1700000000,"100","110","90","105","12"]]""")!;
        KlineRawResponse raw = rows;

        var result = KlineMapper.Map(raw);

        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new Kline
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000).UtcDateTime,
                Open = 100m,
                High = 110m,
                Low = 90m,
                Close = 105m,
                Volume = 12m
            });
    }

    [Fact]
    public void Map_WhenStockPayloadIsProvided_ReturnsStockKlines()
    {
        KlineRawResponse raw = new VnDirectStockPricesRawResponse
        {
            Data =
            [
                new VnDirectStockPriceRawDto
                {
                    Date = "2026-01-02",
                    Open = 100m,
                    High = 110m,
                    Low = 90m,
                    Close = 105m,
                    NmVolume = 12m
                }
            ]
        };

        var result = KlineMapper.Map(raw);

        result.Should().ContainSingle()
            .Which.Close.Should().Be(105m);
    }
}
