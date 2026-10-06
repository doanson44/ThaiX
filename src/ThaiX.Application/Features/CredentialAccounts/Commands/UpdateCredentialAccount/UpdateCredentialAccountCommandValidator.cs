using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.UpdateCredentialAccount;

public sealed class UpdateCredentialAccountCommandValidator : AbstractValidator<UpdateCredentialAccountCommand>
{
    public UpdateCredentialAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
