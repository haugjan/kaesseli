using FluentAssertions;
using Kaesseli.Application.Utility;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kaesseli.Application.Test;

public class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_RegistersAllServices()
    {
        //Arrange
        var serviceCollection = new ServiceCollection();

        //Act
        serviceCollection.AddApplicationServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var dateTimeService = serviceProvider.GetService<IDateTimeService>();

        //Assert
        dateTimeService.Should().NotBe(unexpected: null);
    }
}
