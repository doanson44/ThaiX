namespace ThaiX.Application.Common.Configuration;

public sealed class NotificationSchedulingSettings
{
    public const string SectionName = "NotificationScheduling";
    public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";
    public int DuePollIntervalSeconds { get; set; } = 30;
    public int MaxDuePerPoll { get; set; } = 50;
    public int CatchUpLimit { get; set; } = 5;
    public int ClaimLockSeconds { get; set; } = 120;
    public int MaxConsecutiveFailures { get; set; } = 3;
    public MisfirePolicyOptions Misfire { get; set; } = new();
}

public sealed class MisfirePolicyOptions
{
    public string DefaultPolicy { get; set; } = "Skip";
    public string OneTimePolicy { get; set; } = "Skip";
    public int CatchUpLimit { get; set; } = 5;
}
