using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.AddStockTransaction;

public sealed class AddStockTransactionCommandValidator : AbstractValidator<AddStockTransactionCommand>
{
    public AddStockTransactionCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.TransactionType)
            .Must(t => t is TransactionType.Buy or TransactionType.Sell)
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
    }
}
