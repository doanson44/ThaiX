using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.Services;

public sealed class DateTimeProviderService : IDateTimeProvider
{
    private static class TimeZoneIds
    {
        public const string HoChiMinh = "Asia/Ho_Chi_Minh";
        public const string HoChiMinhWindows = "SE Asia Standard Time";
    }

    private const string TimeZoneConfigKey = "TimeZone:TimeZoneId";

    private readonly TimeZoneInfo _timeZone;

    public DateTimeProviderService(
        IConfiguration configuration,
        ILogger<DateTimeProviderService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _timeZone = ResolveTimeZone(configuration[TimeZoneConfigKey], logger);
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _timeZone);

    public TimeZoneInfo TimeZone => _timeZone;

    public string TimeZoneId => _timeZone.Id;

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            logger.LogWarning("Configuration '{ConfigKey}' is missing. Falling back to UTC.", TimeZoneConfigKey);
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(NormalizeTimeZoneId(timeZoneId));
        }
        catch (TimeZoneNotFoundException ex)
        {
            logger.LogWarning(ex, "Configured time zone '{TimeZoneId}' was not found. Falling back to UTC.", timeZoneId);
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException ex)
        {
            logger.LogWarning(ex, "Configured time zone '{TimeZoneId}' is invalid. Falling back to UTC.", timeZoneId);
            return TimeZoneInfo.Utc;
        }
    }

    private static string NormalizeTimeZoneId(string timeZoneId)
    {
        if (!OperatingSystem.IsWindows())
            return timeZoneId;

        return timeZoneId switch
        {
            TimeZoneIds.HoChiMinh => TimeZoneIds.HoChiMinhWindows,
            _ => timeZoneId
        };
    }
}