using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;

public sealed class RequestEmailOtpCommandHandler(IEmailOtpService emailOtpService)
    : IRequestHandler<RequestEmailOtpCommand, EmailOtpChallengeInfo>
{
    public Task<EmailOtpChallengeInfo> Handle(RequestEmailOtpCommand request, CancellationToken cancellationToken) =>
        emailOtpService.RequestAsync(request.Email, cancellationToken);
}
