using Radzen;

namespace ThaiX.Client.Extensions;

/// <summary>
/// Extension methods for <see cref="NotificationService"/> to provide
/// consistent notification behavior across the application.
/// All notifications appear at the bottom-right corner.
/// </summary>
public static class NotificationExtensions
{
    private const int SuccessDuration = 4000;
    private const int InfoDuration = 5000;
    private const int WarningDuration = 5000;
    private const int ErrorDuration = 6000;
    private const string BottomRightStyle = "position: fixed; bottom: 0; right: 0;";

    public static void NotifySuccess(
        this NotificationService service,
        string summary,
        string? detail = null,
        int? duration = null)
    {
        service.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = summary,
            Detail = detail,
            Duration = duration ?? SuccessDuration,
            Style = BottomRightStyle
        });
    }

    public static void NotifyError(
        this NotificationService service,
        string summary,
        string? detail = null,
        int? duration = null)
    {
        service.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = summary,
            Detail = detail,
            Duration = duration ?? ErrorDuration,
            Style = BottomRightStyle
        });
    }

    public static void NotifyInfo(
        this NotificationService service,
        string summary,
        string? detail = null,
        int? duration = null)
    {
        service.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = summary,
            Detail = detail,
            Duration = duration ?? InfoDuration,
            Style = BottomRightStyle
        });
    }

    public static void NotifyWarning(
        this NotificationService service,
        string summary,
        string? detail = null,
        int? duration = null)
    {
        service.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = summary,
            Detail = detail,
            Duration = duration ?? WarningDuration,
            Style = BottomRightStyle
        });
    }
}
