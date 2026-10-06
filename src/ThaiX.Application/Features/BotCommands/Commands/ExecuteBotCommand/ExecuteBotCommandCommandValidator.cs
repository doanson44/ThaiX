using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;

public sealed class ExecuteBotCommandCommandValidator : AbstractValidator<ExecuteBotCommandCommand>
{
    public ExecuteBotCommandCommandValidator()
    {
        RuleFor(x => x.RawText)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(500).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Channel)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(30).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.ExternalChannelId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
