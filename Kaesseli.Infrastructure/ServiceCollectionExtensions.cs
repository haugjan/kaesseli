using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Common;
using Kaesseli.Infrastructure.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionss
{
    public static IServiceCollection AddBudgetRepositories(this IServiceCollection services, IConfiguration configuration) =>
        services.AddScoped<IBudgetRepository, BudgetRepository>()
            .AddScoped<IJournalRepository, JournalRepository>()
            .AddDbContext<KaesseliContext>(options =>
                                             options.UseSqlite(connectionString: configuration.GetConnectionString(name: "BudgetDatabase")));

    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<KaesseliContext>();
        context.Database.EnsureCreated();
    }
}