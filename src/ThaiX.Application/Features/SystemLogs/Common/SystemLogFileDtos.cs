namespace ThaiX.Application.Features.SystemLogs.Common;

public sealed record SystemLogFileDto(
    string FileName,
    DateOnly Date,
    long SizeBytes,
    DateTime LastModifiedUtc);

public sealed record SystemLogFilePreviewDto(
    string FileName,
    string Content,
    bool IsTruncated);

public sealed record SystemLogFileDownloadDto(
    string FileName,
    Stream Content);
