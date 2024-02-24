using System.Collections.Immutable;
using System.Reflection;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_CanResolveAllMediatRHandlers()
    {
        // Arrange

        var configMock = new Mock<IConfiguration>();
        var configSectionMock = new Mock<IConfigurationSection>();
        configMock.Setup(cfg => cfg.GetSection(It.IsAny<string>()))
                  .Returns((string _) => configSectionMock.Object);
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddApplicationServices()
            .AddInfrastructureServices(configMock.Object);

        var assembly = Assembly.GetAssembly(type: typeof(ApplicationServiceCollectionExtensions));
        var handlerTypes = assembly!.GetTypes()
                                    .Where(
                                        t => t.GetInterfaces()
                                              .Any(
                                                  i => i.IsGenericType
                                                    && (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                                                     || i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))))
                                    .ToImmutableList();

        foreach (var handlerType in handlerTypes) serviceCollection.AddTransient(handlerType);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        foreach (var handlerType in handlerTypes)
        {
            // Act
            var handler = serviceProvider.GetService(handlerType);

            // Assert
            handler.Should().NotBeNull(because: $"weil {handlerType.Name} im DI Container registriert und auflösbar sein sollte.");
        }
    }
}