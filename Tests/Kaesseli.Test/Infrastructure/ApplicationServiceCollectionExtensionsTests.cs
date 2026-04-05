using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Infrastructure;

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
        var timeProvider = serviceProvider.GetService<TimeProvider>();

        //Assert
        timeProvider.ShouldNotBeNull();
    }
}
