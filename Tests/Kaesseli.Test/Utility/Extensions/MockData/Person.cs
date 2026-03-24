namespace Kaesseli.Test.Utility.Extensions.MockData;

public class Person
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Address Address { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
