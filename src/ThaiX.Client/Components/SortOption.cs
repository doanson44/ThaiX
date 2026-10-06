namespace ThaiX.Client.Components;

/// <summary>
/// Defines a sortable field for use with <see cref="ClientSortBar{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The item type to sort.</typeparam>
public sealed record SortOption<TItem>(
    string Key,
    string Label,
    Func<TItem, IComparable?> KeySelector);
