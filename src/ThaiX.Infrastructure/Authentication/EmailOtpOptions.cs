namespace ThaiX.Infrastructure.Authentication;

public sealed class EmailOtpOptions
{
    public const string SectionName = "EmailOtp";

    public int ExpirationMinutes { get; set; } = 5;
    public int MaximumAttempts { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 60;
}
