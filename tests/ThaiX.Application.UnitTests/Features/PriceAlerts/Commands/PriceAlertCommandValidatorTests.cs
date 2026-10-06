using ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;
using ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Commands;

public sealed class PriceAlertCommandValidatorTests
{
    [Fact]
    public void CreatePriceAlertCommandValidator_ShouldValidateRequiredFields()
    {
        var validator = new CreatePriceAlertCommandValidator();
        var command = new CreatePriceAlertCommand
        {
            Symbol = string.Empty,
            AssetType = (AssetType)(-1),
            Condition = (AlertCondition)(-1),
            TargetPrice = 0m,
            Note = new string('x', 501),
            IsOneTime = false
        };

        var result = validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Symbol));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.AssetType));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Condition));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TargetPrice));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Note));
    }

    [Fact]
    public void UpdatePriceAlertCommandValidator_ShouldValidateRequiredFields()
    {
        var validator = new UpdatePriceAlertCommandValidator();
        var command = new UpdatePriceAlertCommand
        {
            Id = Guid.Empty,
            Condition = (AlertCondition)(-1),
            TargetPrice = 0m,
            Note = new string('x', 501),
            IsOneTime = false,
            IsEnabled = true
        };

        var result = validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Condition));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TargetPrice));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Note));
    }
}
