using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Portfolios.Queries.ExportPortfolios;

public sealed class ExportPortfoliosQueryHandler : IRequestHandler<ExportPortfoliosQuery, ExportPortfoliosResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ExportPortfoliosQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExportPortfoliosResult> Handle(ExportPortfoliosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Portfolios
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.OwnerId == _currentUser.UserId);

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(x => EF.Functions.Like(EF.Functions.Collate(x.Name, collation), pattern, "\\"));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var memoryStream = new MemoryStream();
        memoryStream.Write([0xEF, 0xBB, 0xBF]);

        await using var writer = new StreamWriter(memoryStream, leaveOpen: true);
        await using var csv = new CsvWriter(writer, csvConfig);

        csv.WriteField("Name");
        csv.WriteField("PortfolioType");
        csv.WriteField("Description");
        csv.WriteField("CreatedAt");
        csv.WriteField("UpdatedAt");
        await csv.NextRecordAsync();

        foreach (var item in items)
        {
            csv.WriteField(item.Name);
            csv.WriteField(item.PortfolioType.ToString());
            csv.WriteField(item.Description);
            csv.WriteField(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            csv.WriteField(item.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync(cancellationToken);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        return new ExportPortfoliosResult
        {
            FileContent = memoryStream.ToArray(),
            FileName = $"portfolios_export_{timestamp}.csv"
        };
    }
}
