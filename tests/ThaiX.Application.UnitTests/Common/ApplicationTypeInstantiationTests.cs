using System.Reflection;

namespace ThaiX.Application.UnitTests.Common;

public sealed class ApplicationTypeInstantiationTests
{
    [Fact]
    public void SimpleApplicationModels_ShouldConstructAndExposeProperties()
    {
        var assembly = typeof(DependencyInjection).Assembly;
        var failures = new List<string>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type.IsEnum || type.IsGenericType || type.IsNested)
                continue;

            if (type.Namespace is null || !type.Namespace.StartsWith("ThaiX.Application", StringComparison.Ordinal))
                continue;

            if (type.Name.EndsWith("Handler", StringComparison.Ordinal)
                || type.Name.EndsWith("Validator", StringComparison.Ordinal)
                || type.Name.EndsWith("Behavior", StringComparison.Ordinal)
                || type.Name.EndsWith("Service", StringComparison.Ordinal)
                || type.Name.EndsWith("Factory", StringComparison.Ordinal)
                || type.Name.EndsWith("Provider", StringComparison.Ordinal)
                || type.Name.EndsWith("Attribute", StringComparison.Ordinal)
                || type.Name.EndsWith("Interface", StringComparison.Ordinal))
            {
                continue;
            }

            var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (ctor is null)
                continue;

            var parameters = ctor.GetParameters();
            if (parameters.Any(p => p.ParameterType.IsInterface || p.ParameterType.IsAbstract))
                continue;

            try
            {
                var args = parameters.Select(p => GetDefaultValue(p.ParameterType)).ToArray();
                var instance = ctor.Invoke(args);

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanRead && p.GetMethod?.IsPublic == true))
                {
                    _ = property.GetValue(instance);
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{type.FullName}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        failures.Should().BeEmpty($"Failed to instantiate {failures.Count} types: {string.Join("; ", failures.Take(10))}");
    }

    private static object? GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return "default";

        if (type == typeof(Guid))
            return Guid.NewGuid();

        if (type == typeof(DateTime))
            return DateTime.UtcNow;

        if (type == typeof(decimal))
            return 1m;

        if (type == typeof(int))
            return 1;

        if (type == typeof(long))
            return 1L;

        if (type == typeof(bool))
            return false;

        if (type == typeof(TimeSpan))
            return TimeSpan.Zero;

        if (type.IsEnum)
            return Enum.GetValues(type).GetValue(0)!;

        if (type.IsArray)
            return Array.CreateInstance(type.GetElementType()!, 0);

        if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition();
            var elementType = type.GetGenericArguments()[0];

            if (generic == typeof(List<>))
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            if (generic == typeof(IEnumerable<>) || generic == typeof(IReadOnlyCollection<>) || generic == typeof(IReadOnlyList<>))
                return Array.CreateInstance(elementType, 0);

            if (generic == typeof(Dictionary<,>))
                return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments()));
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
