namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Value object representing a person's full name.
/// Configured as an EF Core owned type on the Contact table.
/// </summary>
public sealed class FullName
{
    /// <summary>
    /// First (given) name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Last (family) name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    // Private parameterless constructor for EF Core
    private FullName() { }

    public FullName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}
