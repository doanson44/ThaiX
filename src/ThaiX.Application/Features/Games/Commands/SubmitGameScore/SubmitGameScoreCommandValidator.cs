using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Games.Commands.SubmitGameScore;

public sealed class SubmitGameScoreCommandValidator : AbstractValidator<SubmitGameScoreCommand>
{
    public SubmitGameScoreCommandValidator()
    {
        RuleFor(command => command.GameId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Must(GameScoreRules.IsSupported)
            .WithErrorCode(ErrorCodes.INVALID_REQUEST);

        RuleFor(command => command.RunId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(command => command.PlayerId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(80)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(command => command.UserId)
            .NotNull()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .When(command => command.PlayerId?.StartsWith("user:", StringComparison.Ordinal) == true);

        RuleFor(command => command.PlayerName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MinimumLength(2)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .MaximumLength(64)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Must(name => name.Trim().Length >= 2)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Must(name => name.All(character => !char.IsControl(character)))
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(command => command.Score)
            .InclusiveBetween(0, 1_000_000)
            .WithErrorCode(ErrorCodes.INVALID_RANGE)
            .Must((command, score) => GameScoreRules.IsValidScore(command.GameId, score))
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(command => command.DurationSeconds)
            .InclusiveBetween(1, 86_400)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);

        RuleFor(command => command.Mode)
            .MaximumLength(24)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
