namespace ThaiX.Domain.Aggregates.Notifications.Enums;

public enum MisfirePolicy
{
    Skip = 1,
    RunOnceNow = 2,
    CatchUpAll = 3,
    CatchUpLimited = 4
}
