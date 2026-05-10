using Kaesseli.Features.AccountSuggestion;
using Kaesseli.Features.AccountSuggestion.Gemini;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Journal;
using Kaesseli.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IServiceCollection AddInfrastructureServices(
            IConfiguration configuration
        ) =>
            services
                .AddRepositories()
                .AddGemini(configuration)
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
                            : cosmos =>
                            {
                                cosmos.Region("Switzerland North");
                            }
                    );
                });
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IBudgetRepository, BudgetRepository>()
            .AddScoped<IJournalRepository, JournalRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IAutomationRepository, AutomationRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<IAccountSuggestionRepository, AccountSuggestionRepository>();

}
