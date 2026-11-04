namespace ThaiX.Client.Models.SystemLogs;

public sealed record SystemLogFileDto(
    string FileName,
    DateOnly Date,
    long SizeBytes,
    DateTime LastModifiedUtc);
