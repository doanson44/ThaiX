using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "admin@thaix.com",
            Password = "P@ssw0rd123"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "P@ssw0rd")]
    [InlineData(null, "P@ssw0rd")]
    [InlineData("  ", "P@ssw0rd")]
    public void Validate_WithEmptyUsername_ShouldHaveError(string? username, string password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = username!,
            Password = password
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginCommand.Username));
    }

    [Theory]
    [InlineData("admin@thaix.com", "")]
    [InlineData("admin@thaix.com", null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string username, string? password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = username,
            Password = password!
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WithBothFieldsEmpty_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "",
            Password = ""
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
