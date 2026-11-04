using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.RequestEmailOtp;

public sealed class RequestEmailOtpCommandHandlerTests
{
    private readonly Mock<IEmailOtpService> _emailOtpServiceMock = new();
    private readonly RequestEmailOtpCommandHandler _sut;

    public RequestEmailOtpCommandHandlerTests()
    {
        _sut = new RequestEmailOtpCommandHandler(_emailOtpServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEmail_ShouldReturnChallengeMetadata()
    {
        // Arrange
        var command = new RequestEmailOtpCommand("user@thaix.test");
        var expected = new EmailOtpChallengeInfo(
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(5),
            DateTime.UtcNow.AddSeconds(60),
            6);

        _emailOtpServiceMock
            .Setup(x => x.RequestAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _emailOtpServiceMock.Verify(
            x => x.RequestAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
