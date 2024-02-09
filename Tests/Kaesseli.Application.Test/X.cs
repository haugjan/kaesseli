using System.Reflection;
using FluentAssertions;
using Kaesseli.Application.Common;
using MediatR;
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

    [Fact]
    public void AllIRequestHandlersAreRegistered()
    {
        // Arrange
        var assembly =
            Assembly.GetAssembly(
                type:
                typeof(ApplicationServiceCollectionExtensions)); 

        var requestTypes = assembly!
                           .GetTypes()
                           .Where(
                               type => type.GetInterfaces()
                                           .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

        foreach (var requestType in requestTypes)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(
                requestType,
                requestType.GetInterfaces()
                           .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                           .GetGenericArguments()
                           .First());

            // Act
            assembly.GetTypes().Should().Contain(t => handlerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        }
    }
}