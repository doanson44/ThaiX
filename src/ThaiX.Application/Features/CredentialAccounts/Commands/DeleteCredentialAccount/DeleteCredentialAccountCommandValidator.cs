using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.DeleteCredentialAccount;

public sealed class DeleteCredentialAccountCommandValidator : AbstractValidator<DeleteCredentialAccountCommand>
{
    public DeleteCredentialAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
