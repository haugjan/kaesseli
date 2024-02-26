using FluentAssertions;
using Kaesseli.Utility.Test.Extensions.MockData;
using Xunit;

namespace Kaesseli.Utility.Test.Extensions;

public class ObjectExtensionsTests
{
    [Fact]
    public void ToYaml_SerializesToYaml()
    {
        //Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(year: 1982, month: 11, day: 3),
            Address = new Address
            {
                Street = "Sample street",
                City = "Sample city",
                Zip = 1234
            }
        };
        const string expected = """
                                FirstName: John
                                LastName: Doe
                                DateOfBirth: 1982-11-03
                                Address:
                                  Street: Sample street
                                  City: Sample city
                                  Zip: 1234
                                
                                """;

        //Act
        var current = person.ToYaml();

        //Assert
        current.Should().Be(expected);

    }
}