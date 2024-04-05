using FluentAssertions;
using Kaesseli.Application.Utility;
using Xunit;

namespace Kaesseli.Application.Test.Utility;

public class EnvironmentServiceTests
{
    [Fact]
    public void CurrentUser_ReturnsCurrentUser()
    {
        //Arrange
        var service = new EnvironmentService();

        //Act
        var currentUser=service.CurrentUser;
        var expectedUser = Environment.UserName;

        //Assert
        currentUser.Should().Be(expectedUser);
    }
}