using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.BankInterestRates.Queries.GetBankInterestRates;

namespace ThaiX.Application.UnitTests.Features.ExternalData.BankInterestRates.Queries.GetBankInterestRates;

public sealed class GetBankInterestRatesQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetBankInterestRatesQueryHandler _sut;

    public GetBankInterestRatesQueryHandlerTests()
    {
        _sut = new GetBankInterestRatesQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsNull_ShouldReturnFailedResponse()
    {
        // Arrange
        _externalDataServiceMock
            .Setup(x => x.GetBankInterestRatesAsync<ExternalBankInterestRatesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalBankInterestRatesResponse?)null);

        // Act
        var result = await _sut.Handle(new GetBankInterestRatesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeEmpty();
        result.Message.Should().Be("Failed to retrieve bank interest rates from external API.");
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsValid_ShouldMapAndReturnSuccess()
    {
        // Arrange
        var external = new ExternalBankInterestRatesResponse
        {
            Success = true,
            Message = "ok",
            Data =
            [
                new BankInterestRateDto
                {
                    Id = "1",
                    Name = "ABC Bank",
                    Symbol = "ABC",
                    Icon = "https://img",
                    InterestRates =
                    [
                        new InterestRateDetailDto { Deposit = 1, Value = 3.1m },
                        new InterestRateDetailDto { Deposit = 12, Value = 5.9m }
                    ]
                }
            ]
        };

        _externalDataServiceMock
            .Setup(x => x.GetBankInterestRatesAsync<ExternalBankInterestRatesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(external);

        // Act
        var result = await _sut.Handle(new GetBankInterestRatesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("ok");
        result.Data.Should().HaveCount(1);
        result.Data[0].BankName.Should().Be("ABC Bank");
        result.Data[0].Month1.Should().Be(3.1m);
        result.Data[0].Month12.Should().Be(5.9m);
    }
}
