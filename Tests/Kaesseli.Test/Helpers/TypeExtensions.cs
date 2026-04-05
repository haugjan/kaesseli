
// ReSharper disable once CheckNamespace
namespace System;

public static class TypeExtensions
{
    extension(Type type)
    {
        public async Task<string> ReadResource(string fileName)
        {
            await using var stream = type.ReadResourceAsStream(fileName);
            using var reader = new StreamReader(stream: stream);
            return (await reader.ReadToEndAsync()).Replace("\r\n", "\n");
        }

        public Stream ReadResourceAsStream(string fileName)
        {
            var assembly = type.Assembly;
            var resourceName = $"{type.Namespace}.{fileName}";
            return assembly.GetManifestResourceStream(resourceName)
                                  ?? throw new ArgumentException(message: $"Could not find resource '{resourceName}'");

        }
    }
}
