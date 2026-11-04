namespace ThaiX.Infrastructure.Persistence.Entities;

public sealed class EmailOtpChallenge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string CodeHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public DateTime? InvalidatedAtUtc { get; set; }
}
