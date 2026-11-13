using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseGold;

public sealed class SaveTowerDefenseGoldCommandValidator : AbstractValidator<SaveTowerDefenseGoldCommand>
{
    public SaveTowerDefenseGoldCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(command => command.Gold)
            .InclusiveBetween(0, 1_000_000_000)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
