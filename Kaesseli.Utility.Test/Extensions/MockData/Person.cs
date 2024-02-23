namespace Kaesseli.Utility.Test.Extensions.MockData;

public class Person
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Address Address { get; init; }
}