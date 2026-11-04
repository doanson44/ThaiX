using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;

public sealed class UpdateLoginSettingsCommandHandler(IEmailOtpService emailOtpService)
    : IRequestHandler<UpdateLoginSettingsCommand>
{
    public async Task Handle(UpdateLoginSettingsCommand request, CancellationToken cancellationToken)
    {
        await emailOtpService.SetLoginEnabledAsync(
            request.UserId,
            request.EmailOtpLoginEnabled,
            cancellationToken);
    }
}
