using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Persistence;
using ThaiX.Infrastructure.Persistence.Entities;

namespace ThaiX.Infrastructure.Authentication;

public sealed class EmailOtpService : IEmailOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailOtpService> _logger;
    private readonly EmailOtpOptions _options;

    public EmailOtpService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEmailSender emailSender,
        IOptions<EmailOtpOptions> options,
        ILogger<EmailOtpService> logger)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailOtpChallengeInfo> RequestAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var genericResponse = new EmailOtpChallengeInfo(
            Guid.NewGuid(),
            now.AddMinutes(_options.ExpirationMinutes),
            now.AddSeconds(_options.ResendCooldownSeconds),
            _options.CodeLength);

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || !user.EmailConfirmed || !user.EmailOtpLoginEnabled ||
            await _userManager.IsLockedOutAsync(user) || !await _signInManager.CanSignInAsync(user))
        {
            return genericResponse;
        }

        var cooldown = TimeSpan.FromSeconds(_options.ResendCooldownSeconds);
        EmailOtpChallenge challenge;
        ApplicationUser currentUser;
        string code;

        await using (var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken))
        {
            // Serialize requests for this account before reading or replacing its active challenge.
            await _context.Users
                .Where(x => x.Id == user.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.EmailOtpLoginEnabled, x => x.EmailOtpLoginEnabled),
                    cancellationToken);

            var userStillExists = await _context.Users
                .AnyAsync(x => x.Id == user.Id, cancellationToken);

            if (!userStillExists)
            {
                await transaction.CommitAsync(cancellationToken);
                return genericResponse;
            }

            await _context.Entry(user).ReloadAsync(cancellationToken);
            currentUser = user;

            if (!currentUser.EmailConfirmed ||
                !currentUser.EmailOtpLoginEnabled || await _userManager.IsLockedOutAsync(currentUser) ||
                !await _signInManager.CanSignInAsync(currentUser))
            {
                await transaction.CommitAsync(cancellationToken);
                return genericResponse;
            }
            var recentChallenge = await _context.EmailOtpChallenges
                .Where(x => x.UserId == currentUser.Id && x.CreatedAtUtc > now - cooldown)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (recentChallenge is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return new EmailOtpChallengeInfo(
                    recentChallenge.Id,
                    recentChallenge.ExpiresAtUtc,
                    recentChallenge.CreatedAtUtc.Add(cooldown),
                    _options.CodeLength);
            }

            await _context.EmailOtpChallenges
                .Where(x => x.UserId == currentUser.Id &&
                            x.ConsumedAtUtc == null &&
                            x.InvalidatedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.InvalidatedAtUtc, now),
                    cancellationToken);

            code = RandomNumberGenerator
                .GetInt32(0, (int)Math.Pow(10, _options.CodeLength))
                .ToString($"D{_options.CodeLength}");

            challenge = new EmailOtpChallenge
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id,
                CodeHash = _passwordHasher.HashPassword(currentUser, code),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(_options.ExpirationMinutes),
                FailedAttempts = 0
            };

            _context.EmailOtpChallenges.Add(challenge);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        try
        {
            var body = EmailTemplateBuilder.Build(
                "ThaiX - Login Verification",
                $"<p>Use this verification code to sign in to your ThaiX account:</p><h1>{code}</h1><p>This code expires in {_options.ExpirationMinutes} minutes. Do not share it with anyone.</p>",
                footNote: "ThaiX Security Team");

            await _emailSender.SendEmailAsync(
                currentUser.Email!,
                "ThaiX - Login Verification",
                body,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await InvalidateChallengeAsync(challenge.Id);
            throw;
        }
        catch (Exception ex)
        {
            await InvalidateChallengeAsync(challenge.Id);
            _logger.LogError("Failed to send email OTP. Failure type: {FailureType}", ex.GetType().Name);
        }

        return new EmailOtpChallengeInfo(
            challenge.Id,
            challenge.ExpiresAtUtc,
            challenge.CreatedAtUtc.Add(cooldown),
            _options.CodeLength);
    }

    public async Task<Guid> VerifyAsync(
        Guid challengeId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var challenge = await _context.EmailOtpChallenges
            .FirstOrDefaultAsync(x => x.Id == challengeId, cancellationToken);

        if (string.IsNullOrWhiteSpace(code) ||
            code.Length != _options.CodeLength ||
            code.Any(character => character is < '0' or > '9') ||
            challenge is null ||
            challenge.ConsumedAtUtc.HasValue ||
            challenge.InvalidatedAtUtc.HasValue ||
            challenge.ExpiresAtUtc <= now ||
            challenge.FailedAttempts >= _options.MaximumAttempts)
        {
            throw InvalidCode();
        }

        var user = await _userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user is null || !user.EmailConfirmed || !user.EmailOtpLoginEnabled)
        {
            throw InvalidCode();
        }

        var verifiedAt = DateTime.UtcNow;
        if (_passwordHasher.VerifyHashedPassword(user, challenge.CodeHash, code) == PasswordVerificationResult.Failed)
        {
            await _context.EmailOtpChallenges
                .Where(x => x.Id == challengeId &&
                            x.ConsumedAtUtc == null &&
                            x.InvalidatedAtUtc == null &&
                            x.ExpiresAtUtc > verifiedAt &&
                            x.FailedAttempts < _options.MaximumAttempts)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.FailedAttempts, x => x.FailedAttempts + 1),
                    cancellationToken);

            throw InvalidCode();
        }

        var consumed = await _context.EmailOtpChallenges
            .Where(x => x.Id == challengeId &&
                        x.ConsumedAtUtc == null &&
                        x.InvalidatedAtUtc == null &&
                        x.ExpiresAtUtc > verifiedAt &&
                        x.FailedAttempts < _options.MaximumAttempts)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.ConsumedAtUtc, verifiedAt),
                cancellationToken);

        if (consumed != 1)
        {
            throw InvalidCode();
        }

        return user.Id;
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var retentionCutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);

        var deletedCount = await _context.EmailOtpChallenges
            .Where(x => x.ExpiresAtUtc < retentionCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        deletedCount += await _context.EmailOtpChallenges
            .Where(x => x.ConsumedAtUtc != null && x.ConsumedAtUtc < retentionCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        deletedCount += await _context.EmailOtpChallenges
            .Where(x => x.InvalidatedAtUtc != null && x.InvalidatedAtUtc < retentionCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount;
    }

    private async Task InvalidateChallengeAsync(Guid challengeId)
    {
        await _context.EmailOtpChallenges
            .Where(x => x.Id == challengeId && x.ConsumedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.InvalidatedAtUtc, DateTime.UtcNow),
                CancellationToken.None);
    }

    private static OperationFailedException InvalidCode() =>
        new(ErrorCodes.INVALID_EMAIL_OTP, ErrorMessages.INVALID_EMAIL_OTP);
}
