namespace ThaiX.Application.Common.Interfaces;

public sealed record EmailOtpChallengeInfo(
    Guid ChallengeId,
    DateTime ExpiresAtUtc,
    DateTime ResendAfterUtc,
    int CodeLength);

public interface IEmailOtpService
{
    Task<EmailOtpChallengeInfo> RequestAsync(string email, CancellationToken cancellationToken = default);
    Task<Guid> VerifyAsync(Guid challengeId, string code, CancellationToken cancellationToken = default);
}
