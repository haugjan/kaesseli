using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

// ReSharper disable once CheckNamespace
namespace System;

public static class ObjectExtensions
{
    public static string ToYaml(this object obj)
    {
        var serializer = new SerializerBuilder()
                         .WithTypeConverter(typeConverter: new DateOnlyYamlConverter())
                         .Build();
        return serializer.Serialize(obj).Replace("\r\n", "\n");
    }

    private class DateOnlyYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) =>
            type == typeof(DateOnly);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            throw new NotImplementedException();

        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is null) return;
            var dateOnly = (DateOnly)value;
            emitter.Emit(@event: new Scalar(value: dateOnly.ToString(format: "yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }
        
    }
}