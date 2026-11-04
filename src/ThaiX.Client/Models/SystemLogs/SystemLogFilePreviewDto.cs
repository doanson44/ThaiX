namespace ThaiX.Client.Models.SystemLogs;

public sealed record SystemLogFilePreviewDto(
    string FileName,
    string Content,
    bool IsTruncated);
