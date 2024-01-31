using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Journal;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBudgetRepositories(this IServiceCollection services) =>
        services.AddScoped<IBudgetRepository, BudgetRepository>()
            .AddScoped<IJournalRepository, JournalRepository>();
}