using Hangfire;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class EmailOtpChallengeCleanupJob : HangfireJobBase
{
    private readonly IEmailOtpService _emailOtpService;
    private readonly ILogger<EmailOtpChallengeCleanupJob> _logger;

    public EmailOtpChallengeCleanupJob(
        IEmailOtpService emailOtpService,
        IHangfireJobState hangfireJobState,
        ILogger<EmailOtpChallengeCleanupJob> logger) : base(hangfireJobState, logger)
    {
        _emailOtpService = emailOtpService;
        _logger = logger;
    }

    [Queue("low")]
    public async Task CleanupExpiredChallenges(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.EmailOtpChallengeCleanupJob))
        {
            return;
        }

        var deletedCount = await _emailOtpService.CleanupExpiredAsync(cancellationToken);
        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Email OTP cleanup removed {DeletedCount} expired challenge records.",
                deletedCount);
        }
    }
}
