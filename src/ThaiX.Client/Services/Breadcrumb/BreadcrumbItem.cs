namespace ThaiX.Client.Services.Breadcrumb;

/// <summary>
/// Represents a single breadcrumb segment. Path is null for the current (last) page.
/// </summary>
public sealed record BreadcrumbItem(string Text, string? Path = null);
