using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Trading.Common;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class PersistWeeklySuggestionReportJob
{
    private const int RetentionWeeks = 10;
    private const int DaysPerWeek = 7;

    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<PersistWeeklySuggestionReportJob> _logger;

    public PersistWeeklySuggestionReportJob(
        IApplicationDbContext dbContext,
        IFileStorage fileStorage,
        ILogger<PersistWeeklySuggestionReportJob> logger)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task RunAsync(WeeklySuggestionReportPersistPayload payload, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.WeeklySuggestionReports
            .AsNoTracking()
            .AnyAsync(x => x.ReportKey == payload.ReportKey, cancellationToken);

        if (exists)
        {
            await PersistTemplateFilesAsync(payload, cancellationToken);
            await CleanupOldReportsAsync(cancellationToken);
            _logger.LogInformation(
                "PersistWeeklySuggestionReportJob skipped because report already exists: {ReportKey}",
                payload.ReportKey);
            return;
        }

        var report = WeeklySuggestionReport.Create(
            runAtUtc: payload.RunAtUtc,
            elapsedSeconds: payload.ElapsedSeconds,
            candidateScanLimit: payload.CandidateScanLimit,
            topCount: payload.TopCount,
            assetClass: payload.AssetClass,
            reportType: payload.ReportType,
            status: payload.Status,
            reportKey: payload.ReportKey);

        foreach (var item in payload.Items)
        {
            report.AddItem(
                timeframe: item.Timeframe,
                rank: item.Rank,
                symbol: item.Symbol,
                marketType: item.MarketType,
                compositeScore: item.CompositeScore,
                signal: item.Signal,
                confidence: item.Confidence,
                entryPrice: item.EntryPrice,
                stopLoss: item.StopLoss,
                takeProfit1: item.TakeProfit1,
                takeProfit2: item.TakeProfit2);
        }

        _dbContext.WeeklySuggestionReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await PersistTemplateFilesAsync(payload, cancellationToken);
        await CleanupOldReportsAsync(cancellationToken);

        _logger.LogInformation(
            "PersistWeeklySuggestionReportJob saved report {ReportKey} with {ItemCount} items",
            payload.ReportKey,
            payload.Items.Count);
    }

    private async Task PersistTemplateFilesAsync(
        WeeklySuggestionReportPersistPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = new WeeklySuggestionTemplateContext(
                ReportKey: payload.ReportKey,
                RunAtUtc: payload.RunAtUtc,
                AssetClass: payload.AssetClass,
                Status: payload.Status,
                Items: payload.Items
                    .OrderBy(x => x.Timeframe)
                    .ThenBy(x => x.Rank)
                    .Select(x => new WeeklySuggestionTemplateItem(
                        Timeframe: x.Timeframe,
                        Rank: x.Rank,
                        Symbol: x.Symbol,
                        MarketType: x.MarketType,
                        CompositeScore: x.CompositeScore,
                        Signal: x.Signal,
                        Confidence: x.Confidence,
                        EntryPrice: x.EntryPrice,
                        StopLoss: x.StopLoss,
                        TakeProfit1: x.TakeProfit1,
                        TakeProfit2: x.TakeProfit2))
                    .ToList());

            var csvKey = WeeklySuggestionReportTemplateStorage.BuildCsvStorageKey(payload.ReportKey, payload.AssetClass);
            var markdownKey = WeeklySuggestionReportTemplateStorage.BuildMarkdownStorageKey(payload.ReportKey, payload.AssetClass);

            var csvBytes = WeeklySuggestionReportTemplateRenderer.RenderCsv(context);
            var markdownBytes = WeeklySuggestionReportTemplateRenderer.RenderMarkdown(context);

            await SaveTemplateFileAsync(csvKey, "text/csv", csvBytes, cancellationToken);
            await SaveTemplateFileAsync(markdownKey, "text/markdown", markdownBytes, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "PersistWeeklySuggestionReportJob failed to save template files for report {ReportKey}",
                payload.ReportKey);
        }
    }

    private async Task SaveTemplateFileAsync(
        string storageKey,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(content, writable: false);
        await _fileStorage.SaveAsync(stream, storageKey, contentType, cancellationToken);
    }

    private async Task CleanupOldReportsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cutoffUtc = DateTime.UtcNow.AddDays(-(RetentionWeeks * DaysPerWeek));

            var oldReports = await _dbContext.WeeklySuggestionReports
                .AsNoTracking()
                .Where(x => x.RunAtUtc < cutoffUtc)
                .Select(x => new
                {
                    x.ReportKey,
                    x.AssetClass
                })
                .ToListAsync(cancellationToken);

            if (oldReports.Count == 0)
            {
                return;
            }

            var deletedReportsCount = await _dbContext.WeeklySuggestionReports
                .Where(x => x.RunAtUtc < cutoffUtc)
                .ExecuteDeleteAsync(cancellationToken);

            var deletedTemplateFilesCount = 0;
            var failedTemplateFileDeletesCount = 0;

            foreach (var oldReport in oldReports)
            {
                var (deletedCount, failedCount) = await DeleteTemplateFilesForReportAsync(
                    oldReport.ReportKey,
                    oldReport.AssetClass,
                    cancellationToken);

                deletedTemplateFilesCount += deletedCount;
                failedTemplateFileDeletesCount += failedCount;
            }

            _logger.LogInformation(
                "PersistWeeklySuggestionReportJob cleanup completed. Deleted {DeletedReportsCount} reports older than {RetentionWeeks} weeks (cutoff: {CutoffUtc:o}). Template files deleted: {DeletedTemplateFilesCount}, delete failures: {FailedTemplateFileDeletesCount}",
                deletedReportsCount,
                RetentionWeeks,
                cutoffUtc,
                deletedTemplateFilesCount,
                failedTemplateFileDeletesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PersistWeeklySuggestionReportJob cleanup failed");
        }
    }

    private async Task<(int deletedCount, int failedCount)> DeleteTemplateFilesForReportAsync(
        string reportKey,
        SuggestionAssetClass assetClass,
        CancellationToken cancellationToken)
    {
        var keys = new[]
        {
            WeeklySuggestionReportTemplateStorage.BuildCsvStorageKey(reportKey, assetClass),
            WeeklySuggestionReportTemplateStorage.BuildMarkdownStorageKey(reportKey, assetClass)
        };

        var deletedCount = 0;
        var failedCount = 0;

        foreach (var key in keys)
        {
            try
            {
                await _fileStorage.DeleteAsync(key, cancellationToken);
                deletedCount++;
            }
            catch (Exception ex)
            {
                failedCount++;
                _logger.LogWarning(
                    ex,
                    "PersistWeeklySuggestionReportJob failed to delete template file {StorageKey} for report {ReportKey}",
                    key,
                    reportKey);
            }
        }

        return (deletedCount, failedCount);
    }
}

public sealed record WeeklySuggestionReportPersistPayload(
    string ReportKey,
    DateTime RunAtUtc,
    int ElapsedSeconds,
    int CandidateScanLimit,
    int TopCount,
    SuggestionAssetClass AssetClass,
    SuggestionReportType ReportType,
    WeeklySuggestionReportStatus Status,
    IReadOnlyList<WeeklySuggestionReportPersistItem> Items);

public sealed record WeeklySuggestionReportPersistItem(
    string Timeframe,
    int Rank,
    string Symbol,
    SuggestionAssetClass MarketType,
    decimal CompositeScore,
    string Signal,
    decimal Confidence,
    decimal EntryPrice,
    decimal? StopLoss,
    decimal? TakeProfit1,
    decimal? TakeProfit2);
