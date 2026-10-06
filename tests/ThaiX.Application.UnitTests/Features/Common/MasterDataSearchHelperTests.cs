using ThaiX.Application.Common.Helpers;

namespace ThaiX.Application.UnitTests.Features.Common;

public sealed class MasterDataSearchHelperTests
{
    [Fact]
    public void EscapeLikePattern_ShouldEscapeWildcardCharacters()
    {
        var result = MasterDataSearchHelper.EscapeLikePattern("100%_test\\");

        result.Should().Be("100\\%\\_test\\\\");
    }

    [Fact]
    public void BuildContainsPattern_ShouldWrapEscapedValue()
    {
        var result = MasterDataSearchHelper.BuildContainsPattern("abc%_x");

        result.Should().Be("%abc\\%\\_x%");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildContainsPattern_WhenEmptyOrWhitespace_ShouldReturnNull(string? input)
    {
        var result = MasterDataSearchHelper.BuildContainsPattern(input);

        result.Should().BeNull();
    }
}
