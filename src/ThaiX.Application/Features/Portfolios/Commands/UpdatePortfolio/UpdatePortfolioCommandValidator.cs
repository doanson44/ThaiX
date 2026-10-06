using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Portfolios.Commands.UpdatePortfolio;

public sealed class UpdatePortfolioCommandValidator : AbstractValidator<UpdatePortfolioCommand>
{
    public UpdatePortfolioCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.PortfolioType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
