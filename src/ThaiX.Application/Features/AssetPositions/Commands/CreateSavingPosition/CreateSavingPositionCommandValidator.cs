using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateSavingPosition;

public sealed class CreateSavingPositionCommandValidator : AbstractValidator<CreateSavingPositionCommand>
{
    public CreateSavingPositionCommandValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.BankName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50)
            .When(x => x.AccountNumber is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.PrincipalAmount)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.InterestRate)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.InterestType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.MaturityDate)
            .GreaterThan(x => x.DepositDate)
            .When(x => x.MaturityDate.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
