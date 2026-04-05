using System.Reflection;
using Bogus;

namespace Kaesseli.Test.Faker;

public sealed class SmartFaker<T> : Faker<T>
    where T : class
{
    public SmartFaker()
    {
        var constructors = typeof(T).GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var parameterlessCtor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
        if (parameterlessCtor != null && !parameterlessCtor.IsPublic)
        {
            // Private/protected parameterless constructor - use reflection to invoke it
            CustomInstantiator(_ => (T)parameterlessCtor.Invoke(null));
        }
        else if (parameterlessCtor == null)
        {
            var ctor = constructors[0];
            CustomInstantiator(f =>
            {
                var parameters = ctor.GetParameters()
                    .Select(p => GetValueByType(p.ParameterType, f))
                    .ToArray();
                return (T)ctor.Invoke(parameters);
            });
        }

        Rules(
            (faker, obj) =>
            {
                foreach (var prop in typeof(T).GetProperties())
                    SetPropertyIfCanWrite(prop, faker, obj);
            }
        );
    }

    private static void SetPropertyIfCanWrite(PropertyInfo prop, Bogus.Faker faker, T obj)
    {
        if (prop.SetMethod == null)
            return;

        var value = GetValueByType(prop.PropertyType, faker);

        if (value != null)
            prop.SetValue(obj, value);
    }

    private static object? GetValueByType(Type type, Bogus.Faker faker)
    {
        var value = type switch
        {
            { } t when t == typeof(string) => faker.Lorem.Sentence(),
            { } t when t == typeof(int) => faker.Random.Int(),
            { } t when t == typeof(double) => faker.Random.Double(),
            { } t when t == typeof(float) => faker.Random.Float(),
            { } t when t == typeof(decimal) => faker.Random.Decimal(),
            { } t when t == typeof(long) => faker.Random.Long(),
            { } t when t == typeof(bool) => faker.Random.Bool(),
            { } t when t == typeof(DateTime) => faker.Date.Recent(),
            { } t when t == typeof(Guid) => LongToGuid(value: faker.Random.Long()),
            { } t when t == typeof(DateOnly) => faker.Date.BetweenDateOnly(
                start: new DateOnly(year: 2000, month: 01, day: 01),
                end: new DateOnly(year: 2010, month: 01, day: 01)
            ),
            { } t when t == typeof(DateOnly?) => faker.Date.BetweenDateOnly(
                start: new DateOnly(year: 2000, month: 01, day: 01),
                end: new DateOnly(year: 2010, month: 01, day: 01)
            ),
            { } t when t == typeof(TimeOnly) => TimeOnly.FromTimeSpan(
                timeSpan: faker.Date.Timespan()
            ),
            { } t when t == typeof(byte) => faker.Random.Byte(),
            { } t when t == typeof(sbyte) => faker.Random.SByte(),
            { } t when t == typeof(short) => faker.Random.Short(),
            { } t when t == typeof(ushort) => faker.Random.UShort(),
            { } t when t == typeof(uint) => faker.Random.UInt(),
            { } t when t == typeof(ulong) => faker.Random.ULong(),
            { } t when t == typeof(char) => faker.Random.Char(),
            { IsEnum: true } t => faker.PickRandom(t.GetEnumValues().Cast<object>().ToArray()),
            { } t when t == typeof(List<string>) => faker.Lorem.Words(),
            { } t when t == typeof(Dictionary<string, string>) => faker
                .Make(
                    count: faker.Random.Int(min: 1, max: 5),
                    _ => new KeyValuePair<string, string>(
                        key: faker.Lorem.Word(),
                        value: faker.Lorem.Word()
                    )
                )
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            { IsPrimitive: false, IsAbstract: false } t
                when t != typeof(string)
                    && !t.IsEnum
                    && t.GetConstructor(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null, Type.EmptyTypes, null) != null => Activator.CreateInstance(
                t, nonPublic: true
            ) switch
            {
                null => null,
                var instance => PopulateComplexType(instance, faker),
            },
            { IsPrimitive: false, IsAbstract: false } t
                when t != typeof(string)
                    && !t.IsEnum
                    && t.GetConstructor(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null, Type.EmptyTypes, null) == null => TryInvokeFirstConstructor(t, faker),
            _ => null,
        };
        return value;
    }

    private static object PopulateComplexType(object instance, Bogus.Faker faker)
    {
        foreach (var prop in instance.GetType().GetProperties())
        {
            if (prop.SetMethod == null)
                continue;

            var propValue = GetValueByType(prop.PropertyType, faker);
            if (propValue != null)
                prop.SetValue(instance, propValue);
        }

        return instance;
    }

    private static object? TryInvokeFirstConstructor(Type type, Bogus.Faker faker)
    {
        var ctor = type.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
        if (ctor == null) return null;
        var parameters = ctor.GetParameters()
            .Select(p => GetValueByType(p.ParameterType, faker))
            .ToArray();
        try { return ctor.Invoke(parameters); }
        catch { return null; }
    }

    private static Guid LongToGuid(long value)
    {
        var bytes = BitConverter.GetBytes(value);

        var guidBytes = new byte[16];

        Array.Copy(bytes, sourceIndex: 0, guidBytes, destinationIndex: 0, bytes.Length);

        Array.Copy(bytes, sourceIndex: 0, guidBytes, destinationIndex: 8, bytes.Length);

        return new Guid(guidBytes);
    }
}
