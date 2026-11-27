using ThaiX.Client.Services.Ui;

namespace ThaiX.Client.UnitTests.Services.Ui;

public sealed class ThaixIconsTests
{
    [Theory]
    [InlineData("home", "house")]
    [InlineData("document", "file-earmark-text")]
    [InlineData("gamepad", "controller")]
    [InlineData("account_balance_wallet", "wallet2")]
    public void ResolveBootstrapIcon_ShouldReturnBootstrapGlyph_WhenAvailable(
        string iconName,
        string expectedGlyph)
    {
        // Arrange

        // Act
        var result = ThaixIcons.ResolveBootstrapIcon(iconName);

        // Assert
        result.Should().Be(expectedGlyph);
    }

    [Theory]
    [InlineData("ai-chip", "memory")]
    [InlineData("alert", "info")]
    [InlineData("gamepad", "sports_esports")]
    public void Resolve_ShouldReturnRadzenAlias_WhenLegacyNameIsUsed(
        string iconName,
        string expectedGlyph)
    {
        // Arrange

        // Act
        var result = ThaixIcons.Resolve(iconName);

        // Assert
        result.Should().Be(expectedGlyph);
    }

    [Theory]
    [InlineData("bomb")]
    [InlineData("regular_expression")]
    [InlineData("military_tech")]
    [InlineData("sports_tennis")]
    public void Resolve_ShouldPreserveMaterialSymbolNames_WhenRadzenSupportsIcon(
        string iconName)
    {
        // Arrange

        // Act
        var result = ThaixIcons.Resolve(iconName);

        // Assert
        result.Should().Be(iconName);
        ThaixIcons.ResolveBootstrapIcon(iconName).Should().BeNull();
    }
}