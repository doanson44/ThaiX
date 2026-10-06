using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactIdentityDocument;

public sealed class AddContactIdentityDocumentCommandValidator : AbstractValidator<AddContactIdentityDocumentCommand>
{
    public AddContactIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(50).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(50).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.IssuedBy)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.IssuedPlace)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.IssuedDate)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.IssuedDate).WithErrorCode(ErrorCodes.INVALID_RANGE)
            .When(x => x.ExpiryDate.HasValue);
    }
}
