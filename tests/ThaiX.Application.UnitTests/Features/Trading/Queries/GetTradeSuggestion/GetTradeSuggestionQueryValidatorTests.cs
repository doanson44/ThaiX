using ThaiX.Application.Common.Constants;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Trading.Queries.GetTradeSuggestion;

public sealed class GetTradeSuggestionQueryValidatorTests
{
    [Fact]
    public void Validate_WhenSymbolIsEmpty_ShouldHaveRequiredFieldError()
    {
        var validator = new GetTradeSuggestionQueryValidator();
        var request = new GetTradeSuggestionQuery
        {
            Symbol = string.Empty,
            MarketType = MarketType.Crypto,
            Timeframe = "Min1",
            UseLongTermTimeframe = false
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Symbol) && e.ErrorCode == ErrorCodes.REQUIRED_FIELD);
    }

    [Fact]
    public void Validate_WhenCryptoTimeframeIsInvalid_ShouldHaveInvalidFormatError()
    {
        var validator = new GetTradeSuggestionQueryValidator();
        var request = new GetTradeSuggestionQuery
        {
            Symbol = "BTCUSDT",
            MarketType = MarketType.Crypto,
            Timeframe = "InvalidTimeframe",
            UseLongTermTimeframe = false
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Timeframe) && e.ErrorCode == ErrorCodes.INVALID_FORMAT);
    }

    [Fact]
    public void Validate_WhenStockTimeframeNotLongTerm_ShouldHaveInvalidFormatError()
    {
        var validator = new GetTradeSuggestionQueryValidator();
        var request = new GetTradeSuggestionQuery
        {
            Symbol = "AAPL",
            MarketType = MarketType.Stock,
            Timeframe = "Week1",
            UseLongTermTimeframe = false
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Timeframe) && e.ErrorCode == ErrorCodes.INVALID_FORMAT);
    }

    [Fact]
    public void Validate_WhenStockTimeframeWithLongTermFlag_ShouldBeValid()
    {
        var validator = new GetTradeSuggestionQueryValidator();
        var request = new GetTradeSuggestionQuery
        {
            Symbol = "AAPL",
            MarketType = MarketType.Stock,
            Timeframe = "Week1",
            UseLongTermTimeframe = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
