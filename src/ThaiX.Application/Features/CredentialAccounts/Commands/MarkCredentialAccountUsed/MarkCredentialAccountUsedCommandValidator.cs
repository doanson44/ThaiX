using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.MarkCredentialAccountUsed;

public sealed class MarkCredentialAccountUsedCommandValidator : AbstractValidator<MarkCredentialAccountUsedCommand>
{
    public MarkCredentialAccountUsedCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
