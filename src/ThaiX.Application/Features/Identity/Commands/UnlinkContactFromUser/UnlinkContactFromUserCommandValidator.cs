using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkContactFromUser;

public sealed class UnlinkContactFromUserCommandValidator : AbstractValidator<UnlinkContactFromUserCommand>
{
    public UnlinkContactFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
