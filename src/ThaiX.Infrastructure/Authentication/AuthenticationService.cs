using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Security;

namespace ThaiX.Infrastructure.Authentication;

/// <summary>
/// Implementation of IAuthenticationService.
/// Handles user, client credentials, and 2FA authentication.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IdentityUserService _identityUserService;
    private readonly ApiClientService _apiClientService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService,
        IdentityUserService identityUserService,
        ApiClientService apiClientService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _identityUserService = identityUserService;
        _apiClientService = apiClientService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(
        string usernameOrEmail,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(usernameOrEmail)
                   ?? await _userManager.FindByNameAsync(usernameOrEmail);

        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {UsernameOrEmail}", usernameOrEmail);
            throw new OperationFailedException(
                ErrorCodes.INVALID_CREDENTIALS,
                ErrorMessages.INVALID_CREDENTIALS);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);

        if (signInResult.RequiresTwoFactor)
        {
            _logger.LogInformation("2FA required for user: {UserId}", user.Id);
            var sessionToken = _jwtTokenService.GenerateTwoFactorSessionToken(user.Id);
            return new AuthenticationResult
            {
                Succeeded = false,
                RequiresTwoFactor = true,
                TwoFactorSessionToken = sessionToken,
                Token = null
            };
        }

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("Login attempt for locked out user: {UserId}", user.Id);
                throw new OperationFailedException(
                    ErrorCodes.ACCOUNT_LOCKED,
                    ErrorMessages.ACCOUNT_LOCKED);
            }

            if (signInResult.IsNotAllowed)
            {
                _logger.LogWarning("Login attempt for user not allowed to sign in: {UserId}", user.Id);
                throw new OperationFailedException(
                    ErrorCodes.ACCOUNT_NOT_ALLOWED,
                    ErrorMessages.ACCOUNT_NOT_ALLOWED);
            }

            _logger.LogWarning("Failed login attempt for user: {UsernameOrEmail}", usernameOrEmail);
            throw new OperationFailedException(
                ErrorCodes.INVALID_CREDENTIALS,
                ErrorMessages.INVALID_CREDENTIALS);
        }

        var tokenResult = await GenerateUserTokenResultAsync(user, rememberMe, cancellationToken);

        _logger.LogInformation("User logged in successfully: {UserId}, {Email}", user.Id, user.Email);

        return new AuthenticationResult
        {
            Succeeded = true,
            RequiresTwoFactor = false,
            Token = tokenResult
        };
    }

    public async Task<TokenResult> AuthenticateWithTwoFactorAsync(
        string twoFactorSessionToken,
        string twoFactorCode,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtTokenService.ValidateTwoFactorSessionToken(twoFactorSessionToken);
        if (userId == null)
        {
            throw new OperationFailedException(
                ErrorCodes.INVALID_TOKEN,
                ErrorMessages.INVALID_TOKEN);
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                ErrorMessages.RESOURCE_NOT_FOUND);
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            twoFactorCode);

        if (!isValid)
        {
            _logger.LogWarning("Invalid 2FA code for user: {UserId}", user.Id);
            throw new OperationFailedException(
                ErrorCodes.TWO_FACTOR_INVALID,
                "Invalid two-factor authentication code.");
        }

        // Reset lockout count on successful 2FA
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("User logged in with 2FA: {UserId}, {Email}", user.Id, user.Email);

        return await GenerateUserTokenResultAsync(user, false, cancellationToken);
    }

    public async Task<TokenResult> AuthenticateWithRecoveryCodeAsync(
        string twoFactorSessionToken,
        string recoveryCode,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtTokenService.ValidateTwoFactorSessionToken(twoFactorSessionToken);
        if (userId == null)
        {
            throw new OperationFailedException(
                ErrorCodes.INVALID_TOKEN,
                ErrorMessages.INVALID_TOKEN);
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                ErrorMessages.RESOURCE_NOT_FOUND);
        }

        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode.Replace(" ", string.Empty).Replace("-", string.Empty));
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid recovery code for user: {UserId}", user.Id);
            throw new OperationFailedException(
                ErrorCodes.RECOVERY_CODE_INVALID,
                "Invalid recovery code.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("User logged in with recovery code: {UserId}, {Email}", user.Id, user.Email);

        return await GenerateUserTokenResultAsync(user, false, cancellationToken);
    }

    public async Task<AuthenticationResult> AuthenticateExternalUserAsync(
        string provider,
        string providerKey,
        string email,
        CancellationToken cancellationToken = default)
    {
        // Try to find existing user by external login
        var user = await _userManager.FindByLoginAsync(provider, providerKey);

        if (user == null)
        {
            // Try to find by email
            user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // External provider confirmed
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create user from external login: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    throw new OperationFailedException(
                        ErrorCodes.EXTERNAL_LOGIN_FAILED,
                        "Failed to create account from external login.");
                }

                _logger.LogInformation("Created user from external login: {UserId}, {Email}, {Provider}",
                    user.Id, email, provider);
            }

            // Link external login
            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                _logger.LogError("Failed to add external login: {Errors}",
                    string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                throw new OperationFailedException(
                    ErrorCodes.EXTERNAL_LOGIN_FAILED,
                    "Failed to link external login.");
            }
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new OperationFailedException(
                ErrorCodes.ACCOUNT_LOCKED,
                ErrorMessages.ACCOUNT_LOCKED);
        }

        var tokenResult = await GenerateUserTokenResultAsync(user, false, cancellationToken);

        _logger.LogInformation("External login successful: {UserId}, {Email}, {Provider}",
            user.Id, user.Email, provider);

        return new AuthenticationResult
        {
            Succeeded = true,
            RequiresTwoFactor = false,
            Token = tokenResult
        };
    }

    public async Task<TokenResult> AuthenticateClientAsync(
        string clientId,
        string clientSecret,
        string? requestedScope,
        CancellationToken cancellationToken = default)
    {
        var client = await _apiClientService.AuthenticateAsync(
            clientId, clientSecret, cancellationToken);

        if (client == null)
        {
            _logger.LogWarning("Failed client credentials authentication: {ClientId}", clientId);
            throw new OperationFailedException(
                ErrorCodes.INVALID_CLIENT_CREDENTIALS,
                ErrorMessages.INVALID_CLIENT_CREDENTIALS);
        }

        if (!client.IsActive)
        {
            _logger.LogWarning("Inactive client attempted authentication: {ClientId}", clientId);
            throw new OperationFailedException(
                ErrorCodes.CLIENT_INACTIVE,
                ErrorMessages.CLIENT_INACTIVE);
        }

        var requestedScopes = requestedScope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        var grantedScopes = client.Scopes.ToList();

        if (requestedScopes.Any())
        {
            var unauthorizedScopes = requestedScopes.Except(grantedScopes).ToList();
            if (unauthorizedScopes.Any())
            {
                _logger.LogWarning(
                    "Client {ClientId} requested unauthorized scopes: {Scopes}",
                    clientId,
                    string.Join(", ", unauthorizedScopes));

                throw new OperationFailedException(
                    ErrorCodes.UNAUTHORIZED_SCOPE,
                    $"{ErrorMessages.UNAUTHORIZED_SCOPE}: {string.Join(", ", unauthorizedScopes)}");
            }

            grantedScopes = requestedScopes.ToList();
        }

        var token = _jwtTokenService.GenerateSystemToken(client.ClientId, grantedScopes);

        _logger.LogInformation(
            "System token issued: ClientId={ClientId}, Scopes={Scopes}",
            client.ClientId,
            string.Join(", ", grantedScopes));

        return new TokenResult
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = string.Join(" ", grantedScopes)
        };
    }

    private async Task<TokenResult> GenerateUserTokenResultAsync(
        ApplicationUser user,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        var permissions = await _identityUserService.GetPermissionsAsync(user.Id, cancellationToken);
        var token = _jwtTokenService.GenerateUserToken(user.Id, user.Email!, permissions, rememberMe);
        var expiresIn = _jwtTokenService.GetUserTokenExpiresInSeconds(rememberMe);

        return new TokenResult
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            Scope = string.Join(" ", permissions)
        };
    }
}
