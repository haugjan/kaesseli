
// ReSharper disable once CheckNamespace
namespace System;

public static class TypeExtensions
{
    public static async Task<string> ReadResource(this Type type, string fileName)
    {
        await using var stream = ReadResourceAsStream(type, fileName);
        using var reader = new StreamReader(stream: stream);
        return (await reader.ReadToEndAsync()).Replace("\r\n", "\n");
    }

    public static Stream ReadResourceAsStream(this Type type, string fileName)
    {
        var assembly = type.Assembly;
        var resourceName = $"{type.Namespace}.{fileName}";
        return assembly.GetManifestResourceStream(resourceName)
                              ?? throw new ArgumentException(message: $"Could not find resource '{resourceName}'");

    }
}
