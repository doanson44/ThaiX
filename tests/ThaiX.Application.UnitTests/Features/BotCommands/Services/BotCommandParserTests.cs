using ThaiX.Application.Features.BotCommands.Services;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Services;

public sealed class BotCommandParserTests
{
    private readonly BotCommandParser _sut = new();

    [Fact]
    public void Parse_WithSlashCommandAndFlags_ShouldExtractCommandArgumentsAndFlags()
    {
        // Arrange
        const string raw = "/signal BTC Hour4 --async --crypto";

        // Act
        var result = _sut.Parse(raw);

        // Assert
        result.Success.Should().BeTrue();
        result.Command.Should().NotBeNull();
        result.Command!.Name.Should().Be("signal");
        result.Command.Arguments.Should().Equal("BTC", "Hour4");
        result.Command.Flags.Should().Contain("--async");
        result.Command.Flags.Should().Contain("--crypto");
    }

    [Fact]
    public void Parse_WithQuotedArgument_ShouldKeepQuotedValueAsSingleArgument()
    {
        // Arrange
        const string raw = "/alert BTC > 110000 stock --repeat \"Breakout watch\"";

        // Act
        var result = _sut.Parse(raw);

        // Assert
        result.Success.Should().BeTrue();
        result.Command.Should().NotBeNull();
        result.Command!.Name.Should().Be("alert");
        result.Command.Arguments.Should().ContainInOrder("BTC", ">", "110000", "stock", "Breakout watch");
        result.Command.Flags.Should().Contain("--repeat");
    }

    [Fact]
    public void Parse_WithEmptyText_ShouldReturnFailedResult()
    {
        // Arrange
        const string raw = "  ";

        // Act
        var result = _sut.Parse(raw);

        // Assert
        result.Success.Should().BeFalse();
        result.Command.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
