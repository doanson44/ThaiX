using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;

public sealed class GetTradeSuggestionQueryValidator : AbstractValidator<GetTradeSuggestionQuery>
{
    private static readonly HashSet<string> SupportedCryptoTimeframes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Min1", "Min5", "Min15", "Min30", "Min60",
        "Hour4", "Hour8", "Day1", "Week1", "Month1"
    };

    private static readonly HashSet<string> SupportedStockLongTimeframes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Week1", "Month1"
    };

    public GetTradeSuggestionQueryValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(30).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.MarketType)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_REQUEST);

        RuleFor(x => x.Timeframe)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Timeframe)
            .Must((request, timeframe) =>
                (request.MarketType != MarketType.Crypto && request.MarketType != MarketType.CryptoSpot)
                || IsValidCryptoTimeframe(timeframe))
            .WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Timeframe));

        RuleFor(x => x.UseLongTermTimeframe)
            .Must((request, useLongTermTimeframe) =>
                request.MarketType == MarketType.Stock || !useLongTermTimeframe)
            .WithErrorCode(ErrorCodes.INVALID_REQUEST);

        RuleFor(x => x.Timeframe)
            .Must((request, timeframe) => IsValidStockTimeframe(request, timeframe))
            .WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .When(x => x.MarketType == MarketType.Stock && !string.IsNullOrWhiteSpace(x.Timeframe));

        RuleFor(x => x.MarketRegime)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_REQUEST);

        RuleFor(x => x.EventRisk)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_REQUEST);
    }

    private static bool IsValidCryptoTimeframe(string timeframe)
        => SupportedCryptoTimeframes.Contains(timeframe.Trim());

    private static bool IsValidStockTimeframe(GetTradeSuggestionQuery request, string timeframe)
    {
        var normalized = timeframe.Trim();
        if (request.UseLongTermTimeframe)
        {
            return SupportedStockLongTimeframes.Contains(normalized);
        }

        return string.Equals(normalized, "Day1", StringComparison.OrdinalIgnoreCase);
    }
}
