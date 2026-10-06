using FluentValidation;

namespace ThaiX.Application.Features.Contacts.Commands.StartContactImport;

/// <summary>
/// Validates StartContactImportCommand: FilePath required, BatchSize in range.
/// </summary>
public sealed class StartContactImportCommandValidator : AbstractValidator<StartContactImportCommand>
{
    private const int MinBatchSize = 1;
    private const int MaxBatchSize = 5000;

    public StartContactImportCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .MaximumLength(2048);

        When(x => x.BatchSize.HasValue, () =>
        {
            RuleFor(x => x.BatchSize!.Value)
                .InclusiveBetween(MinBatchSize, MaxBatchSize);
        });
    }
}
