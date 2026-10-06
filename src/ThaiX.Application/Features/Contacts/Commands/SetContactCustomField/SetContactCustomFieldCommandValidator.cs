using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactCustomField;

public sealed class SetContactCustomFieldCommandValidator : AbstractValidator<SetContactCustomFieldCommand>
{
    public SetContactCustomFieldCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Key)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Value)
            .NotNull().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(1000).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
