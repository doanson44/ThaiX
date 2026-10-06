using FluentValidation;

namespace ThaiX.Application.Features.Contacts.Commands.ImportContacts;

/// <summary>
/// Validates ImportContactsCommand: BatchSize in range when provided.
/// </summary>
public sealed class ImportContactsCommandValidator : AbstractValidator<ImportContactsCommand>
{
    private const int MaxBatchSize = 5000;

    public ImportContactsCommandValidator()
    {
        When(x => x.BatchSize.HasValue, () =>
        {
            RuleFor(x => x.BatchSize!.Value)
                .InclusiveBetween(1, MaxBatchSize);
        });
    }
}
