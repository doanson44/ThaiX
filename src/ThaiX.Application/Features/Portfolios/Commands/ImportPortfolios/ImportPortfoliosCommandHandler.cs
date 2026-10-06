using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Commands.ImportPortfolios;

public sealed class ImportPortfoliosCommandHandler : IRequestHandler<ImportPortfoliosCommand, ImportResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ImportPortfoliosCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ImportResult> Handle(ImportPortfoliosCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ImportRowError>();
        var totalRows = 0;
        var inserted = 0;
        var updated = 0;
        var skipped = 0;

        var existingByName = await _context.Portfolios
            .Where(x => !x.IsDeleted && x.OwnerId == _currentUser.UserId)
            .ToDictionaryAsync(x => NormalizeName(x.Name), cancellationToken);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        using var csv = new CsvReader(reader, csvConfig);

        if (!await csv.ReadAsync())
        {
            return new ImportResult
            {
                TotalRows = 0,
                InsertedCount = 0,
                UpdatedCount = 0,
                SkippedCount = 0,
                Errors = [new ImportRowError { RowNumber = 0, Error = "CSV file is empty or missing header." }]
            };
        }

        csv.ReadHeader();

        var rowNumber = 1;
        while (await csv.ReadAsync())
        {
            rowNumber++;
            totalRows++;

            var name = (csv.GetField("Name") ?? string.Empty).Trim();
            var typeRaw = (csv.GetField("PortfolioType") ?? string.Empty).Trim();
            var description = (csv.GetField("Description") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                skipped++;
                errors.Add(new ImportRowError
                {
                    RowNumber = rowNumber,
                    RawData = BuildRawData(name, typeRaw, description),
                    Error = "Name is required."
                });
                continue;
            }

            if (!TryParsePortfolioType(typeRaw, out var portfolioType))
            {
                skipped++;
                errors.Add(new ImportRowError
                {
                    RowNumber = rowNumber,
                    RawData = BuildRawData(name, typeRaw, description),
                    Error = "PortfolioType is invalid. Allowed values: Trading, LongTerm, Retirement, Savings."
                });
                continue;
            }

            var normalizedName = NormalizeName(name);
            if (existingByName.TryGetValue(normalizedName, out var existing))
            {
                existing.Update(name, portfolioType, description);
                updated++;
                continue;
            }

            var portfolio = Portfolio.Create(_currentUser.UserId, name, portfolioType, description);
            _context.Portfolios.Add(portfolio);
            existingByName[normalizedName] = portfolio;
            inserted++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ImportResult
        {
            TotalRows = totalRows,
            InsertedCount = inserted,
            UpdatedCount = updated,
            SkippedCount = skipped,
            Errors = errors
        };
    }

    private static string NormalizeName(string value) => value.Trim().ToLowerInvariant();

    private static string BuildRawData(string? name, string? typeRaw, string? description)
        => string.Join('|', [name ?? string.Empty, typeRaw ?? string.Empty, description ?? string.Empty]);

    private static bool TryParsePortfolioType(string input, out PortfolioType portfolioType)
    {
        if (Enum.TryParse(input, true, out portfolioType))
        {
            return Enum.IsDefined(portfolioType);
        }

        var normalized = input.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (normalized.Equals("LongTerm", StringComparison.OrdinalIgnoreCase))
        {
            portfolioType = PortfolioType.LongTerm;
            return true;
        }

        return false;
    }
}
