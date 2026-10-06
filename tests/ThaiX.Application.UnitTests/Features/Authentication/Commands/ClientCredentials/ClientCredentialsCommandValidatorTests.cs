using ThaiX.Application.Features.Authentication.Commands.ClientCredentials;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.ClientCredentials;

public sealed class ClientCredentialsCommandValidatorTests
{
    private readonly ClientCredentialsCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        var command = new ClientCredentialsCommand
        {
            ClientId = "my-client",
            ClientSecret = "my-secret"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidInputAndScope_ShouldNotHaveErrors()
    {
        var command = new ClientCredentialsCommand
        {
            ClientId = "my-client",
            ClientSecret = "my-secret",
            Scope = "read write"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyClientId_ShouldHaveError(string? clientId)
    {
        var command = new ClientCredentialsCommand
        {
            ClientId = clientId!,
            ClientSecret = "my-secret"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ClientCredentialsCommand.ClientId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyClientSecret_ShouldHaveError(string? clientSecret)
    {
        var command = new ClientCredentialsCommand
        {
            ClientId = "my-client",
            ClientSecret = clientSecret!
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ClientCredentialsCommand.ClientSecret));
    }

    [Fact]
    public void Validate_WithBothEmpty_ShouldHaveMultipleErrors()
    {
        var command = new ClientCredentialsCommand
        {
            ClientId = "",
            ClientSecret = ""
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
