using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;

public sealed class UpdateLoginSettingsCommandHandler(IIdentityUserService identityUserService)
    : IRequestHandler<UpdateLoginSettingsCommand>
{
    public async Task Handle(UpdateLoginSettingsCommand request, CancellationToken cancellationToken)
    {
        await identityUserService.SetEmailOtpLoginEnabledAsync(
            request.UserId,
            request.EmailOtpLoginEnabled,
            cancellationToken);
    }
}
