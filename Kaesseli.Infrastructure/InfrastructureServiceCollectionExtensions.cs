using Kaesseli.Application.Integration.Camt;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Accounts;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Common;
using Kaesseli.Infrastructure.Integration;
using Kaesseli.Infrastructure.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration) =>
        services.AddRepositories()
                .AddScoped<ICamtProcessor, CamtProcessor>()
                .AddDbContext<KaesseliContext>(
                    options =>
                        options.UseSqlServer(connectionString: configuration.GetConnectionString(name: "BudgetDatabase")  ));

    private static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IBudgetRepository, BudgetRepository>()
                .AddScoped<IJournalRepository, JournalRepository>()
                .AddScoped<IAccountRepository, AccountRepository>()
                .AddScoped<ITransactionRepository, TransactionRepository>();

    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<KaesseliContext>();
        context.Database.EnsureCreated();
    }
}