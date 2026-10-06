using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Blog.Posts.Commands.SchedulePost;

public sealed class SchedulePostCommandValidator : AbstractValidator<SchedulePostCommand>
{
    public SchedulePostCommandValidator()
    {
        RuleFor(x => x.PublishAt)
            .GreaterThan(_ => DateTime.UtcNow)
            .WithErrorCode(ErrorCodes.INVALID_RANGE)
            .WithMessage("Scheduled publish time must be in the future.");
    }
}
