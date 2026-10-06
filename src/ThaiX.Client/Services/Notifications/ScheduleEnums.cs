namespace ThaiX.Client.Services.Notifications;

public enum ScheduleType
{
    OneTime = 1,
    EveryXDays = 2,
    Weekly = 3,
    Monthly = 4
}

public enum MonthlyOverflowPolicy
{
    SkipMonth = 1,
    RunOnLastDay = 2
}

public enum MisfirePolicy
{
    Skip = 1,
    RunOnceNow = 2,
    CatchUpAll = 3,
    CatchUpLimited = 4
}

public enum ScheduleStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Completed = 4,
    Disabled = 5,
    Deleted = 6
}
