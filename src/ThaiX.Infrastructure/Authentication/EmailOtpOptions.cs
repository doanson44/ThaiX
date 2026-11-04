namespace ThaiX.Infrastructure.Authentication;

public sealed class EmailOtpOptions
{
    public const string SectionName = "EmailOtp";

    public int CodeLength { get; set; } = 6;
    public int ExpirationMinutes { get; set; } = 5;
    public int MaximumAttempts { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 60;
    public int RetentionDays { get; set; } = 1;
}
