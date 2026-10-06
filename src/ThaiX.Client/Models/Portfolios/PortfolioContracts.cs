namespace ThaiX.Client.Models.Portfolios;

public sealed class PortfolioListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string PortfolioType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PortfolioDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string PortfolioType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PortfoliosListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
}

public sealed class CreatePortfolioRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PortfolioType { get; set; } = "Trading";
}

public sealed class UpdatePortfolioRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PortfolioType { get; set; } = "Trading";
}

public sealed record PortfolioExportRequest
{
    public string? SearchTerm { get; init; }
}

public sealed record PortfolioExportFileResult
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] FileContent { get; init; }
}

public sealed record PortfolioImportResultDto
{
    public int TotalRows { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public IReadOnlyList<PortfolioImportRowErrorDto> Errors { get; init; } = [];
}

public sealed record PortfolioImportRowErrorDto
{
    public int RowNumber { get; init; }
    public string? RawData { get; init; }
    public string Error { get; init; } = string.Empty;
}
