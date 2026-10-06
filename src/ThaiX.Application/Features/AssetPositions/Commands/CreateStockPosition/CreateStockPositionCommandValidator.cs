using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateStockPosition;

public sealed class CreateStockPositionCommandValidator : AbstractValidator<CreateStockPositionCommand>
{
    public CreateStockPositionCommandValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(16)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Exchange)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.TargetPrice)
            .GreaterThan(0)
            .When(x => x.TargetPrice.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.StopLoss)
            .GreaterThan(0)
            .When(x => x.StopLoss.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
