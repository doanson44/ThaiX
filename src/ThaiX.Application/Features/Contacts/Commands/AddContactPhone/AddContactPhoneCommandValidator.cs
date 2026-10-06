using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactPhone;

public sealed class AddContactPhoneCommandValidator : AbstractValidator<AddContactPhoneCommand>
{
    public AddContactPhoneCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Value)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
