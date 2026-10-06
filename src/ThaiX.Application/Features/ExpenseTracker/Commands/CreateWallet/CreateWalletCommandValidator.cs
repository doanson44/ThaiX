using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateWallet;

/// <summary>
/// Validator for CreateWalletCommand.
/// </summary>
public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.WalletType)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(8).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
