namespace ThaiX.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    TimeZoneInfo TimeZone { get; }
    string TimeZoneId { get; }
}
