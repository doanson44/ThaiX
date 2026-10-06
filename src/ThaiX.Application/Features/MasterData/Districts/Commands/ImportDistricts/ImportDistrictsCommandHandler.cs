using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.ImportDistricts;

/// <summary>
/// Handler for ImportDistrictsCommand.
/// Upserts districts from CSV: insert new, update existing (matched by Code).
/// </summary>
public sealed class ImportDistrictsCommandHandler : IRequestHandler<ImportDistrictsCommand, ImportResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ImportDistrictsCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImportResult> Handle(ImportDistrictsCommand request, CancellationToken cancellationToken)
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
        var cities = await _dbContext.Cities
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => new { c.Id, c.Code }, cancellationToken);

        var existingDistricts = await _dbContext.Districts
            .ToDictionaryAsync(d => d.Code, d => d, cancellationToken);

        var errors = new List<ImportRowError>();
        int inserted = 0, updated = 0, skipped = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2;
            var rawData = $"{row.Code},{row.Name},{row.CityCode}";

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

                if (string.IsNullOrWhiteSpace(row.CityCode))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = "CityCode is required." });
                    skipped++;
                    continue;
                }

                var cityCode = row.CityCode.Trim().ToUpperInvariant();
                if (!cities.TryGetValue(cityCode, out var city))
                {
                    errors.Add(new ImportRowError { RowNumber = rowNumber, RawData = rawData, Error = $"City with code '{cityCode}' not found." });
                    skipped++;
                    continue;
                }

                var code = row.Code.Trim().ToUpperInvariant();

                if (existingDistricts.TryGetValue(code, out var existing))
                {
                    existing.UpdateDetails(row.Code, row.Name, city.Code, city.Id);
                    updated++;
                }
                else
                {
                    var district = District.Create(row.Code, row.Name, city.Code, city.Id);
                    _dbContext.Districts.Add(district);
                    existingDistricts[code] = district;
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
        public string? CityCode { get; set; }
    }
}
