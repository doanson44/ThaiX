using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MarketScanner.Commands.CreateMarketScannerRule;

/// <summary>
/// Validator for CreateMarketScannerRuleCommand.
/// </summary>
public sealed class CreateMarketScannerRuleCommandValidator
    : AbstractValidator<CreateMarketScannerRuleCommand>
{
    public CreateMarketScannerRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.SignalType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Window)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Threshold)
            .GreaterThan(0)
            .When(x => x.Threshold.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
