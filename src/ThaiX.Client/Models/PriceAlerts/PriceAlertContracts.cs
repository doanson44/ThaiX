namespace ThaiX.Client.Models.PriceAlerts;

/// <summary>
/// DTO for the price alert list item in the admin grid.
/// </summary>
public sealed class PriceAlertListItemDto
{
    public Guid Id { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string AssetType { get; init; } = string.Empty;
    public string Condition { get; init; } = string.Empty;
    public decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsOneTime { get; init; }
    public DateTime? LastTriggeredAt { get; init; }
    public int TriggerCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Request parameters for fetching the price alerts list.
/// </summary>
public sealed class PriceAlertsListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// Request body for creating a price alert.
/// </summary>
public sealed class CreatePriceAlertRequest
{
    public string Symbol { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public decimal TargetPrice { get; set; }
    public string? Note { get; set; }
    public bool IsOneTime { get; set; }
}

/// <summary>
/// Request body for updating a price alert.
/// </summary>
public sealed class UpdatePriceAlertRequest
{
    public string Condition { get; set; } = string.Empty;
    public decimal TargetPrice { get; set; }
    public string? Note { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// Response from the symbol search endpoint.
/// </summary>
public sealed class SymbolSearchResult
{
    public IReadOnlyList<string> Symbols { get; init; } = [];
    public int TotalCount { get; init; }
}
