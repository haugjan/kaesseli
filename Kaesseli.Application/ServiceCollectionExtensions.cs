using Kaesseli.Application.Common;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services.AddTransient<IDateTimeService, DateTimeService>()
                .AddMediatR(
                    config =>
                        config.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));
}