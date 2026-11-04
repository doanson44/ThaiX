using MediatR;
using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.Features.Authentication.Commands.VerifyEmailOtp;

public sealed record VerifyEmailOtpCommand(Guid ChallengeId, string Code, bool RememberMe)
    : IRequest<LoginResultDto>;
