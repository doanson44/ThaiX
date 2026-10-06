using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatest;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatest;

public sealed class GetVnDirectRatiosLatestQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetVnDirectRatiosLatestQueryHandler _sut;

    public GetVnDirectRatiosLatestQueryHandlerTests()
    {
        _sut = new GetVnDirectRatiosLatestQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ShouldReturnFailedResponseWithoutCallingService()
    {
        // Arrange
        var query = new GetVnDirectRatiosLatestQuery { Code = "!!bad!!" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid code");
        _externalDataServiceMock.Verify(
            x => x.GetVnDirectRatiosLatestAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCodeAndNullExternalData_ShouldReturnFailedResponse()
    {
        // Arrange
        var query = new GetVnDirectRatiosLatestQuery { Code = "VNM" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("VNM");
        result.Data.Should().BeEmpty();
    }
}
