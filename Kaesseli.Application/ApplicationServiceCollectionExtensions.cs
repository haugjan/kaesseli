using Kaesseli.Application.Utility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services.AddTransient<IDateTimeService, DateTimeService>()
                .AddTransient<IEnvironmentService, EnvironmentService>()
                .AddMediatR(
                    config =>
                        config.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));
}