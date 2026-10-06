using FluentValidation;

namespace ThaiX.Application.Features.Portfolios.Commands.ImportPortfolios;

public sealed class ImportPortfoliosCommandValidator : AbstractValidator<ImportPortfoliosCommand>
{
    public ImportPortfoliosCommandValidator()
    {
        RuleFor(x => x.CsvStream)
            .NotNull();
    }
}
