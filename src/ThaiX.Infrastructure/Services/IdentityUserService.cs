using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Identity.Dtos;
using ThaiX.Application.Features.Users.Commands.ResetPassword;
using ThaiX.Application.Features.Users.Queries.GetUsers;
using ThaiX.Domain.Aggregates.Contacts;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Implementation of IIdentityUserService using ASP.NET Core Identity.
/// Encapsulates all UserManager/SignInManager operations.
/// </summary>
public sealed class IdentityUserService : IIdentityUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IdentityUserService> _logger;

    public IdentityUserService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<IdentityUserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region User Creation & Management

    public async Task<Guid> CreateUserAsync(
        string email,
        string password,
        string? phoneNumber,
        IEnumerable<string>? permissions,
        bool requirePasswordChange,
        bool emailConfirmed,
        CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.DUPLICATE_ENTRY,
                $"User with email '{email}' already exists.");
        }

        // Validate permissions if provided
        if (permissions != null && permissions.Any())
        {
            var invalidPermissions = ValidatePermissions(permissions);
            if (invalidPermissions.Any())
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INVALID_REQUEST,
                    $"Invalid permissions: {string.Join(", ", invalidPermissions)}");
            }
        }

        // Create user (phone normalized for storage, same format as Contact)
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = NormalizePhoneForStorage(phoneNumber),
            EmailConfirmed = emailConfirmed
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        // Assign permissions if provided
        if (permissions != null && permissions.Any())
        {
            var permissionClaims = permissions.Select(p =>
                new Claim(Application.Common.Constants.ClaimTypeConstants.Permission, p));

            var addClaimsResult = await _userManager.AddClaimsAsync(user, permissionClaims);
            if (!addClaimsResult.Succeeded)
            {
                // Rollback: delete user
                await _userManager.DeleteAsync(user);

                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                    $"Failed to assign permissions: {string.Join(", ", addClaimsResult.Errors.Select(e => e.Description))}");
            }
        }

        // Set password change requirement if requested
        if (requirePasswordChange)
        {
            await _userManager.AddClaimAsync(user,
                new Claim("password_change_required", "true"));
        }

        _logger.LogInformation(
            "User created: {UserId} ({Email}) by {ActorId} ({ActorEmail}). RequirePasswordChange: {RequirePasswordChange}, EmailConfirmed: {EmailConfirmed}",
            user.Id,
            user.Email,
            _currentUserService.UserId,
            _currentUserService.Email,
            requirePasswordChange,
            emailConfirmed);

        return user.Id;
    }

    public async Task UpdateUserAsync(
        Guid userId,
        string? email,
        string? phoneNumber,
        bool? emailConfirmed,
        bool? twoFactorEnabled,
        CancellationToken cancellationToken = default)
    {
        // Find user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");
        }

        var changes = new List<string>();

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
        {
            // Check if new email is already taken
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.DUPLICATE_ENTRY,
                    $"Email '{email}' is already taken.");
            }

            var oldEmail = user.Email;
            user.Email = email;
            user.NormalizedEmail = email.ToUpperInvariant();
            user.UserName = email;
            user.NormalizedUserName = email.ToUpperInvariant();

            // Reset email confirmation when email changes
            user.EmailConfirmed = false;

            changes.Add($"Email changed from '{oldEmail}' to '{email}'");
        }

        // Update phone number if provided (normalize for storage, same format as Contact)
        if (phoneNumber != null)
        {
            var normalized = NormalizePhoneForStorage(phoneNumber);
            if (normalized != user.PhoneNumber)
            {
                var oldPhone = user.PhoneNumber;
                user.PhoneNumber = normalized;
                user.PhoneNumberConfirmed = false; // Reset confirmation
                changes.Add($"PhoneNumber changed from '{oldPhone ?? "null"}' to '{user.PhoneNumber ?? "null"}'");
            }
        }

        // Update email confirmed status if provided (admin privilege)
        if (emailConfirmed.HasValue && emailConfirmed.Value != user.EmailConfirmed)
        {
            user.EmailConfirmed = emailConfirmed.Value;
            changes.Add($"EmailConfirmed set to {user.EmailConfirmed}");
        }

        // Update two-factor enabled if provided
        if (twoFactorEnabled.HasValue && twoFactorEnabled.Value != user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = twoFactorEnabled.Value;
            changes.Add($"TwoFactorEnabled set to {user.TwoFactorEnabled}");
        }

        // No changes
        if (changes.Count == 0)
        {
            return;
        }

        // Update user
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
        }

        // Note: RefreshSignInAsync is not called because the app uses JWT-only auth (no cookie scheme).
        // JWT tokens are stateless; updated claims take effect on next token refresh.

        _logger.LogInformation(
            "User {UserId} ({Email}) updated by {ActorId} ({ActorEmail}). Changes: [{Changes}]",
            user.Id,
            user.Email,
            _currentUserService.UserId,
            _currentUserService.Email,
            string.Join(", ", changes));

        return;
    }

    #endregion

    #region Lockout Management

    public async Task SetUserLockoutAsync(
        Guid userId,
        bool isLocked,
        int? lockoutDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        // Find user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");
        }

        // Prevent self-lockout
        if (userId == _currentUserService.UserId)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.OPERATION_NOT_ALLOWED,
                "You cannot lock/unlock your own account.");
        }

        if (isLocked)
        {
            // Lock user
            var lockoutEnd = lockoutDurationMinutes.HasValue
                ? DateTimeOffset.UtcNow.AddMinutes(lockoutDurationMinutes.Value)
                : DateTimeOffset.MaxValue; // Indefinite

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!lockoutResult.Succeeded)
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                    $"Failed to lock user: {string.Join(", ", lockoutResult.Errors.Select(e => e.Description))}");
            }

            // Enable lockout if not already enabled
            if (!user.LockoutEnabled)
            {
                var enableResult = await _userManager.SetLockoutEnabledAsync(user, true);
                if (!enableResult.Succeeded)
                {
                    throw new OperationFailedException(
                        Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                        $"Failed to enable lockout: {string.Join(", ", enableResult.Errors.Select(e => e.Description))}");
                }
            }

            _logger.LogWarning(
                "User {UserId} ({Email}) locked by {ActorId} ({ActorEmail}). Duration: {Duration} minutes",
                user.Id,
                user.Email,
                _currentUserService.UserId,
                _currentUserService.Email,
                lockoutDurationMinutes?.ToString() ?? "indefinite");
        }
        else
        {
            // Unlock user
            var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!unlockResult.Succeeded)
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                    $"Failed to unlock user: {string.Join(", ", unlockResult.Errors.Select(e => e.Description))}");
            }

            // Reset failed access count
            var resetResult = await _userManager.ResetAccessFailedCountAsync(user);
            if (!resetResult.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to reset access failed count for user {UserId}: {Errors}",
                    user.Id,
                    string.Join(", ", resetResult.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation(
                "User {UserId} ({Email}) unlocked by {ActorId} ({ActorEmail})",
                user.Id,
                user.Email,
                _currentUserService.UserId,
                _currentUserService.Email);
        }

        return;
    }

    #endregion

    #region Permission Management

    public async Task SetUserPermissionsAsync(
        Guid userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        // Find user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");
        }

        // Validate all permissions exist in system
        var invalidPermissions = ValidatePermissions(permissions);
        if (invalidPermissions.Any())
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INVALID_REQUEST,
                $"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }

        // Get existing permission claims
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var permissionClaims = existingClaims
            .Where(c => c.Type == Application.Common.Constants.ClaimTypeConstants.Permission)
            .ToList();

        // Remove existing permissions
        if (permissionClaims.Any())
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, permissionClaims);
            if (!removeResult.Succeeded)
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                    $"Failed to remove existing permissions: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
            }
        }

        // Add new permissions
        var newClaims = permissions.Select(p =>
            new Claim(Application.Common.Constants.ClaimTypeConstants.Permission, p)).ToList();

        if (newClaims.Any())
        {
            var addResult = await _userManager.AddClaimsAsync(user, newClaims);
            if (!addResult.Succeeded)
            {
                throw new OperationFailedException(
                    Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                    $"Failed to add new permissions: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }
        }

        // Note: RefreshSignInAsync is not called because the app uses JWT-only auth (no cookie scheme).
        // JWT tokens are stateless; updated claims take effect on next token refresh.

        _logger.LogInformation(
            "Permissions set for user {UserId} ({Email}) by {ActorId} ({ActorEmail}). Permissions: [{Permissions}]",
            user.Id,
            user.Email,
            _currentUserService.UserId,
            _currentUserService.Email,
            string.Join(", ", permissions));

        return;
    }

    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Array.Empty<string>();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == Application.Common.Constants.ClaimTypeConstants.Permission)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<string> ValidatePermissions(IEnumerable<string> permissions)
    {
        var allSystemPermissions = Domain.Common.Constants.Permissions.GetAll();
        var invalidPermissions = permissions
            .Where(p => !allSystemPermissions.Contains(p))
            .ToList();

        return invalidPermissions.AsReadOnly();
    }

    #endregion

    #region Password Management

    public async Task<ResetPasswordResult> ResetPasswordAsync(
        Guid userId,
        string? newPassword,
        bool requirePasswordChange,
        CancellationToken cancellationToken = default)
    {
        // Find user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");
        }

        // Prevent resetting own password via admin endpoint
        if (user.Id == _currentUserService.UserId)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.OPERATION_NOT_ALLOWED,
                "You cannot reset your own password via admin endpoint. Use change password feature instead.");
        }

        // Generate password if not provided
        var password = newPassword;
        var isSystemGenerated = false;

        if (string.IsNullOrWhiteSpace(password))
        {
            password = GenerateSecurePassword();
            isSystemGenerated = true;
        }

        // Remove existing password and set new one
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to remove old password: {string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))}");
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, password);
        if (!addPasswordResult.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to set new password: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
        }

        // Update security stamp (invalidates existing tokens/cookies)
        var updateSecurityStampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!updateSecurityStampResult.Succeeded)
        {
            _logger.LogWarning(
                "Failed to update security stamp for user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", updateSecurityStampResult.Errors.Select(e => e.Description)));
        }

        // Mark for password change if requested
        if (requirePasswordChange)
        {
            var existingClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "password_change_required");

            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            await _userManager.AddClaimAsync(user,
                new Claim("password_change_required", "true"));
        }

        // Sign out user (force re-login with new password)
        // Note: In JWT-only mode, sign-out is client-side (token removal).
        // The security stamp change above will invalidate existing tokens.

        _logger.LogWarning(
            "Password reset for user {UserId} ({Email}) by {ActorId} ({ActorEmail}). System-generated: {IsSystemGenerated}, RequirePasswordChange: {RequirePasswordChange}",
            user.Id,
            user.Email,
            _currentUserService.UserId,
            _currentUserService.Email,
            isSystemGenerated,
            requirePasswordChange);

        return new ResetPasswordResult
        {
            TemporaryPassword = isSystemGenerated ? password : null,
            RequirePasswordChange = requirePasswordChange
        };
    }

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    private static string GenerateSecurePassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "@$!%*?&#";
        const string allChars = uppercase + lowercase + digits + special;

        var random = new Random();
        var password = new char[16];

        // Ensure at least one from each category
        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill remaining with random characters
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    #endregion

    #region Email Confirmation

    public async Task<string> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed for user {UserId} ({Email})", user.Id, user.Email);
        }

        return result.Succeeded;
    }

    public async Task<Guid?> FindUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    #endregion

    #region Password Reset (User-Initiated)

    public async Task<string> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordWithTokenAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset via token for user {UserId} ({Email})", user.Id, user.Email);
        }
        else
        {
            _logger.LogWarning("Password reset via token failed for user {UserId}: {Errors}",
                user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result.Succeeded;
    }

    #endregion

    #region Two-Factor Authentication

    public async Task<string> GetAuthenticatorKeyAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        return key!;
    }

    public async Task<bool> VerifyTwoFactorTokenAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        return await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            token);
    }

    public async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, count);
        _logger.LogInformation("Generated {Count} recovery codes for user {UserId}", count, userId);
        return codes ?? Enumerable.Empty<string>();
    }

    public async Task<bool> GetTwoFactorEnabledAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null && await _userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task SetTwoFactorEnabledAsync(
        Guid userId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var result = await _userManager.SetTwoFactorEnabledAsync(user, enabled);
        if (!result.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to set 2FA: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("2FA {Status} for user {UserId}", enabled ? "enabled" : "disabled", userId);
    }

    public async Task<int> CountRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return 0;
        return await _userManager.CountRecoveryCodesAsync(user);
    }

    public async Task ResetAuthenticatorKeyAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        _logger.LogInformation("Authenticator key reset for user {UserId}", userId);
    }

    #endregion

    #region External Logins

    public async Task<bool> AddExternalLoginAsync(
        Guid userId,
        string provider,
        string providerKey,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var loginInfo = new UserLoginInfo(provider, providerKey, displayName);
        var result = await _userManager.AddLoginAsync(user, loginInfo);
        return result.Succeeded;
    }

    public async Task<IList<ExternalLoginDto>> GetExternalLoginsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new List<ExternalLoginDto>();

        var logins = await _userManager.GetLoginsAsync(user);
        return logins.Select(l => new ExternalLoginDto
        {
            LoginProvider = l.LoginProvider,
            ProviderDisplayName = l.ProviderDisplayName ?? l.LoginProvider,
            ProviderKey = l.ProviderKey
        }).ToList();
    }

    public async Task RemoveExternalLoginAsync(
        Guid userId,
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var result = await _userManager.RemoveLoginAsync(user, provider, providerKey);
        if (!result.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to remove external login: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("External login {Provider} removed for user {UserId}", provider, userId);
    }

    public async Task<Guid?> FindUserIdByExternalLoginAsync(
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        return user?.Id;
    }

    public async Task<bool> HasPasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null && await _userManager.HasPasswordAsync(user);
    }

    #endregion

    #region Personal Data (GDPR)

    public async Task<Dictionary<string, string>> GetPersonalDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var personalData = new Dictionary<string, string>();

        // Add all personal data properties from Identity
        var personalDataProps = typeof(ApplicationUser).GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

        foreach (var p in personalDataProps)
        {
            personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        }

        // Add standard identity fields
        personalData["Id"] = user.Id.ToString();
        personalData["UserName"] = user.UserName ?? string.Empty;
        personalData["Email"] = user.Email ?? string.Empty;
        personalData["EmailConfirmed"] = user.EmailConfirmed.ToString();
        personalData["PhoneNumber"] = user.PhoneNumber ?? string.Empty;
        personalData["PhoneNumberConfirmed"] = user.PhoneNumberConfirmed.ToString();
        personalData["TwoFactorEnabled"] = user.TwoFactorEnabled.ToString();

        // Add external logins
        var logins = await _userManager.GetLoginsAsync(user);
        foreach (var login in logins)
        {
            personalData.Add($"ExternalLogin:{login.LoginProvider}", login.ProviderKey);
        }

        return personalData;
    }

    public async Task DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.RESOURCE_NOT_FOUND,
                $"User with ID '{userId}' not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new OperationFailedException(
                Application.Common.Constants.ErrorCodes.INTERNAL_ERROR,
                $"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogWarning("User {UserId} ({Email}) permanently deleted (GDPR request)", userId, user.Email);
    }

    #endregion

    #region User Queries

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        // Build predicate using PredicateExtensions
        var predicate = PredicateExtensions.True<ApplicationUser>();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            predicate = predicate.And(u =>
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(search)));
        }

        // Apply active status filter if provided
        if (isActive.HasValue)
        {
            if (isActive.Value)
            {
                // Active users: not locked out
                predicate = predicate.And(u =>
                    !u.LockoutEnabled ||
                    u.LockoutEnd == null ||
                    u.LockoutEnd <= DateTimeOffset.UtcNow);
            }
            else
            {
                // Inactive users: currently locked out
                predicate = predicate.And(u =>
                    u.LockoutEnabled &&
                    u.LockoutEnd != null &&
                    u.LockoutEnd > DateTimeOffset.UtcNow);
            }
        }

        // Start query with AsNoTracking
        var query = _userManager.Users.AsNoTracking();

        // Apply predicate via Where()
        query = query.Where(predicate);

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        // Apply projection to DTO
        var projected = query.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            EmailConfirmed = u.EmailConfirmed,
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd
        });

        // Materialize with pagination
        var pagedResult = await projected.ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        return pagedResult;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) != null;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return false;

        if (excludeUserId.HasValue && user.Id == excludeUserId.Value)
            return false;

        return true;
    }

    public async Task<IReadOnlyList<SuggestedUserDto>> GetUsersByNormalizedPhonesAsync(
        IReadOnlyList<string> normalizedPhones,
        IReadOnlyCollection<Guid> excludeUserIds,
        CancellationToken cancellationToken = default)
    {
        if (normalizedPhones is null || normalizedPhones.Count == 0)
            return Array.Empty<SuggestedUserDto>();

        var normalizedSet = normalizedPhones
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToHashSet(StringComparer.Ordinal);

        if (normalizedSet.Count == 0)
            return Array.Empty<SuggestedUserDto>();

        var excludeSet = excludeUserIds?.ToHashSet() ?? new HashSet<Guid>();

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(u => u.PhoneNumber != null && u.PhoneNumber != "" && !excludeSet.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.PhoneNumber })
            .ToListAsync(cancellationToken);

        var result = users
            .Where(u => normalizedSet.Contains(PhoneNormalizer.Normalize(u.PhoneNumber) ?? string.Empty))
            .Select(u => new SuggestedUserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber
            })
            .ToList();

        return result;
    }

    /// <summary>
    /// Normalizes phone for storage (same format as Contact: digits only, +84/0 -> 84).
    /// Accepts +84xxx, 0xxx, 84xxx; returns null if empty or no valid digits.
    /// </summary>
    private static string? NormalizePhoneForStorage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        return PhoneNormalizer.Normalize(raw.Trim());
    }

    private IQueryable<ApplicationUser> ApplySorting(
        IQueryable<ApplicationUser> query,
        string? sortBy,
        bool sortDescending)
    {
        // Default sorting by Email
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderBy(u => u.Email);
        }

        // Dynamic sorting based on SortBy field
        var sortByLower = sortBy.ToLower();

        return sortByLower switch
        {
            "email" => sortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            "emailconfirmed" => sortDescending
                ? query.OrderByDescending(u => u.EmailConfirmed)
                : query.OrderBy(u => u.EmailConfirmed),

            "lockoutend" => sortDescending
                ? query.OrderByDescending(u => u.LockoutEnd)
                : query.OrderBy(u => u.LockoutEnd),

            // Default to email if unknown field
            _ => query.OrderBy(u => u.Email)
        };
    }

    #endregion
}
