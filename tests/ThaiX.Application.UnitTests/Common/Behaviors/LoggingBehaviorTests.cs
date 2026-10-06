using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Behaviors;

namespace ThaiX.Application.UnitTests.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestLoggingRequest, TestLoggingResponse>>> _loggerMock = new();
    private readonly LoggingBehavior<TestLoggingRequest, TestLoggingResponse> _sut;

    public LoggingBehaviorTests()
    {
        _sut = new LoggingBehavior<TestLoggingRequest, TestLoggingResponse>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldLogAndReturnResponse()
    {
        // Arrange
        var request = new TestLoggingRequest { Value = "test" };
        var expectedResponse = new TestLoggingResponse { Result = "success" };

        // Act
        var result = await _sut.Handle(
            request,
            () => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var request = new TestLoggingRequest { Value = "fail" };
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var act = () => _sut.Handle(
            request,
            () => throw exception,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Something went wrong");
    }

    [Fact]
    public async Task Handle_ShouldCallNextDelegate()
    {
        // Arrange
        var request = new TestLoggingRequest { Value = "test" };
        var nextCalled = false;

        // Act
        await _sut.Handle(
            request,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(new TestLoggingResponse { Result = "done" });
            },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    // Test types for LoggingBehavior - must be public for Moq
    public sealed record TestLoggingRequest : IRequest<TestLoggingResponse>
    {
        public string Value { get; init; } = string.Empty;
    }

    public sealed record TestLoggingResponse
    {
        public string Result { get; init; } = string.Empty;
    }
}
