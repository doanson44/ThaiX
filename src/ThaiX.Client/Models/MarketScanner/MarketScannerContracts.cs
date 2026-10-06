namespace ThaiX.Client.Models.MarketScanner;

/// <summary>
/// DTO for market scanner rule list item.
/// </summary>
public sealed class MarketScannerRuleListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SignalType { get; init; } = string.Empty;
    public string Window { get; init; } = string.Empty;
    public decimal? Threshold { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdated { get; init; }
}

/// <summary>
/// Request parameters for fetching the market scanner rules list.
/// </summary>
public sealed class MarketScannerRulesListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Request body for creating a market scanner rule.
/// </summary>
public sealed class CreateMarketScannerRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string SignalType { get; set; } = string.Empty;
    public string Window { get; set; } = string.Empty;
    public decimal? Threshold { get; set; }
}

/// <summary>
/// Request body for updating a market scanner rule.
/// </summary>
public sealed class UpdateMarketScannerRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Window { get; set; } = string.Empty;
    public decimal? Threshold { get; set; }
    public bool IsEnabled { get; set; }
}
