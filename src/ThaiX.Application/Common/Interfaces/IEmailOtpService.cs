namespace ThaiX.Application.Common.Interfaces;

public interface IEmailOtpService
{
    Task<Guid> RequestAsync(string email, CancellationToken cancellationToken = default);
    Task<Guid> VerifyAsync(Guid challengeId, string code, CancellationToken cancellationToken = default);
    Task SetLoginEnabledAsync(Guid userId, bool enabled, CancellationToken cancellationToken = default);
}
