using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkUserFromContact;

public sealed class UnlinkUserFromContactCommandValidator : AbstractValidator<UnlinkUserFromContactCommand>
{
    public UnlinkUserFromContactCommandValidator()
    {
        RuleFor(x => x.ContactId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
