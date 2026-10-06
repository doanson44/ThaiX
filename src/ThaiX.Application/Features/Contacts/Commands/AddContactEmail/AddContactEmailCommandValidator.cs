using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactEmail;

public sealed class AddContactEmailCommandValidator : AbstractValidator<AddContactEmailCommand>
{
    public AddContactEmailCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Value)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(320).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .EmailAddress().WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
