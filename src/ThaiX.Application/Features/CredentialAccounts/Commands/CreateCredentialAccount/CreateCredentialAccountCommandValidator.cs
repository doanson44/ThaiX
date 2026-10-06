using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.CreateCredentialAccount;

public sealed class CreateCredentialAccountCommandValidator : AbstractValidator<CreateCredentialAccountCommand>
{
    public CreateCredentialAccountCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(1000)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
