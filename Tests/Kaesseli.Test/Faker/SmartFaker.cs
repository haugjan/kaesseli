using System.Reflection;
using Bogus;

namespace Kaesseli.TestUtilities.Faker;

public sealed class SmartFaker<T> : Faker<T> where T : class
{
    public SmartFaker() =>
        Rules(
            (faker, obj) =>
            {
                foreach (var prop in typeof(T).GetProperties())
                    SetPropertyIfCanWrite(prop, faker, obj);
            });

    private static void SetPropertyIfCanWrite(PropertyInfo prop, Bogus.Faker faker, T obj)
    {
        if (!prop.CanWrite) return;

        var value = GetValue(prop, faker);

        if (value != null) prop.SetValue(obj, value);
    }

    private static object? GetValue(PropertyInfo prop, Bogus.Faker faker)
    {
        var value = prop.PropertyType switch
        {
            { } type when type == typeof(string) => faker.Lorem.Sentence(),
            { } type when type == typeof(int) => faker.Random.Int(),
            { } type when type == typeof(double) => faker.Random.Double(),
            { } type when type == typeof(float) => faker.Random.Float(),
            { } type when type == typeof(decimal) => faker.Random.Decimal(),
            { } type when type == typeof(long) => faker.Random.Long(),
            { } type when type == typeof(bool) => faker.Random.Bool(),
            { } type when type == typeof(DateTime) => faker.Date.Recent(),
            { } type when type == typeof(Guid) => LongToGuid(value: faker.Random.Long()),
            { } type when type == typeof(DateOnly) =>
                faker.Date.BetweenDateOnly(
                    start: new DateOnly(year: 2000, month: 01, day: 01),
                    end: new DateOnly(year: 2010, month: 01, day: 01)),
            { } type when type == typeof(DateOnly?) =>
                faker.Date.BetweenDateOnly(
                    start: new DateOnly(year: 2000, month: 01, day: 01),
                    end: new DateOnly(year: 2010, month: 01, day: 01)),
            { } type when type == typeof(TimeOnly) => TimeOnly.FromTimeSpan(timeSpan: faker.Date.Timespan()),
            { } type when type == typeof(byte) => faker.Random.Byte(),
            { } type when type == typeof(sbyte) => faker.Random.SByte(),
            { } type when type == typeof(short) => faker.Random.Short(),
            { } type when type == typeof(ushort) => faker.Random.UShort(),
            { } type when type == typeof(uint) => faker.Random.UInt(),
            { } type when type == typeof(ulong) => faker.Random.ULong(),
            { } type when type == typeof(char) => faker.Random.Char(),
            { IsEnum: true } type => faker.PickRandom(items: type.GetEnumValues().Cast<int>()),
            { } type when type == typeof(List<string>) => faker.Lorem.Words(),
            { } type when type == typeof(Dictionary<string, string>) => faker.Make(
                                                                                 count: faker.Random.Int(min: 1, max: 5),
                                                                                 _ => new KeyValuePair<string, string>(
                                                                                     key: faker.Lorem.Word(),
                                                                                     value: faker.Lorem.Word()))
                                                                             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            { IsPrimitive: false } type when type != typeof(string) && !type.IsEnum && type.GetConstructor(Type.EmptyTypes) != null =>
                Activator.CreateInstance(type) switch { null => null, var instance => PopulateComplexType(instance, faker) },
            _ => null
        };
        return value;
    }

    private static object PopulateComplexType(object instance, Bogus.Faker faker)
    {
        foreach (var prop in instance.GetType().GetProperties())
        {
            if (prop.SetMethod != null && (!prop.CanWrite || prop.SetMethod.IsPublic == false)) continue;

            var propValue = GetValue(prop, faker);
            if (propValue != null) prop.SetValue(instance, propValue);
        }

        return instance;
    }

    private static Guid LongToGuid(long value)
    {
        var bytes = BitConverter.GetBytes(value);

        var guidBytes = new byte[16];

        Array.Copy(
            bytes,
            sourceIndex: 0,
            guidBytes,
            destinationIndex: 0,
            bytes.Length);

        Array.Copy(
            bytes,
            sourceIndex: 0,
            guidBytes,
            destinationIndex: 8,
            bytes.Length);

        return new Guid(guidBytes);
    }
}
