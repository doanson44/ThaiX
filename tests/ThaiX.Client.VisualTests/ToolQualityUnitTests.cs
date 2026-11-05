using ThaiX.Client.Services.Tools;
using ThaiX.Client.Services.Tools.Calculators;
using ThaiX.Client.Services.Tools.Transforms;
using Xunit;

namespace ThaiX.Client.VisualTests;

public sealed class ToolQualityUnitTests
{
    [Fact]
    public void CsvToJson_WithQuotedCommaAndEscapedQuote_ShouldPreserveValues()
    {
        // Arrange
        const string csv = "name,description\r\nThaiX,\"Tools, \"\"fast\"\"\"";

        // Act
        var json = ConvertToolTransforms.CsvToJson(csv);

        // Assert
        Assert.Contains("\"name\": \"ThaiX\"", json, StringComparison.Ordinal);
        Assert.Contains("Tools, ", json, StringComparison.Ordinal);
        Assert.Contains("\\\"fast\\\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void CsvToJson_WithQuotedNewlineAndBom_ShouldPreserveRecord()
    {
        // Arrange
        const string csv = "\uFEFFid,note\r\n1,\"first line\r\nsecond line\"";

        // Act
        var json = ConvertToolTransforms.CsvToJson(csv);

        // Assert
        Assert.Contains("first line\\r\\nsecond line", json, StringComparison.Ordinal);
    }

    [Fact]
    public void CsvToJson_WithUnterminatedQuote_ShouldThrowFormatException()
    {
        // Arrange
        const string csv = "name,note\nThaiX,\"unfinished";

        // Act
        var act = () => ConvertToolTransforms.CsvToJson(csv);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void CsvToJson_WithDuplicateHeaders_ShouldThrowFormatException()
    {
        // Arrange
        const string csv = "name,name\nThaiX,ThaiX";

        // Act
        var act = () => ConvertToolTransforms.CsvToJson(csv);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GeneratePassword_ShouldIncludeRequiredCharacterClasses(bool symbols)
    {
        // Arrange
        const int length = 32;

        // Act
        var password = GeneratorToolTransforms.GeneratePassword(length, symbols);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Equal(symbols, password.Any(character => "!@#$%^&*-_=+?".Contains(character)));
    }

    [Fact]
    public void ShouldSeedSample_ForNewCalculatorWorkspace_ShouldReturnTrue()
    {
        // Arrange
        var tool = ToolCatalog.Find("position-size")!;
        var state = new ToolWorkspaceState();

        // Act
        var shouldSeed = ToolSampleCatalog.ShouldSeedSample(tool, state);

        // Assert
        Assert.True(shouldSeed);
    }

    [Fact]
    public void ShouldSeedSample_ForInitializedCalculatorWorkspace_ShouldReturnFalse()
    {
        // Arrange
        var tool = ToolCatalog.Find("position-size")!;
        var state = new ToolWorkspaceState
        {
            Fields = CalculatorFieldCatalog.DefaultFields("position-size")
        };

        // Act
        var shouldSeed = ToolSampleCatalog.ShouldSeedSample(tool, state);

        // Assert
        Assert.False(shouldSeed);
    }

    [Fact]
    public void PositionSize_WhenRiskExceedsOneHundredPercent_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.PositionSize(1000m, 101m, 100m, 90m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Liquidation_WhenMaintenanceMarginIsOneHundredPercent_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.Liquidation(100m, 10m, TradeSide.Long, 100m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void TakeProfit_WhenBothTargetModesAreProvided_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.TakeProfit(100m, TradeSide.Long, 1m, 90m, 2m, 10m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WhenEquityIsNonPositive_ShouldThrow()
    {
        // Arrange / Act
        var act = () => RiskCalculators.MaxDrawdownFromSeries([100m, 0m, 90m]);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Funding_WithNegativeRate_ShouldReverseLongCashflowSign()
    {
        // Arrange / Act
        var result = TradingCalculators.Funding(1000m, -0.01m, 1, TradeSide.Long);

        // Assert
        Assert.True(decimal.Parse(result.Breakdown[0].Value, System.Globalization.CultureInfo.InvariantCulture) > 0m);
    }
}
