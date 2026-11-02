using System.Text.Json;
using FluentAssertions;
using Moq;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotKlines;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcSpotKlinesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSymbolContainsOneBalancedParenthesis_ReturnsSuccess()
    {
        using var document = JsonDocument.Parse(
            """
            [
                [1700086400000, "2000.00", "2050.00", "1980.00", "2025.00", "10.0", 1700172799999, "20250.00"]
            ]
            """);
        var rows = document.RootElement.EnumerateArray()
            .Select(row => row.EnumerateArray().Select(value => value.Clone()).ToList())
            .ToList();

        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT",
                "1d",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);

        var handler = new GetMexcSpotKlinesQueryHandler(externalDataService.Object);

        var result = await handler.Handle(
            new GetMexcSpotKlinesQuery { Symbol = "GOLD(XAUT)USDT", Interval = "1d" },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Symbol.Should().Be("GOLD(XAUT)USDT");
        result.Data.Should().ContainSingle();
        result.Data[0].ClosePrice.Should().Be("2025.00");
    }

    [Fact]
    public async Task Handle_WhenSymbolContainsMultipleParenthesisPairs_ReturnsFailure()
    {
        var externalDataService = new Mock<IExternalDataService>();
        var handler = new GetMexcSpotKlinesQueryHandler(externalDataService.Object);

        var result = await handler.Handle(
            new GetMexcSpotKlinesQuery { Symbol = "GOLD((XAUT))USDT", Interval = "1d" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid symbol");
        externalDataService.Verify(
            service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
