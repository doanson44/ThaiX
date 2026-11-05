using ThaiX.Client.Services.Tools;
using ThaiX.Client.Services.Tools.Calculators;
using ThaiX.Client.Services.Tools.Transforms;
using Xunit;

namespace ThaiX.Client.UnitTests;

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
    public void FormatSql_WithKeywordsInsideLiteralsAndComments_ShouldPreserveThem()
    {
        // Arrange
        const string sql = "select 'from where', [order] from trades /* where group */ where note = 'order by'; -- from\nselect 1";

        // Act
        var formatted = TextToolTransforms.FormatSql(sql);

        // Assert
        Assert.Contains("'from where'", formatted, StringComparison.Ordinal);
        Assert.Contains("[order]", formatted, StringComparison.Ordinal);
        Assert.Contains("'order by'", formatted, StringComparison.Ordinal);
        Assert.Contains("/* where group */", formatted, StringComparison.Ordinal);
        Assert.Contains("-- from", formatted, StringComparison.Ordinal);
        Assert.Contains("SELECT", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void MarkdownToHtml_WithRawHtmlAndUnsafeLinks_ShouldNotEmitExecutableMarkup()
    {
        // Arrange
        const string markdown = "<img src=x onerror=alert(1)>\n\n[run](javascript:alert(1))";

        // Act
        var html = TextToolTransforms.MarkdownToHtml(markdown);

        // Assert
        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror=", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("href=\"javascript:", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HtmlToMarkdownRough_WithHeadingsLinksAndLists_ShouldPreserveStructure()
    {
        // Arrange
        const string html = "<h2>Tools</h2><p>Use <strong>ThaiX</strong> <a href=\"https://example.com\">docs</a>.</p><ul><li>One</li><li>Two</li></ul>";

        // Act
        var markdown = TextToolTransforms.HtmlToMarkdownRough(html);

        // Assert
        Assert.Contains("## Tools", markdown, StringComparison.Ordinal);
        Assert.Contains("**ThaiX**", markdown, StringComparison.Ordinal);
        Assert.Contains("[docs](https://example.com)", markdown, StringComparison.Ordinal);
        Assert.Contains("- One", markdown, StringComparison.Ordinal);
        Assert.Contains("- Two", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToTypescript_WithSpecialPropertyName_ShouldQuoteProperty()
    {
        // Arrange
        const string json = "{\"first-name\":\"ThaiX\",\"1st-value\":2}";

        // Act
        var output = ConvertToolTransforms.JsonToTypescript(json);

        // Assert
        Assert.Contains("\"first-name\": string;", output, StringComparison.Ordinal);
        Assert.Contains("\"1st-value\": number;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToCsharp_WithRepeatedNestedTypeNames_ShouldGenerateUniqueTypes()
    {
        // Arrange
        const string json = "{\"first\":{\"meta\":{\"id\":1}},\"second\":{\"meta\":{\"name\":\"ThaiX\"}}}";

        // Act
        var output = ConvertToolTransforms.JsonToCsharp(json);

        // Assert
        Assert.Contains("public class Meta", output, StringComparison.Ordinal);
        Assert.Contains("public class Meta2", output, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToTypescript_WithRepeatedNestedTypeNames_ShouldGenerateUniqueTypes()
    {
        // Arrange
        const string json = "{\"first\":{\"meta\":{\"id\":1}},\"second\":{\"meta\":{\"name\":\"ThaiX\"}}}";

        // Act
        var output = ConvertToolTransforms.JsonToTypescript(json);

        // Assert
        Assert.Contains("export interface Meta", output, StringComparison.Ordinal);
        Assert.Contains("export interface Meta2", output, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToCsharp_WithNonIdentifierPropertyName_ShouldGenerateValidProperty()
    {
        // Arrange
        const string json = "{\"first-name\":\"ThaiX\",\"1st-value\":2}";

        // Act
        var output = ConvertToolTransforms.JsonToCsharp(json);

        // Assert
        Assert.Contains("public string? FirstName", output, StringComparison.Ordinal);
        Assert.Contains("public long P1stValue", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public void NewUuid_WithSupportedFormat_ShouldParse(string format)
    {
        // Arrange / Act
        var value = EncodeToolTransforms.NewUuid(v7: true, format: format);

        // Assert
        Assert.True(Guid.TryParseExact(value, format, out _));
    }

    [Fact]
    public void NewUuid_WithUnsupportedFormat_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => EncodeToolTransforms.NewUuid(v7: false, "invalid");

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void Base64UrlEncodeAndDecode_ShouldRoundTripWithoutPadding()
    {
        // Arrange
        const string input = "{\"sub\":\"thaix-user\",\"role\":\"admin\"}";

        // Act
        var encoded = EncodeToolTransforms.Base64UrlEncode(input);
        var decoded = EncodeToolTransforms.Base64UrlDecodeText(encoded);

        // Assert
        Assert.DoesNotContain("=", encoded, StringComparison.Ordinal);
        Assert.DoesNotContain("+", encoded, StringComparison.Ordinal);
        Assert.DoesNotContain("/", encoded, StringComparison.Ordinal);
        Assert.Equal(input, decoded);
    }

    [Fact]
    public void UnixConvert_FromSeconds_ShouldNotInferMilliseconds()
    {
        // Arrange
        const string timestamp = "1735689600";

        // Act
        var result = EncodeToolTransforms.UnixConvert(timestamp, toUnix: false);

        // Assert
        Assert.Equal("2025-01-01T00:00:00.0000000Z (UTC)", result);
    }

    [Fact]
    public void UnixConvert_FromMilliseconds_ShouldUseExplicitMillisecondMode()
    {
        // Arrange
        const string timestamp = "1735689600000";

        // Act
        var result = EncodeToolTransforms.UnixConvert(timestamp, toUnix: false, milliseconds: true);

        // Assert
        Assert.Equal("2025-01-01T00:00:00.0000000Z (UTC)", result);
    }

    [Fact]
    public void UnixConvert_ToMilliseconds_ShouldReturnMillisecondTimestamp()
    {
        // Arrange
        const string dateTime = "2025-01-01T00:00:00Z";

        // Act
        var result = EncodeToolTransforms.UnixConvert(dateTime, toUnix: true, milliseconds: true);

        // Assert
        Assert.Equal("1735689600000", result);
    }

    [Fact]
    public void UrlEncode_WithAbsoluteUrl_ShouldPreserveUrlSeparators()
    {
        // Arrange
        const string url = "https://example.com/tools?q=btc usdt&tab=preview";

        // Act
        var encoded = EncodeToolTransforms.UrlEncode(url);

        // Assert
        Assert.StartsWith("https://example.com/tools?", encoded, StringComparison.Ordinal);
        Assert.Contains("q=btc%20usdt&tab=preview", encoded, StringComparison.Ordinal);
    }

    [Fact]
    public void ConventionalCommit_WithBreakingChange_ShouldRequireAndRenderDescription()
    {
        // Arrange / Act
        var output = GeneratorToolTransforms.ConventionalCommit(
            "feat", "tools", "remove legacy tool state", true, "Existing saved drafts require migration.");

        // Assert
        Assert.StartsWith("feat(tools)!: remove legacy tool state", output, StringComparison.Ordinal);
        Assert.Contains("BREAKING CHANGE: Existing saved drafts require migration.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ConventionalCommit_WithUnsupportedType_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.ConventionalCommit(
            "feature type", "tools", "add tool", false, string.Empty);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void ConventionalCommit_WithHeaderLongerThanSeventyTwoCharacters_ShouldThrowFormatException()
    {
        // Arrange
        var subject = new string('a', 70);

        // Act
        var act = () => GeneratorToolTransforms.ConventionalCommit(
            "feat", "tools", subject, false, string.Empty);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void ConventionalCommit_WithBreakingChangeAndNoDescription_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.ConventionalCommit(
            "fix", "tools", "remove old behavior", true, string.Empty);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void BuildCurl_WithSingleQuoteInUrl_ShouldEscapeShellArgument()
    {
        // Arrange
        const string url = "https://example.com/a'b";

        // Act
        var command = GeneratorToolTransforms.BuildCurl("GET", url, string.Empty, string.Empty);

        // Assert
        Assert.Contains("'https://example.com/a'\\''b'", command, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCurl_WithHeaderWithoutColon_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.BuildCurl(
            "GET", "https://example.com", "Authorization Bearer token", string.Empty);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void BuildCurl_WithInvalidHeaderName_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.BuildCurl(
            "GET", "https://example.com", "Bad Header: value", string.Empty);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void BuildCurl_WithInvalidMethod_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.BuildCurl("GET; rm -rf /", "https://example.com", "", "");

        // Assert
        Assert.Throws<FormatException>(act);
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
    public void CsvToJson_WithQuoteInsideUnquotedField_ShouldThrowFormatException()
    {
        // Arrange
        const string csv = "name,note\nThaiX,unfinished\"quote";

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
        Assert.True(password.Any(char.IsUpper));
        Assert.True(password.Any(char.IsLower));
        Assert.True(password.Any(char.IsDigit));
        Assert.Equal(symbols, password.Any(character => "!@#$%^&*-_=+?".Contains(character)));
    }

    [Fact]
    public void GetNextCronOccurrences_WithWeekdayQuarterHours_ShouldReturnExpectedUtcTimes()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 11, 6, 8, 59, 0, TimeSpan.Zero);

        // Act
        var next = GeneratorToolTransforms.GetNextCronOccurrences("*/15 9-10 * * 1-5", fromUtc);

        // Assert
        Assert.Equal(
            [
                new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 11, 6, 9, 15, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 11, 6, 9, 30, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 11, 6, 9, 45, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero)
            ],
            next);
    }

    [Fact]
    public void GetNextCronOccurrences_WithLeapDaySchedule_ShouldFindFutureOccurrences()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

        // Act
        var next = GeneratorToolTransforms.GetNextCronOccurrences("0 0 29 2 *", fromUtc, 3);

        // Assert
        Assert.Equal(
            new[]
            {
                new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2032, 2, 29, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2036, 2, 29, 0, 0, 0, TimeSpan.Zero)
            },
            next);
    }

    [Fact]
    public void GetNextCronOccurrences_WithImpossibleCalendarDate_ShouldThrowFormatException()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

        // Act
        var act = () => GeneratorToolTransforms.GetNextCronOccurrences("0 0 31 2 *", fromUtc);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Theory]
    [InlineData("* * * *")]
    [InlineData("60 * * * *")]
    [InlineData("*/0 * * * *")]
    [InlineData("1-60 * * * *")]
    public void GetNextCronOccurrences_WithInvalidExpression_ShouldThrowFormatException(string expression)
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 11, 6, 8, 59, 0, TimeSpan.Zero);

        // Act
        var act = () => GeneratorToolTransforms.GetNextCronOccurrences(expression, fromUtc);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void RegexTest_WithIgnoreCase_ShouldMatchDifferentCasing()
    {
        // Arrange
        const string input = "BTCUSDT btcusdt";

        // Act
        var result = TextToolTransforms.RegexTest("btcusdt", input, null, ignoreCase: true);

        // Assert
        Assert.Contains("Matches: 2", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RegexTest_WithoutIgnoreCase_ShouldRemainCaseSensitive()
    {
        // Arrange
        const string input = "BTCUSDT btcusdt";

        // Act
        var result = TextToolTransforms.RegexTest("btcusdt", input, null);

        // Assert
        Assert.Contains("Matches: 1", result, StringComparison.Ordinal);
    }

    [Fact]
    public void QrSvg_WithHighErrorCorrectionAndStandardQuietZone_ShouldReturnSvg()
    {
        // Arrange / Act
        var svg = GeneratorToolTransforms.QrSvg("https://example.com", 4, "high");

        // Assert
        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("viewBox", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void QrSvg_WithInvalidErrorCorrection_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.QrSvg("ThaiX", 4, "extreme");

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void QrSvg_WithQuietZoneOutsideSupportedRange_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.QrSvg("ThaiX", 17, "medium");

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void PasswordStrength_ShouldDiscloseHeuristicLimitations()
    {
        // Arrange / Act
        var result = GeneratorToolTransforms.PasswordStrength("Example-Password-2026");

        // Assert
        Assert.Contains("Heuristic only", result, StringComparison.Ordinal);
        Assert.Contains("not an entropy or breach check", result, StringComparison.Ordinal);
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
    public void JsonToCsharp_WithCollidingSanitizedNames_ShouldUseUniqueProperties()
    {
        // Arrange
        const string json = "{\"first-name\":1,\"first_name\":2}";

        // Act
        var output = ConvertToolTransforms.JsonToCsharp(json);

        // Assert
        Assert.Contains("FirstName { get; set; }", output, StringComparison.Ordinal);
        Assert.Contains("FirstName2 { get; set; }", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MarketCap_WithTotalAndMaximumSupply_ShouldReturnMarketCapAndFdv()
    {
        // Arrange / Act
        var result = CryptoCalculators.MarketCap(2m, 100m, 120m, 150m);

        // Assert
        Assert.Equal("200", result.Breakdown[0].Value);
        Assert.Equal("240", result.Breakdown[1].Value);
        Assert.Equal("300", result.Breakdown[2].Value);
        Assert.Equal("Calc.Result.TotalSupplyValue", result.Breakdown[1].LabelKey);
        Assert.Equal("Calc.Result.Fdv", result.Breakdown[2].LabelKey);
    }

    [Fact]
    public void MarketCap_WhenTotalSupplyIsBelowCirculatingSupply_ShouldThrow()
    {
        // Arrange / Act
        var act = () => CryptoCalculators.MarketCap(2m, 100m, 90m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void MarketCap_WhenMaximumSupplyIsBelowTotalSupply_ShouldThrow()
    {
        // Arrange / Act
        var act = () => CryptoCalculators.MarketCap(2m, 100m, 120m, 110m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void FuturesPnl_WhenFeeReachesOneHundredPercent_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.FuturesPnl(100m, 110m, 1m, 10m, TradeSide.Long, 100m, 0m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Liquidation_WhenLeverageIsBelowOne_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.Liquidation(100m, 0.5m, TradeSide.Long, 0.5m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void CalculatorRunner_WhenFundingPeriodsAreFractional_ShouldThrow()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            ["notional"] = "1000",
            ["ratePct"] = "0.01",
            ["periods"] = "1.5",
            ["side"] = "long"
        };

        // Act
        var act = () => CalculatorRunner.Run("funding", fields);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void RiskOfRuin_ShouldLabelOutputAsHeuristicNotProbability()
    {
        // Arrange / Act
        var result = RiskCalculators.RiskOfRuin(55m, 1.5m, 1m);

        // Assert
        Assert.Contains("not a calibrated probability", result.Formula, StringComparison.Ordinal);
        Assert.Equal("Calc.Interp.RiskOfRuin", result.InterpretationKey);
    }

    [Fact]
    public void PositionSize_ForShortTrade_ShouldAcceptStopAboveEntry()
    {
        // Arrange / Act
        var result = TradingCalculators.PositionSize(1000m, 1m, 100m, 110m, TradeSide.Short);

        // Assert
        Assert.Equal("1", result.Breakdown[1].Value);
    }

    [Fact]
    public void PositionSize_ForShortTradeWithStopBelowEntry_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.PositionSize(1000m, 1m, 100m, 90m, TradeSide.Short);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
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
