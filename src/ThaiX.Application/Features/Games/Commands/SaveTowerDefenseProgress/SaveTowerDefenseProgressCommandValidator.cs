using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseProgress;

public sealed class SaveTowerDefenseProgressCommandValidator : AbstractValidator<SaveTowerDefenseProgressCommand>
{
    public SaveTowerDefenseProgressCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(command => command.Progress)
            .NotNull()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        When(command => command.Progress is not null, () =>
        {
            RuleFor(command => command.Progress.Credits)
                .InclusiveBetween(0, 1_000_000_000)
                .WithErrorCode(ErrorCodes.INVALID_RANGE);

            RuleFor(command => command.Progress.BonusStartingGold)
                .InclusiveBetween(0, 1_000_000)
                .WithErrorCode(ErrorCodes.INVALID_RANGE);

            RuleFor(command => command.Progress.ClearedMissions)
                .NotNull()
                .Must(items => items is { Length: <= 100 } && items.All(item => !string.IsNullOrWhiteSpace(item) && item.Length <= 64))
                .WithErrorCode(ErrorCodes.INVALID_RANGE);

            RuleFor(command => command.Progress.UnlockedTowers)
                .NotNull()
                .Must(items => items is { Length: <= 32 } && items.All(item => !string.IsNullOrWhiteSpace(item) && item.Length <= 64))
                .WithErrorCode(ErrorCodes.INVALID_RANGE);

            RuleFor(command => command.Progress.BonusMissions)
                .NotNull()
                .Must(items => items is { Length: <= 32 } && items.All(item => !string.IsNullOrWhiteSpace(item) && item.Length <= 64))
                .WithErrorCode(ErrorCodes.INVALID_RANGE);
        });
    }
}
