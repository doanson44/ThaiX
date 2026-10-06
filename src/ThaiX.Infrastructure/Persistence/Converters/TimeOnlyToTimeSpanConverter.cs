using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ThaiX.Infrastructure.Persistence.Converters;

/// <summary>
/// Converts TimeOnly to TimeSpan and vice versa for MariaDB/MySQL ADO.NET compatibility.
/// The MySQL ADO.NET provider returns System.TimeSpan when reading TIME columns,
/// which causes InvalidCastException when bound to TimeOnly without a converter.
/// </summary>
public sealed class TimeOnlyToTimeSpanConverter : ValueConverter<TimeOnly, TimeSpan>
{
    public TimeOnlyToTimeSpanConverter()
        : base(
            timeOnly => timeOnly.ToTimeSpan(),
            timeSpan => TimeOnly.FromTimeSpan(timeSpan))
    {
    }
}
