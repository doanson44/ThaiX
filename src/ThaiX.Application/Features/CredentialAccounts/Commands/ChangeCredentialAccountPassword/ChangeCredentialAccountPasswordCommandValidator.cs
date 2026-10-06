using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.ChangeCredentialAccountPassword;

public sealed class ChangeCredentialAccountPasswordCommandValidator : AbstractValidator<ChangeCredentialAccountPasswordCommand>
{
    public ChangeCredentialAccountPasswordCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(1000)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
