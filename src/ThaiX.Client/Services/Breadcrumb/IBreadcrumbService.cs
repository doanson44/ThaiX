namespace ThaiX.Client.Services.Breadcrumb;

/// <summary>
/// Service for the current page breadcrumb trail. Layout reads items and renders RadzenBreadCrumb.
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Current breadcrumb items (path from root to current page). Empty when no breadcrumb.
    /// </summary>
    IReadOnlyList<BreadcrumbItem> Items { get; }

    /// <summary>
    /// Raised when Items change so the layout can re-render.
    /// </summary>
    event Action? OnChanged;

    /// <summary>
    /// Set the breadcrumb trail for the current page. Call from page OnInitialized/SetParameters.
    /// </summary>
    void Set(IReadOnlyList<BreadcrumbItem> items);

    /// <summary>
    /// Clear breadcrumb (e.g. before building default from route).
    /// </summary>
    void Clear();
}
