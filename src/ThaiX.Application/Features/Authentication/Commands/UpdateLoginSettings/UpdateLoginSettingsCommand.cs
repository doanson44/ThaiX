using MediatR;

namespace ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;

public sealed record UpdateLoginSettingsCommand(Guid UserId, bool EmailOtpLoginEnabled) : IRequest;
