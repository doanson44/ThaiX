using System.Globalization;
using ThaiX.Application.Common.Helpers;

namespace ThaiX.Application.UnitTests.Common.Helpers;

public sealed class DecimalConversionHelperTests
{
    [Theory]
    [InlineData(1.23d, "1.23")]
    [InlineData(14.1d, "14.1")]
    [InlineData(-123.4567d, "-123.4567")]
    public void ToDecimalPreservingLegacyPrecision_WhenFiniteValueIsProvided_PreservesFifteenSignificantDigits(
        double value,
        string expected)
    {
        var result = DecimalConversionHelper.ToDecimalPreservingLegacyPrecision(value);

        result.Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ToDecimalPreservingLegacyPrecision_WhenValueIsNotFinite_ThrowsOverflowException(double value)
    {
        var act = () => DecimalConversionHelper.ToDecimalPreservingLegacyPrecision(value);

        act.Should().Throw<OverflowException>();
    }
}
