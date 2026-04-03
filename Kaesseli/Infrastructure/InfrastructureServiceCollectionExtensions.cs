using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Accounts;
using Kaesseli.Infrastructure.Automation;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Common;
using Kaesseli.Infrastructure.Integration;
using Kaesseli.Infrastructure.Journal;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .AddRepositories()
            .AddScoped<ICamtProcessor, CamtProcessor>()
            .AddScoped<IPostFinanceCsvProcessor, PostFinanceCsvProcessor>()
            .AddDbContext<KaesseliContext>(options =>
            {
                var endpoint =
                    configuration["CosmosDb:Endpoint"]
                    ?? throw new InvalidOperationException("CosmosDb:Endpoint is not configured.");
                var key =
                    configuration["CosmosDb:Key"]
                    ?? throw new InvalidOperationException("CosmosDb:Key is not configured.");
                var database =
                    configuration["CosmosDb:Database"]
                    ?? throw new InvalidOperationException("CosmosDb:Database is not configured.");
                var isLocalEmulator = endpoint.Contains(
                    "localhost",
                    StringComparison.OrdinalIgnoreCase
                );

                options.UseCosmos(
                    accountEndpoint: endpoint,
                    accountKey: key,
                    databaseName: database,
                    cosmosOptionsAction: isLocalEmulator
                        ? cosmos =>
                        {
                            cosmos.ConnectionMode(ConnectionMode.Gateway);
                            cosmos.HttpClientFactory(() =>
                            {
                                var handler = new HttpClientHandler
                                {
                                    ServerCertificateCustomValidationCallback =
                                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                                };
                                return new HttpClient(handler);
                            });
                        }
                        : null
                );
            });

    private static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IBudgetRepository, BudgetRepository>()
            .AddScoped<IJournalRepository, JournalRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IAutomationRepository, AutomationRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>();

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KaesseliContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
