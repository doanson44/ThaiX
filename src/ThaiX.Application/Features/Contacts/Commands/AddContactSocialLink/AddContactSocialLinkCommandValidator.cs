using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactSocialLink;

public sealed class AddContactSocialLinkCommandValidator : AbstractValidator<AddContactSocialLinkCommand>
{
    public AddContactSocialLinkCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Platform)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(50).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Url)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(2048).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
