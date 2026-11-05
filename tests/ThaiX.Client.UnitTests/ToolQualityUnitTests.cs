using ThaiX.Client.Services.Tools;
using ThaiX.Client.Services.Tools.Calculators;
using ThaiX.Client.Services.Tools.Transforms;
using Microsoft.JSInterop;
using Xunit;

namespace ThaiX.Client.UnitTests;

public sealed class ToolQualityUnitTests
{
    [Fact]
    public async Task GetLastUsedAsync_WithDuplicateAndUnknownSlugs_ShouldReturnUniqueKnownTools()
    {
        // Arrange
        const string stored = "[\"json-formatter\",\"json-formatter\",\"JSON-FORMATTER\",\"unknown-tool\",\"base64\"]";
        var store = new ToolStateStore(new StubJsRuntime(stored), null!, null!, null!);

        // Act
        var tools = await store.GetLastUsedAsync();

        // Assert
        Assert.Equal(2, tools.Count);
        Assert.Equal("json-formatter", tools[0].Slug);
        Assert.Equal("base64", tools[1].Slug);
    }

    private sealed class StubJsRuntime(string? storedValue) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            if (!string.Equals(identifier, "localStorage.getItem", StringComparison.Ordinal))
                throw new InvalidOperationException($"Unexpected JS invocation: {identifier}");

            return ValueTask.FromResult((TValue)(object?)storedValue!);
        }
    }

    [Fact]
    public void YamlToJson_WithMalformedFlowSequence_ShouldReportSourcePosition()
    {
        // Arrange
        const string yaml = "items: [one, two";

        // Act
        var act = () => ConvertToolTransforms.YamlToJson(yaml);

        // Assert
        var exception = Assert.Throws<YamlDotNet.Core.YamlException>(act);
        Assert.Contains("Line:", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Col:", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatXml_WithSignificantTextWhitespace_ShouldPreserveIt()
    {
        // Arrange
        const string xml = "<root><item> value </item></root>";

        // Act
        var formatted = ConvertToolTransforms.FormatXml(xml);

        // Assert
        Assert.Contains("<item> value </item>", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void PrettyPrint_WithLargeJsonArray_ShouldFormatWithoutTruncation()
    {
        // Arrange
        var json = "{\"values\":[" + string.Join(',', Enumerable.Range(0, 50000)) + "]}";

        // Act
        var formatted = JsonToolTransforms.PrettyPrint(json);

        // Assert
        Assert.Contains("\"values\": [", formatted, StringComparison.Ordinal);
        Assert.Contains("49999", formatted, StringComparison.Ordinal);
        Assert.True(formatted.Length > json.Length);
    }

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
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
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
    public void JsonCompare_WithNullRoots_ShouldReturnEqual()
    {
        // Arrange / Act
        var result = JsonToolTransforms.Compare("null", "null");

        // Assert
        Assert.Equal("Documents are equal (canonical form).", result);
    }

    [Fact]
    public void JsonCompare_WithMissingVersusNullProperty_ShouldReportPath()
    {
        // Arrange / Act
        var result = JsonToolTransforms.Compare("{}", "{\"missing\":null}");

        // Assert
        Assert.Contains("$[\"missing\"]", result, StringComparison.Ordinal);
        Assert.Contains("<missing>", result, StringComparison.Ordinal);
        Assert.Contains("null", result, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonCompare_WithPropertyNameContainingDot_ShouldUseQuotedPath()
    {
        // Arrange / Act
        var result = JsonToolTransforms.Compare("{\"a.b\":1}", "{\"a.b\":2}");

        // Assert
        Assert.Contains("$[\"a.b\"]", result, StringComparison.Ordinal);
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

    [Fact]
    public void JsonToCsharp_WithHeterogeneousArray_ShouldUseSafeObjectCollection()
    {
        // Arrange
        const string json = "{\"items\":[{\"id\":1},{\"name\":\"ThaiX\"}]}";

        // Act
        var output = ConvertToolTransforms.JsonToCsharp(json);

        // Assert
        Assert.Contains("List<object?> Items", output, StringComparison.Ordinal);
        Assert.DoesNotContain("List<Item>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToTypescript_WithHeterogeneousPrimitiveArray_ShouldEmitUnionType()
    {
        // Arrange
        const string json = "{\"values\":[1,\"two\",true]}";

        // Act
        var output = ConvertToolTransforms.JsonToTypescript(json);

        // Assert
        Assert.Contains("(number | string | boolean)[]", output, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonToCsharp_WithDuplicateNestedPropertyNames_ShouldThrowFormatException()
    {
        // Arrange
        const string json = "{\"meta\":{\"id\":1,\"id\":2}}";

        // Act
        var act = () => ConvertToolTransforms.JsonToCsharp(json);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void JsonToTypescript_WithDuplicateRootPropertyNames_ShouldThrowFormatException()
    {
        // Arrange
        const string json = "{\"id\":1,\"id\":2}";

        // Act
        var act = () => ConvertToolTransforms.JsonToTypescript(json);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void RegexTest_WithMultilineOption_ShouldMatchLineAnchors()
    {
        // Arrange
        const string input = "header\\nBTCUSDT\\nfooter";

        // Act
        var withoutMultiline = TextToolTransforms.RegexTest("^BTC", input, null);
        var withMultiline = TextToolTransforms.RegexTest("^BTC", input, null, multiline: true);

        // Assert
        Assert.Contains("Matches: 0", withoutMultiline, StringComparison.Ordinal);
        Assert.Contains("Matches: 1", withMultiline, StringComparison.Ordinal);
    }

    [Fact]
    public void RegexTest_WithSinglelineOption_ShouldMatchAcrossNewlines()
    {
        // Arrange
        const string input = "start\\nend";

        // Act
        var withoutSingleline = TextToolTransforms.RegexTest("^start.*end$", input, null);
        var withSingleline = TextToolTransforms.RegexTest("^start.*end$", input, null, singleline: true);

        // Assert
        Assert.Contains("Matches: 0", withoutSingleline, StringComparison.Ordinal);
        Assert.Contains("Matches: 1", withSingleline, StringComparison.Ordinal);
    }

    [Fact]
    public void DiffSideBySide_WithWhitespaceOption_ShouldChangeComparison()
    {
        // Arrange
        const string left = "alpha beta";
        const string right = "alphabeta";

        // Act
        var strict = TextToolTransforms.DiffSideBySide(left, right, ignoreWhitespace: false);
        var relaxed = TextToolTransforms.DiffSideBySide(left, right, ignoreWhitespace: true);

        // Assert
        Assert.NotEqual(strict, relaxed);
    }

    [Fact]
    public void PrettyPrint_WithMalformedJson_ShouldExposeLineAndColumn()
    {
        // Arrange
        const string json = "{\n  \"value\": }";

        // Act
        var exception = Assert.Throws<System.Text.Json.JsonException>(() => JsonToolTransforms.PrettyPrint(json));

        // Assert
        Assert.NotNull(exception.LineNumber);
        Assert.NotNull(exception.BytePositionInLine);
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
    public void DecodeJwt_WithSignature_ShouldWarnThatClaimsAreUnverified()
    {
        // Arrange
        var header = EncodeToolTransforms.Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var payload = EncodeToolTransforms.Base64UrlEncode("{\"sub\":\"user-1\",\"role\":\"admin\"}");
        var token = $"{header}.{payload}.signature";

        // Act
        var output = EncodeToolTransforms.DecodeJwt(token);

        // Assert
        Assert.Contains("UNVERIFIED", output, StringComparison.Ordinal);
        Assert.Contains("Signature present (not verified).", output, StringComparison.Ordinal);
        Assert.Contains("\"role\": \"admin\"", output, StringComparison.Ordinal);
    }

    [Fact]
    public void DecodeJwt_WithoutSignature_ShouldReportMissingSignatureAndUnverifiedClaims()
    {
        // Arrange
        var header = EncodeToolTransforms.Base64UrlEncode("{\"alg\":\"none\"}");
        var payload = EncodeToolTransforms.Base64UrlEncode("{\"sub\":\"user-1\"}");
        var token = $"{header}.{payload}";

        // Act
        var output = EncodeToolTransforms.DecodeJwt(token);

        // Assert
        Assert.Contains("UNVERIFIED", output, StringComparison.Ordinal);
        Assert.Contains("Signature missing.", output, StringComparison.Ordinal);
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
    public void UrlComponentCodec_ShouldEncodeAndDecodeReservedCharacters()
    {
        // Arrange
        const string component = "BTC/USDT?source=chart&mode=fast";

        // Act
        var encoded = EncodeToolTransforms.UrlEncodeComponent(component);
        var decoded = EncodeToolTransforms.UrlDecodeComponent(encoded);

        // Assert
        Assert.Contains("%2F", encoded, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("%3F", encoded, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("%26", encoded, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(component, decoded);
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
    public void BuildCurl_WithQuotesAndShellMetacharacters_ShouldQuoteHeaderAndBody()
    {
        // Arrange
        const string headers = "X-Comment: O'Reilly";
        const string body = "{\"command\":\"$(id); echo 'unsafe'\"}";

        // Act
        var command = GeneratorToolTransforms.BuildCurl("POST", "https://example.com", headers, body);

        // Assert
        Assert.Contains("'X-Comment: O'\\''Reilly'", command, StringComparison.Ordinal);
        Assert.Contains("$(id); echo", command, StringComparison.Ordinal);
        Assert.Contains("'\\''unsafe'\\''", command, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCurl_WithControlCharacterInUrl_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.BuildCurl("GET", "https://example.com/\nmalicious", "", "");

        // Assert
        Assert.Throws<FormatException>(act);
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
    public void BuildCurl_WithCustomHttpTokenMethod_ShouldAcceptMethod()
    {
        // Arrange / Act
        var command = GeneratorToolTransforms.BuildCurl("M-SEARCH", "https://example.com", "", "");

        // Assert
        Assert.StartsWith("curl -X M-SEARCH ", command, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCurl_WithNullCharacterInBody_ShouldThrowFormatException()
    {
        // Arrange / Act
        var act = () => GeneratorToolTransforms.BuildCurl("POST", "https://example.com", "", "a\\0b");

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
    public void GetNextCronOccurrences_WithNamedMonthAndWeekdays_ShouldReturnExpectedDates()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

        // Act
        var next = GeneratorToolTransforms.GetNextCronOccurrences("0 9 * JAN MON-FRI", fromUtc, 2);

        // Assert
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 9, 0, 0, TimeSpan.Zero), next[0]);
        Assert.Equal(new DateTimeOffset(2027, 1, 4, 9, 0, 0, TimeSpan.Zero), next[1]);
    }

    [Fact]
    public void GetNextCronOccurrences_WithLeapDayAlias_ShouldFindNextLeapYear()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var next = GeneratorToolTransforms.GetNextCronOccurrences("0 0 29 FEB *", fromUtc, 1);

        // Assert
        Assert.Equal(new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero), next[0]);
    }

    [Fact]
    public void DescribeCron_WithLocalizedLabels_ShouldUseProvidedHeadings()
    {
        // Arrange
        var fromUtc = new DateTimeOffset(2026, 11, 6, 8, 59, 0, TimeSpan.Zero);

        // Act
        var output = GeneratorToolTransforms.DescribeCron(
            "0 9 * * 1-5",
            fromUtc,
            cronLabel: "Expression",
            nextRunsLabel: "Upcoming runs (UTC)");

        // Assert
        Assert.Contains("Expression: 0 9 * * 1-5", output, StringComparison.Ordinal);
        Assert.Contains("Upcoming runs (UTC):", output, StringComparison.Ordinal);
        Assert.Contains("2026-11-06 09:00 UTC", output, StringComparison.Ordinal);
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
    public void DiffSideBySide_WithChangedLine_ShouldAlignOldAndNewValues()
    {
        // Arrange / Act
        var result = TextToolTransforms.DiffSideBySide("one\ntwo", "one\nthree");

        // Assert
        Assert.Contains("OLD", result, StringComparison.Ordinal);
        Assert.Contains("NEW", result, StringComparison.Ordinal);
        Assert.Contains("- two", result, StringComparison.Ordinal);
        Assert.Contains("+ three", result, StringComparison.Ordinal);
    }

    [Fact]
    public void DiffSideBySide_WithEmptyInputs_ShouldReturnHeaders()
    {
        // Arrange / Act
        var result = TextToolTransforms.DiffSideBySide(string.Empty, string.Empty);

        // Assert
        Assert.Contains("OLD", result, StringComparison.Ordinal);
        Assert.Contains("NEW", result, StringComparison.Ordinal);
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
    public void RegexTest_WithReplacement_ShouldApplyReplacementText()
    {
        // Arrange
        const string input = "BTCUSDT ETHUSDT";

        // Act
        var result = TextToolTransforms.RegexTest(
            "[A-Z]{3,10}USDT",
            input,
            "$0-PERP");

        // Assert
        Assert.Contains("BTCUSDT-PERP ETHUSDT-PERP", result, StringComparison.Ordinal);
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
    public void FuturesPnl_ForLong_ShouldApplySlippageToBothSides()
    {
        // Arrange / Act
        var result = TradingCalculators.FuturesPnl(
            100m, 110m, 1m, 10m, TradeSide.Long, 0m, 0m, 1m);

        // Assert
        Assert.Equal("7.9", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.GrossPnl").Value);
        Assert.Equal("2.1", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.SlippageCost").Value);
    }

    [Fact]
    public void FuturesPnl_ForShort_ShouldApplyAdverseSlippageToBothSides()
    {
        // Arrange / Act
        var result = TradingCalculators.FuturesPnl(
            100m, 90m, 1m, 10m, TradeSide.Short, 0m, 0m, 1m);

        // Assert
        Assert.Equal("8.1", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.GrossPnl").Value);
        Assert.Equal("1.9", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.SlippageCost").Value);
    }

    [Fact]
    public void FuturesPnl_WhenSlippageIsOneHundredPercent_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.FuturesPnl(
            100m, 110m, 1m, 10m, TradeSide.Long, 0m, 0m, 100m);

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
    public void ConsecutiveLoss_ShouldReturnEquityAfterEachLoss()
    {
        // Arrange / Act
        var result = RiskCalculators.ConsecutiveLoss(1000m, 10m, 3);

        // Assert
        var equityPath = result.Breakdown
            .Where(line => line.LabelKey == "Calc.Result.RemainingEquity")
            .Select(line => line.Value)
            .ToArray();
        Assert.Equal(new[] { "729", "900", "810", "729" }, equityPath);
        Assert.Contains(result.Breakdown, line =>
            line.LabelKey == "Calc.Result.LossCount" && line.Value == "3");
    }

    [Fact]
    public void LossRecovery_WithMultipleScenarios_ShouldReturnEachRequiredRecovery()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            ["lossScenarios"] = "10,20,50"
        };

        // Act
        var result = CalculatorRunner.Run("loss-recovery", fields);

        // Assert
        Assert.Equal(6, result.Breakdown.Count);
        Assert.Equal("10", result.Breakdown[0].Value);
        Assert.Equal("11.11111111", result.Breakdown[1].Value);
        Assert.Equal("20", result.Breakdown[2].Value);
        Assert.Equal("25", result.Breakdown[3].Value);
        Assert.Equal("50", result.Breakdown[4].Value);
        Assert.Equal("100", result.Breakdown[5].Value);
    }

    [Fact]
    public void RiskOfRuin_ShouldReportMonteCarloAssumptionsAndConfidenceInterval()
    {
        // Arrange / Act
        var result = RiskCalculators.RiskOfRuin(55m, 1.5m, 1m, 50m, 100, 2000, 42);

        // Assert
        Assert.Contains("Monte Carlo", result.Formula, StringComparison.Ordinal);
        Assert.Contains("excludes fees, slippage", result.Formula, StringComparison.Ordinal);
        Assert.Equal("Calc.Interp.RiskOfRuin", result.InterpretationKey);
        Assert.Contains(result.Breakdown, line => line.LabelKey == "Calc.Result.RiskOfRuinConfidenceLow");
        Assert.Contains(result.Breakdown, line => line.LabelKey == "Calc.Result.RiskOfRuinConfidenceHigh");
    }

    [Fact]
    public void RiskOfRuin_WithSameSeedAndInputs_ShouldReturnSameEstimate()
    {
        // Arrange
        var first = RiskCalculators.RiskOfRuin(55m, 1.5m, 1m, 50m, 100, 2000, 1234);

        // Act
        var second = RiskCalculators.RiskOfRuin(55m, 1.5m, 1m, 50m, 100, 2000, 1234);

        // Assert
        Assert.Equal(first.Breakdown, second.Breakdown);
    }

    [Fact]
    public void RiskOfRuin_WhenSimulationWorkExceedsLimit_ShouldThrow()
    {
        // Arrange / Act
        var act = () => RiskCalculators.RiskOfRuin(55m, 1.5m, 1m, 50m, 1000, 10000, 1);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void MarketCapCompare_WithMultipleTargets_ShouldReturnEachImpliedPrice()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            ["supply"] = "1000000",
            ["targetMcaps"] = "500000000,1000000000,2000000000"
        };

        // Act
        var result = CalculatorRunner.Run("market-cap-compare", fields);

        // Assert
        Assert.Equal(6, result.Breakdown.Count);
        Assert.Equal("500", result.Breakdown[1].Value);
        Assert.Equal("1000", result.Breakdown[3].Value);
        Assert.Equal("2000", result.Breakdown[5].Value);
    }

    [Fact]
    public void MarketCapCompare_WithMoreThanTenTargets_ShouldThrow()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            ["supply"] = "1000000",
            ["targetMcaps"] = string.Join(',', Enumerable.Range(1, 11))
        };

        // Act
        var act = () => CalculatorRunner.Run("market-cap-compare", fields);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void FuturesPnl_WithContractSize_ShouldScaleBaseQuantityAndPnl()
    {
        // Arrange / Act
        var result = TradingCalculators.FuturesPnl(
            100m, 110m, 2m, 10m, TradeSide.Long, 0m, 0m, 0m, 3m);

        // Assert
        Assert.Equal("60", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.GrossPnl").Value);
        Assert.Equal("3", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.ContractSize").Value);
        Assert.Equal("6", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.BaseQuantity").Value);
    }

    [Fact]
    public void FuturesPnl_WithPositiveFundingRate_ShouldDeductLongFundingCashflow()
    {
        // Arrange / Act
        var result = TradingCalculators.FuturesPnl(
            100m, 110m, 1m, 10m, TradeSide.Long, 0m, 0m, 0m, 1m, 0.01m, 2);

        // Assert
        Assert.Equal("-0.021", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingCashflow").Value);
        Assert.Equal("9.979", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.NetPnl").Value);
    }

    [Fact]
    public void FuturesPnl_WithNegativeFundingRate_ShouldCreditLongFundingCashflow()
    {
        // Arrange / Act
        var result = TradingCalculators.FuturesPnl(
            100m, 110m, 1m, 10m, TradeSide.Long, 0m, 0m, 0m, 1m, -0.01m, 2);

        // Assert
        Assert.Equal("0.021", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingCashflow").Value);
        Assert.Equal("10.021", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.NetPnl").Value);
    }

    [Fact]
    public void Liquidation_ForLongLinearIsolatedPosition_ShouldUseMaintenanceMarginDenominator()
    {
        // Arrange / Act
        var result = TradingCalculators.Liquidation(100m, 10m, TradeSide.Long, 0.5m);

        // Assert
        Assert.Equal("90.45226131", result.Breakdown[0].Value);
    }

    [Fact]
    public void Liquidation_ForShortLinearIsolatedPosition_ShouldUseMaintenanceMarginDenominator()
    {
        // Arrange / Act
        var result = TradingCalculators.Liquidation(100m, 10m, TradeSide.Short, 0.5m);

        // Assert
        Assert.Equal("109.45273632", result.Breakdown[0].Value);
    }

    [Fact]
    public void RiskReward_WithStopAndTargetOnWrongSides_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.RiskReward(100m, 110m, 90m, TradeSide.Long);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void FuturesPnl_WhenLeverageIsBelowOne_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.FuturesPnl(
            100m, 110m, 1m, 0.5m, TradeSide.Long, 0m, 0m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Funding_WithUndefinedTradeSide_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.Funding(1000m, 0.01m, 1, (TradeSide)99);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void StopLoss_WithUndefinedTradeSide_ShouldThrow()
    {
        // Arrange / Act
        var act = () => TradingCalculators.StopLoss(10000m, 1m, 10m, 100m, (TradeSide)99);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void PositionSize_WithFeesAndSlippage_ShouldReducePositionQuantity()
    {
        // Arrange
        var gross = TradingCalculators.PositionSize(10000m, 1m, 100m, 90m, TradeSide.Long);
        var net = TradingCalculators.PositionSize(10000m, 1m, 100m, 90m, TradeSide.Long, 0.1m, 0.2m);

        // Act
        var grossQuantity = decimal.Parse(gross.Breakdown[5].Value, System.Globalization.CultureInfo.InvariantCulture);
        var netQuantity = decimal.Parse(net.Breakdown[5].Value, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.True(netQuantity < grossQuantity);
    }

    [Fact]
    public void RiskReward_WithFeesAndSlippage_ShouldReduceNetRewardRiskRatio()
    {
        // Arrange
        var gross = TradingCalculators.RiskReward(100m, 90m, 120m, TradeSide.Long);
        var net = TradingCalculators.RiskReward(100m, 90m, 120m, TradeSide.Long, 0.1m, 0.1m);

        // Act
        var grossRr = decimal.Parse(gross.Breakdown[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        var netRr = decimal.Parse(net.Breakdown[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        var grossBreakEven = decimal.Parse(gross.Breakdown[3].Value, System.Globalization.CultureInfo.InvariantCulture);
        var netBreakEven = decimal.Parse(net.Breakdown[3].Value, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.True(netRr < grossRr);
        Assert.True(netBreakEven > grossBreakEven);
    }

    [Fact]
    public void RiskReward_ShouldReportGrossBreakEvenWinRate()
    {
        // Arrange / Act
        var result = TradingCalculators.RiskReward(100m, 90m, 120m, TradeSide.Long);

        // Assert
        Assert.Equal("2", result.Breakdown[2].Value);
        Assert.Equal("33.33333333", result.Breakdown[3].Value);
        Assert.Equal("Calc.Result.BreakEvenWinRatePct", result.Breakdown[3].LabelKey);
    }

    [Fact]
    public void PositionSize_ForShortTrade_ShouldAcceptStopAboveEntry()
    {
        // Arrange / Act
        var result = TradingCalculators.PositionSize(1000m, 1m, 100m, 110m, TradeSide.Short);

        // Assert
        Assert.Equal("1", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.Quantity").Value);
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
    public void AverageEntry_WithPerLegFees_ShouldIncludeFeesInCostBasis()
    {
        // Arrange / Act
        var result = TradingCalculators.AverageEntry([(100m, 1m), (200m, 1m)], 1m);

        // Assert
        Assert.Equal("151.5", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.AverageEntry").Value);
        Assert.Equal("303", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.Cost").Value);
        Assert.Equal("3", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.Fees").Value);
    }

    [Fact]
    public void Dca_WithFeesAndCurrentPrice_ShouldReportOutlayAndUnrealizedPnl()
    {
        // Arrange / Act
        var result = TradingCalculators.Dca([(100m, 100m), (200m, 100m)], 1m, 150m);

        // Assert
        Assert.Equal("202", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.TotalOutlay").Value);
        Assert.Equal("225", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.MarketValue").Value);
        Assert.Equal("23", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.UnrealizedPnl").Value);
    }

    [Fact]
    public void StopLoss_WithFeesAndSlippage_ShouldMoveLongStopFurtherAway()
    {
        // Arrange
        var gross = TradingCalculators.StopLoss(10000m, 1m, 10m, 100m, TradeSide.Long);
        var net = TradingCalculators.StopLoss(10000m, 1m, 10m, 100m, TradeSide.Long, 0.1m, 0.1m);

        // Act
        var grossStop = decimal.Parse(gross.Breakdown.Single(line => line.LabelKey == "Calc.Result.StopLossPrice").Value, System.Globalization.CultureInfo.InvariantCulture);
        var netStop = decimal.Parse(net.Breakdown.Single(line => line.LabelKey == "Calc.Result.StopLossPrice").Value, System.Globalization.CultureInfo.InvariantCulture);
        var totalRiskPerUnit = decimal.Parse(net.Breakdown.Single(line => line.LabelKey == "Calc.Result.TotalRiskPerUnit").Value, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.True(netStop < grossStop);
        Assert.Equal(10m, totalRiskPerUnit);
    }

    [Fact]
    public void TakeProfit_WithFeesAndSlippage_ShouldReportGrossAndNetProfit()
    {
        // Arrange / Act
        var result = TradingCalculators.TakeProfit(100m, TradeSide.Long, 1m, 90m, 2m, null, 0.1m, 0.1m);

        // Assert
        var grossProfit = decimal.Parse(result.Breakdown.Single(line => line.LabelKey == "Calc.Result.GrossProfit").Value, System.Globalization.CultureInfo.InvariantCulture);
        var netProfit = decimal.Parse(result.Breakdown.Single(line => line.LabelKey == "Calc.Result.ExpectedProfit").Value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(netProfit < grossProfit);
        Assert.Contains(result.Breakdown, line => line.LabelKey == "Calc.Result.NetRisk");
        Assert.Contains(result.Breakdown, line => line.LabelKey == "Calc.Result.BreakEvenWinRatePct");
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
    public void Kelly_WithConfiguredCap_ShouldLimitFullKellyAllocation()
    {
        // Arrange / Act
        var result = RiskCalculators.Kelly(60m, 2m, 15m);

        // Assert
        Assert.Equal("40", result.Breakdown[0].Value);
        Assert.Equal("20", result.Breakdown[1].Value);
        Assert.Equal("10", result.Breakdown[2].Value);
        Assert.Equal("15", result.Breakdown[3].Value);
    }

    [Fact]
    public void Kelly_WhenCapExceedsOneHundredPercent_ShouldThrow()
    {
        // Arrange / Act
        var act = () => RiskCalculators.Kelly(60m, 2m, 101m);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Expectancy_ShouldReportAbsoluteAndRMultipleValues()
    {
        // Arrange / Act
        var result = RiskCalculators.Expectancy(50m, 2m, 1m);

        // Assert
        Assert.Equal("0.5", result.Breakdown[0].Value);
        Assert.Equal("0.5", result.Breakdown[1].Value);
        Assert.Equal("Calc.Result.ExpectancyR", result.Breakdown[1].LabelKey);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WithRecoveredDrawdown_ShouldReportDurations()
    {
        // Arrange / Act
        var result = RiskCalculators.MaxDrawdownFromSeries([100m, 120m, 90m, 100m, 120m, 110m]);

        // Assert
        Assert.Equal("30", result.Breakdown[0].Value);
        Assert.Equal("25", result.Breakdown[1].Value);
        Assert.Equal("1", result.Breakdown[2].Value);
        Assert.Equal("2", result.Breakdown[3].Value);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WhenNotRecovered_ShouldReportPendingRecovery()
    {
        // Arrange / Act
        var result = RiskCalculators.MaxDrawdownFromSeries([100m, 120m, 90m, 100m]);

        // Assert
        Assert.Equal("N/A", result.Breakdown[3].Value);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WithOnlyZeroEquity_ShouldReturnZeroDrawdown()
    {
        // Arrange / Act
        var result = RiskCalculators.MaxDrawdownFromSeries([0m, 0m]);

        // Assert
        Assert.Equal("0", result.Breakdown[0].Value);
        Assert.Equal("0", result.Breakdown[1].Value);
        Assert.Equal("0", result.Breakdown[3].Value);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WithZeroEquity_ShouldReportFullDrawdown()
    {
        // Arrange / Act
        var result = RiskCalculators.MaxDrawdownFromSeries([100m, 0m, 90m]);

        // Assert
        Assert.Equal("100", result.Breakdown[1].Value);
        Assert.Equal("N/A", result.Breakdown[3].Value);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WhenEquityIsNegative_ShouldThrow()
    {
        // Arrange / Act
        var act = () => RiskCalculators.MaxDrawdownFromSeries([100m, -1m, 90m]);

        // Assert
        Assert.Throws<CalculatorInputException>(act);
    }

    [Fact]
    public void Funding_WithFractionalPeriodCount_ShouldThrow()
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
    public void MaxDrawdownFromSeries_WithDifferentAbsoluteAndPercentagePeaks_ShouldReportSeparateDurations()
    {
        // Arrange
        decimal[] equity = [100m, 80m, 200m, 170m];

        // Act
        var result = RiskCalculators.MaxDrawdownFromSeries(equity);

        // Assert
        Assert.Equal("30", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.MaxDrawdown").Value);
        Assert.Equal("20", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.MaxDrawdownPct").Value);
        Assert.Equal("1", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.AbsolutePeakToTroughPeriods").Value);
        Assert.Equal("N/A", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.AbsoluteTroughToRecoveryPeriods").Value);
        Assert.Equal("1", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.TroughToRecoveryPeriods").Value);
    }

    [Fact]
    public void MaxDrawdownFromSeries_WithRecoveredTrough_ShouldReportRecoveryPeriods()
    {
        // Arrange
        decimal[] equity = [100m, 80m, 90m, 100m];

        // Act
        var result = RiskCalculators.MaxDrawdownFromSeries(equity);

        // Assert
        Assert.Contains(result.Breakdown, line =>
            line.LabelKey == "Calc.Result.TroughToRecoveryPeriods" && line.Value == "2");
    }

    [Fact]
    public void Funding_WithNegativeRate_ShouldIdentifyShortAsPayer()
    {
        // Arrange / Act
        var result = TradingCalculators.Funding(1000m, -0.01m, 1, TradeSide.Long);

        // Assert
        Assert.Equal("Calc.Side.Short", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingPayer").Value);
        Assert.Equal("Calc.Side.Long", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingReceiver").Value);
    }

    [Fact]
    public void Funding_WithPositiveRate_ShouldIdentifyLongAsPayer()
    {
        // Arrange / Act
        var result = TradingCalculators.Funding(1000m, 0.01m, 1, TradeSide.Short);

        // Assert
        Assert.Equal("Calc.Side.Long", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingPayer").Value);
        Assert.Equal("Calc.Side.Short", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.FundingReceiver").Value);
    }

    [Fact]
    public void Leverage_ShouldReportApproximateAdverseMove()
    {
        // Arrange / Act
        var result = TradingCalculators.Leverage(1000m, 100m);

        // Assert
        Assert.Equal("10", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.EffectiveLeverage").Value);
        Assert.Equal("10", result.Breakdown.Single(line => line.LabelKey == "Calc.Result.AdverseMovePct").Value);
        Assert.Contains("not an exchange liquidation price", result.Formula, StringComparison.Ordinal);
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
