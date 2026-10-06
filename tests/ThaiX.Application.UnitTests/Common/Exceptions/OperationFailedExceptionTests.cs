using ThaiX.Application.Common.Exceptions;

namespace ThaiX.Application.UnitTests.Common.Exceptions;

public sealed class OperationFailedExceptionTests
{
    [Fact]
    public void Constructor_WithValidParams_ShouldSetProperties()
    {
        // Act
        var exception = new OperationFailedException("ERR_001", "Something failed");

        // Assert
        exception.ErrorCode.Should().Be("ERR_001");
        exception.Message.Should().Be("Something failed");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_WithInvalidErrorCode_ShouldThrowArgumentException(string? errorCode)
    {
        // Act
        var act = () => new OperationFailedException(errorCode!, "Some message");

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("errorCode");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_WithInvalidErrorMessage_ShouldThrowArgumentException(string? errorMessage)
    {
        // Act
        var act = () => new OperationFailedException("ERR_001", errorMessage!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("errorMessage");
    }

    [Fact]
    public void OperationFailedException_ShouldInheritFromException()
    {
        var exception = new OperationFailedException("TEST", "Test error");

        exception.Should().BeAssignableTo<Exception>();
    }
}
