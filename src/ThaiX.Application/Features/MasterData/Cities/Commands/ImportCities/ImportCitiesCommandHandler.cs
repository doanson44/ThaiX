using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.ImportCities;

/// <summary>
/// Handler for ImportCitiesCommand.
/// Upserts cities from CSV: insert new, update existing (matched by Code).
/// </summary>
public sealed class ImportCitiesCommandHandler : IRequestHandler<ImportCitiesCommand, ImportResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ImportCitiesCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImportResult> Handle(ImportCitiesCommand request, CancellationToken cancellationToken)
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

        var existingCities = await _dbContext.Cities
            .ToDictionaryAsync(c => c.Code, c => c, cancellationToken);

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

                if (existingCities.TryGetValue(code, out var existing))
                {
                    existing.UpdateDetails(row.Code, row.Name, country.Code, country.Id);
                    updated++;
                }
                else
                {
                    var city = City.Create(row.Code, row.Name, country.Code, country.Id);
                    _dbContext.Cities.Add(city);
                    existingCities[code] = city;
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
