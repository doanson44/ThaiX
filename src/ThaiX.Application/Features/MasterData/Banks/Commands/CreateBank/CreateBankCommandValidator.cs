using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.CreateBank;

/// <summary>
/// Validator for CreateBankCommand.
/// </summary>
public sealed class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(10)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
