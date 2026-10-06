using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactBankAccount;

public sealed class AddContactBankAccountCommandValidator : AbstractValidator<AddContactBankAccountCommand>
{
    public AddContactBankAccountCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.BankCode)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.BranchName)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.BranchName is not null);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(50).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.AccountName)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Length(3).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
