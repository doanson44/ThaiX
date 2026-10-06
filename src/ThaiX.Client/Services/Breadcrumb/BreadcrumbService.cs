namespace ThaiX.Client.Services.Breadcrumb;

/// <inheritdoc />
public sealed class BreadcrumbService : IBreadcrumbService
{
    private List<BreadcrumbItem> _items = [];

    /// <inheritdoc />
    public IReadOnlyList<BreadcrumbItem> Items => _items;

    /// <inheritdoc />
    public event Action? OnChanged;

    /// <inheritdoc />
    public void Set(IReadOnlyList<BreadcrumbItem> items)
    {
        _items = items?.ToList() ?? [];
        OnChanged?.Invoke();
    }

    /// <inheritdoc />
    public void Clear()
    {
        _items = [];
        OnChanged?.Invoke();
    }
}
