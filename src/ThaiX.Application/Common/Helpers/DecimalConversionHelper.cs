using System.Globalization;

namespace ThaiX.Application.Common.Helpers;

/// <summary>
/// Converts binary floating-point results to decimal using the legacy .NET conversion precision.
/// </summary>
public static class DecimalConversionHelper
{
    public static decimal ToDecimalPreservingLegacyPrecision(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new OverflowException("Non-finite values cannot be converted to decimal.");
        }

        Span<char> buffer = stackalloc char[32];
        if (!value.TryFormat(buffer, out var charsWritten, "G15", CultureInfo.InvariantCulture))
        {
            throw new FormatException("The floating-point value could not be formatted.");
        }

        return decimal.Parse(buffer[..charsWritten], NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
