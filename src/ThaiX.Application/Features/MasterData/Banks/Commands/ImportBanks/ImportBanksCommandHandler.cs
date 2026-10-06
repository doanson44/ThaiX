using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.ImportBanks;

/// <summary>
/// Handler for ImportBanksCommand.
/// Upserts banks from CSV: insert new, update existing (matched by Code).
/// </summary>
public sealed class ImportBanksCommandHandler : IRequestHandler<ImportBanksCommand, ImportResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ImportBanksCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImportResult> Handle(ImportBanksCommand request, CancellationToken cancellationToken)
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

        // Load lookup data
        var countries = await _dbContext.Countries
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => new { c.Id, c.Code }, cancellationToken);

        var existingBanks = await _dbContext.Banks
            .ToDictionaryAsync(b => b.Code, b => b, cancellationToken);

        var errors = new List<ImportRowError>();
        int inserted = 0, updated = 0, skipped = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2;
            var rawData = $"{row.Code},{row.Name},{row.CountryCode}";

            try
            {
                if (string.IsNullOrWhiteSpace(row.Code))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = "Code is required." });
                    skipped++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = "Name is required." });
                    skipped++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.CountryCode))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = "CountryCode is required." });
                    skipped++;
                    continue;
                }

                var countryCode = row.CountryCode.Trim().ToUpperInvariant();
                if (!countries.TryGetValue(countryCode, out var country))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = $"Country with code '{countryCode}' not found." });
                    skipped++;
                    continue;
                }

                var code = row.Code.Trim().ToUpperInvariant();

                if (existingBanks.TryGetValue(code, out var existing))
                {
                    existing.UpdateDetails(row.Code, row.Name, country.Code, country.Id);
                    updated++;
                }
                else
                {
                    var bank = Bank.Create(row.Code, row.Name, country.Code, country.Id);
                    _dbContext.Banks.Add(bank);
                    existingBanks[code] = bank;
                    inserted++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = ex.Message });
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
        public string? CountryCode { get; set; }
    }
}
