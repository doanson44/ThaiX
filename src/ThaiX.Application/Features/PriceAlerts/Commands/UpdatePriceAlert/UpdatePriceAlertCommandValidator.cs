using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;

public sealed class UpdatePriceAlertCommandValidator : AbstractValidator<UpdatePriceAlertCommand>
{
    public UpdatePriceAlertCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

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
