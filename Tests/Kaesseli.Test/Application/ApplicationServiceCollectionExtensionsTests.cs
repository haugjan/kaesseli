using Kaesseli.Application.Utility;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application;

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
        dateTimeService.ShouldNotBeNull();
    }
}
