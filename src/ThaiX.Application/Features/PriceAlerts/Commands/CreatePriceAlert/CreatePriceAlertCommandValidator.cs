using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;

public sealed class CreatePriceAlertCommandValidator : AbstractValidator<CreatePriceAlertCommand>
{
    public CreatePriceAlertCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(32)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.AssetType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Condition)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.TargetPrice)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
