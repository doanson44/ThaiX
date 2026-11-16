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
    [InlineData("regular_expression", "regex")]
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

    [Fact]
    public void UsesCustomFallback_ShouldReturnTrue_WhenOnlyGameSpecificGlyphIsAvailable()
    {
        // Arrange

        // Act
        var result = ThaixIcons.UsesCustomFallback("bomb");

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("home")]
    [InlineData("regular_expression")]
    [InlineData("settings")]
    public void UsesCustomFallback_ShouldReturnFalse_WhenCommonIconLibraryCanRender(
        string iconName)
    {
        // Arrange

        // Act
        var result = ThaixIcons.UsesCustomFallback(ThaixIcons.Resolve(iconName));

        // Assert
        result.Should().BeFalse();
    }
}
