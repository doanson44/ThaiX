namespace ThaiX.Application.Common.Caching;

/// <summary>
/// Declares which cache groups should be invalidated after the command handler runs.
/// Multiple attributes are supported.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class InvalidateCacheAttribute : Attribute
{
    public string Group { get; }

    public InvalidateCacheAttribute(string group)
    {
        Group = group;
    }
}
