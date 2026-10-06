using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Authenticates user and generates JWT token (or requests 2FA).
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.AuthenticateUserAsync(
            request.Username,
            request.Password,
            request.RememberMe,
            cancellationToken);

        if (result.RequiresTwoFactor)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                RequiresTwoFactor = true,
                TwoFactorSessionToken = result.TwoFactorSessionToken
            };
        }

        return new LoginResultDto
        {
            Succeeded = true,
            RequiresTwoFactor = false,
            Token = result.Token != null ? new TokenDto
            {
                AccessToken = result.Token.AccessToken,
                TokenType = result.Token.TokenType,
                ExpiresIn = result.Token.ExpiresIn,
                Scope = result.Token.Scope
            } : null
        };
    }
}
