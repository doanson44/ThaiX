using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerRule;

/// <summary>
/// Validator for UpdateMarketScannerRuleCommand.
/// </summary>
public sealed class UpdateMarketScannerRuleCommandValidator
    : AbstractValidator<UpdateMarketScannerRuleCommand>
{
    public UpdateMarketScannerRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Window)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Threshold)
            .GreaterThan(0)
            .When(x => x.Threshold.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
