using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.ImportCountries;

/// <summary>
/// Handler for ImportCountriesCommand.
/// Upserts countries from CSV: insert new, update existing (matched by Code).
/// </summary>
public sealed class ImportCountriesCommandHandler : IRequestHandler<ImportCountriesCommand, ImportResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ImportCountriesCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImportResult> Handle(ImportCountriesCommand request, CancellationToken cancellationToken)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        List<CsvRow> rows;
        using (var reader = new StreamReader(request.CsvStream))
        using (var csv = new CsvReader(reader, csvConfig))
        {
            rows = csv.GetRecords<CsvRow>().ToList();
        }

        // Load existing countries for upsert
        var existingCountries = await _dbContext.Countries
            .ToDictionaryAsync(c => c.Code, c => c, cancellationToken);

        var errors = new List<ImportRowError>();
        int inserted = 0, updated = 0, skipped = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2; // 1-based, skip header

            try
            {
                if (string.IsNullOrWhiteSpace(row.Code))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = $"{row.Code},{row.Name}", Error = "Code is required." });
                    skipped++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = $"{row.Code},{row.Name}", Error = "Name is required." });
                    skipped++;
                    continue;
                }

                var code = row.Code.Trim().ToUpperInvariant();

                if (existingCountries.TryGetValue(code, out var existing))
                {
                    existing.UpdateDetails(row.Code, row.Name);
                    updated++;
                }
                else
                {
                    var country = Country.Create(row.Code, row.Name);
                    _dbContext.Countries.Add(country);
                    existingCountries[code] = country;
                    inserted++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = $"{row.Code},{row.Name}", Error = ex.Message });
                skipped++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ImportResult
        {
            TotalRows = rows.Count,
            InsertedCount = inserted,
            UpdatedCount = updated,
            SkippedCount = skipped,
            Errors = errors
        };
    }

    private sealed class CsvRow
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }
}
