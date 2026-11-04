using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.Features.Authentication.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandHandler(
    IEmailOtpService emailOtpService,
    IAuthenticationService authenticationService)
    : IRequestHandler<VerifyEmailOtpCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(
        VerifyEmailOtpCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await emailOtpService.VerifyAsync(request.ChallengeId, request.Code, cancellationToken);
        var result = await authenticationService.AuthenticateWithEmailOtpAsync(
            userId,
            request.RememberMe,
            cancellationToken);

        return new LoginResultDto
        {
            Succeeded = result.Succeeded,
            RequiresTwoFactor = result.RequiresTwoFactor,
            TwoFactorSessionToken = result.TwoFactorSessionToken,
            Token = result.Token is null
                ? null
                : new TokenDto
                {
                    AccessToken = result.Token.AccessToken,
                    TokenType = result.Token.TokenType,
                    ExpiresIn = result.Token.ExpiresIn,
                    Scope = result.Token.Scope
                }
        };
    }
}
