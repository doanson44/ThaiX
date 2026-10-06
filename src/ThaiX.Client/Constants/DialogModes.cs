namespace ThaiX.Client.Constants;

/// <summary>
/// Constants for form/dialog display modes. Reusable across list pages (Contacts, etc.)
/// when using a single dialog for View (read-only) and Edit.
/// </summary>
public static class DialogModes
{
    /// <summary>Dialog is closed / no dialog shown.</summary>
    public const string None = "None";

    /// <summary>Dialog in create mode.</summary>
    public const string Create = "Create";

    /// <summary>Dialog in read-only view mode.</summary>
    public const string View = "View";

    /// <summary>Dialog in edit mode.</summary>
    public const string Edit = "Edit";
}
