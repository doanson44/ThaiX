using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ThaiX.Infrastructure.Persistence.Converters;

/// <summary>
/// Converts DateOnly to DateTime and vice versa for MariaDB/MySQL ADO.NET compatibility.
/// The MySQL ADO.NET provider returns System.DateTime when reading DATE columns,
/// which causes InvalidCastException when bound to DateOnly without a converter.
/// </summary>
public sealed class DateOnlyToDateTimeConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyToDateTimeConverter()
        : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}
