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
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailOtpService> _logger;
    private readonly EmailOtpOptions _options;

    public EmailOtpService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEmailSender emailSender,
        IOptions<EmailOtpOptions> options,
        ILogger<EmailOtpService> logger)
    {
        _context = context;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Guid> RequestAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || !user.EmailConfirmed || !user.EmailOtpLoginEnabled ||
            await _userManager.IsLockedOutAsync(user))
        {
            return Guid.NewGuid();
        }

        var now = DateTime.UtcNow;
        var activeChallenge = await _context.EmailOtpChallenges
            .Where(x => x.UserId == user.Id &&
                        x.ConsumedAtUtc == null &&
                        x.InvalidatedAtUtc == null &&
                        x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeChallenge is not null && now - activeChallenge.CreatedAtUtc < TimeSpan.FromSeconds(_options.ResendCooldownSeconds))
        {
            return activeChallenge.Id;
        }

        await _context.EmailOtpChallenges
            .Where(x => x.UserId == user.Id &&
                        x.ConsumedAtUtc == null &&
                        x.InvalidatedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.InvalidatedAtUtc, now),
                cancellationToken);

        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, _options.CodeLength)).ToString($"D{_options.CodeLength}");
        var challenge = new EmailOtpChallenge
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = _passwordHasher.HashPassword(user, code),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(_options.ExpirationMinutes),
            FailedAttempts = 0
        };

        _context.EmailOtpChallenges.Add(challenge);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var body = EmailTemplateBuilder.Build(
                "ThaiX - Login Verification",
                $"<p>Use this verification code to sign in to your ThaiX account:</p><h1>{code}</h1><p>This code expires in {_options.ExpirationMinutes} minutes. Do not share it with anyone.</p>",
                footNote: "ThaiX Security Team");

            await _emailSender.SendEmailAsync(
                user.Email!,
                "ThaiX - Login Verification",
                body,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await _context.EmailOtpChallenges
                .Where(x => x.Id == challenge.Id && x.ConsumedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.InvalidatedAtUtc, DateTime.UtcNow),
                    cancellationToken);

            _logger.LogError(ex, "Failed to send email OTP for user {UserId}", user.Id);
            throw new OperationFailedException(
                ErrorCodes.INTERNAL_ERROR,
                "Unable to send the verification email.");
        }

        return challenge.Id;
    }

    public async Task<Guid> VerifyAsync(
        Guid challengeId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var challenge = await _context.EmailOtpChallenges
            .FirstOrDefaultAsync(x => x.Id == challengeId, cancellationToken);

        if (challenge is null ||
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

        if (_passwordHasher.VerifyHashedPassword(user, challenge.CodeHash, code) == PasswordVerificationResult.Failed)
        {
            await _context.EmailOtpChallenges
                .Where(x => x.Id == challengeId &&
                            x.ConsumedAtUtc == null &&
                            x.InvalidatedAtUtc == null &&
                            x.ExpiresAtUtc > now &&
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
                        x.ExpiresAtUtc > now &&
                        x.FailedAttempts < MaximumAttempts)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.ConsumedAtUtc, now),
                cancellationToken);

        if (consumed != 1)
        {
            throw InvalidCode();
        }

        return user.Id;
    }

    public async Task SetLoginEnabledAsync(
        Guid userId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                ErrorMessages.RESOURCE_NOT_FOUND);
        }

        if (enabled && !user.EmailConfirmed)
        {
            throw new OperationFailedException(
                ErrorCodes.INVALID_REQUEST,
                "Email must be confirmed before enabling email OTP login.");
        }

        user.EmailOtpLoginEnabled = enabled;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new OperationFailedException(
                ErrorCodes.INTERNAL_ERROR,
                "Unable to update email OTP login settings.");
        }

        if (!enabled)
        {
            await _context.EmailOtpChallenges
                .Where(x => x.UserId == userId &&
                            x.ConsumedAtUtc == null &&
                            x.InvalidatedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.InvalidatedAtUtc, DateTime.UtcNow),
                    cancellationToken);
        }
    }

    private static OperationFailedException InvalidCode() =>
        new(ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_REQUEST);
}
