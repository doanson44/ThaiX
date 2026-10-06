using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Portfolios.Commands.CreatePortfolio;

public sealed class CreatePortfolioCommandValidator : AbstractValidator<CreatePortfolioCommand>
{
    public CreatePortfolioCommandValidator()
    {
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
