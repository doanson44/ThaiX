using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Banking.Queries.GetVnExpressBankRates;

namespace ThaiX.Application.UnitTests.Features.ExternalData.Banking.Queries.GetVnExpressBankRates;

public sealed class GetVnExpressBankRatesQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetVnExpressBankRatesQueryHandler _sut;

    public GetVnExpressBankRatesQueryHandlerTests()
    {
        _sut = new GetVnExpressBankRatesQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsNull_ShouldReturnFailedResponse()
    {
        // Arrange
        // No setup required because default mocked generic calls return null.

        // Act
        var result = await _sut.Handle(new GetVnExpressBankRatesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Channel.Should().Be(BankRateChannel.Online);
        result.Data.Should().BeEmpty();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WhenBothChannelsHaveData_ShouldReturnOnlyRequestedChannel()
    {
        // Arrange
        Setup(new VnExpressBankRatesApiData
        {
            BankRateOnline = [Row("Vietcombank", 2.1m, 2.4m, 3.5m, 3.5m, 5.9m)],
            BankRateOffline = [Row("Vietcombank", 1.9m, 2.2m, 3.3m, 3.3m, 5.5m)]
        });

        // Act
        var result = await _sut.Handle(
            new GetVnExpressBankRatesQuery { Channel = BankRateChannel.Offline },
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Channel.Should().Be(BankRateChannel.Offline);
        result.Data.Should().ContainSingle();
        result.Data[0].Month12.Should().Be(5.5m);
    }

    [Fact]
    public async Task Handle_WhenRowsHaveMissingTermOrBlankAttributes_ShouldNormalizeThem()
    {
        // Arrange: MBV reports 0 for the 9-month term; HSBC has no logo and carries a condition note.
        Setup(new VnExpressBankRatesApiData
        {
            BankRateOnline =
            [
                Row("MBV", 4.6m, 4.75m, 7m, 0m, 7m),
                Row("HSBC", 1m, 2.25m, 2.75m, 2.75m, 3.25m) with { Note = "Conditional product" }
            ]
        });

        // Act
        var result = await _sut.Handle(new GetVnExpressBankRatesQuery(), CancellationToken.None);

        // Assert
        result.Channel.Should().Be(BankRateChannel.Online);
        result.Data.Should().HaveCount(2);

        var mbv = result.Data[0];
        mbv.BankName.Should().Be("MBV");
        mbv.Month9.Should().BeNull("the source reports 0 for terms that are not offered");
        mbv.Month12.Should().Be(7m);
        mbv.LogoUrl.Should().BeNull("blank attributes are normalized to null");
        mbv.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1789653724));

        result.Data[1].Note.Should().Be("Conditional product");
    }

    [Fact]
    public async Task Handle_WhenRequestedChannelHasNoRows_ShouldReturnFailedResponse()
    {
        // Arrange: the source answers, but only the online channel carries data.
        Setup(new VnExpressBankRatesApiData
        {
            BankRateOnline = [Row("Vietcombank", 2.1m, 2.4m, 3.5m, 3.5m, 5.9m)]
        });

        // Act
        var result = await _sut.Handle(
            new GetVnExpressBankRatesQuery { Channel = BankRateChannel.Offline },
            CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Channel.Should().Be(BankRateChannel.Offline);
        result.Data.Should().BeEmpty();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    private void Setup(VnExpressBankRatesApiData data)
    {
        _externalDataServiceMock
            .Setup(x => x.GetVnExpressBankRatesAsync<VnExpressBankRatesApiResponse>(
                It.IsAny<BankRateChannel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VnExpressBankRatesApiResponse
            {
                Code = 200,
                Data = data
            });
    }

    private static VnExpressBankRateRow Row(string bank, decimal month1, decimal month3, decimal month6, decimal month9, decimal month12) =>
        new()
        {
            Bank = bank,
            Logo1 = string.Empty,
            Note = string.Empty,
            Rate1 = month1,
            Rate3 = month3,
            Rate6 = month6,
            Rate9 = month9,
            Rate12 = month12,
            UpdatedAt = 1789653724
        };
}
