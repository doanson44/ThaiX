using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Contacts.Commands.UpsertContacts;
using ThaiX.Application.Features.Contacts.Models;

namespace ThaiX.Application.Features.Contacts.Commands.ImportContacts;

/// <summary>
/// Orchestrates contact import: parse CSV via IContactCsvParser, batch rows, send UpsertContactsCommand per batch.
/// No CSV or domain logic here; parsing and upsert are separated.
/// </summary>
public sealed class ImportContactsCommandHandler : IRequestHandler<ImportContactsCommand, ImportResult>
{
    private const int DefaultBatchSize = 500;
    private const int MaxBatchSize = 5000;

    private readonly IContactCsvParser _parser;
    private readonly IMediator _mediator;

    public ImportContactsCommandHandler(IContactCsvParser parser, IMediator mediator)
    {
        _parser = parser;
        _mediator = mediator;
    }

    public async Task<ImportResult> Handle(ImportContactsCommand request, CancellationToken cancellationToken)
    {
        var batchSize = Math.Clamp(request.BatchSize ?? DefaultBatchSize, 1, MaxBatchSize);
        var totalRows = 0;
        var inserted = 0;
        var updated = 0;
        var skipped = 0;
        var allErrors = new List<ImportRowError>();

        var batch = new List<ParsedImportRow>();

        await foreach (var parsed in _parser.ParseAsync(request.CsvStream, cancellationToken))
        {
            batch.Add(parsed);
            if (batch.Count < batchSize)
                continue;

            totalRows += batch.Count;
            var result = await _mediator.Send(new UpsertContactsCommand { Rows = batch }, cancellationToken);
            inserted += result.InsertedCount;
            updated += result.UpdatedCount;
            skipped += result.SkippedCount;
            allErrors.AddRange(result.Errors);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            totalRows += batch.Count;
            var result = await _mediator.Send(new UpsertContactsCommand { Rows = batch }, cancellationToken);
            inserted += result.InsertedCount;
            updated += result.UpdatedCount;
            skipped += result.SkippedCount;
            allErrors.AddRange(result.Errors);
        }

        return new ImportResult
        {
            TotalRows = totalRows,
            InsertedCount = inserted,
            UpdatedCount = updated,
            SkippedCount = skipped,
            Errors = allErrors
        };
    }
}
