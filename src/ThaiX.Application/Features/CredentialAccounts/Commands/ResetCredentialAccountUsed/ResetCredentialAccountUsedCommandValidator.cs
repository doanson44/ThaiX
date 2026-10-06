using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.ResetCredentialAccountUsed;

public sealed class ResetCredentialAccountUsedCommandValidator : AbstractValidator<ResetCredentialAccountUsedCommand>
{
    public ResetCredentialAccountUsedCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
