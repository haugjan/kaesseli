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

    public static async Task SeedDevelopmentDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KaesseliContext>();

        var existingAccounts = await context.Accounts.ToListAsync();
        if (existingAccounts.Count > 0)
        {
            Console.WriteLine("Development data already exists, skipping seed.");
            return;
        }

        var currentYear = DateTime.UtcNow.Year;

        // Accounting Period
        var period = new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            Description = $"Geschäftsjahr {currentYear}",
            FromInclusive = new DateOnly(currentYear, 1, 1),
            ToInclusive = new DateOnly(currentYear, 12, 31),
        };
        context.AccountingPeriods.Add(period);

        // Accounts
        var bankAccount = new Account
        {
            Id = Guid.NewGuid(), Name = "Bankkonto", Type = AccountType.Asset,
            Icon = new AccountIcon("AccountBalance", "#1976D2"),
        };
        var cash = new Account
        {
            Id = Guid.NewGuid(), Name = "Bargeld", Type = AccountType.Asset,
            Icon = new AccountIcon("Wallet", "#4CAF50"),
        };
        var creditCard = new Account
        {
            Id = Guid.NewGuid(), Name = "Kreditkarte", Type = AccountType.Liability,
            Icon = new AccountIcon("CreditCard", "#F44336"),
        };
        var salary = new Account
        {
            Id = Guid.NewGuid(), Name = "Lohn", Type = AccountType.Revenue,
            Icon = new AccountIcon("Work", "#8BC34A"),
        };
        var groceries = new Account
        {
            Id = Guid.NewGuid(), Name = "Lebensmittel", Type = AccountType.Expense,
            Icon = new AccountIcon("ShoppingCart", "#FF9800"),
        };
        var rent = new Account
        {
            Id = Guid.NewGuid(), Name = "Miete", Type = AccountType.Expense,
            Icon = new AccountIcon("Home", "#9C27B0"),
        };
        var transport = new Account
        {
            Id = Guid.NewGuid(), Name = "ÖV / Transport", Type = AccountType.Expense,
            Icon = new AccountIcon("Train", "#00BCD4"),
        };
        var leisure = new Account
        {
            Id = Guid.NewGuid(), Name = "Freizeit", Type = AccountType.Expense,
            Icon = new AccountIcon("SportsEsports", "#E91E63"),
        };

        context.Accounts.AddRange(bankAccount, cash, creditCard, salary, groceries, rent, transport, leisure);

        // Budget entries (only Revenue/Expense accounts allowed)
        context.BudgetEntries.AddRange(
            new BudgetEntry
            {
                Id = Guid.NewGuid(), Description = "Monatslohn",
                Amount = 5500m, Account = salary, AccountingPeriod = period,
            },
            new BudgetEntry
            {
                Id = Guid.NewGuid(), Description = "Lebensmittel Budget",
                Amount = 600m, Account = groceries, AccountingPeriod = period,
            },
            new BudgetEntry
            {
                Id = Guid.NewGuid(), Description = "Monatsmiete",
                Amount = 1500m, Account = rent, AccountingPeriod = period,
            },
            new BudgetEntry
            {
                Id = Guid.NewGuid(), Description = "GA / Halbtax",
                Amount = 200m, Account = transport, AccountingPeriod = period,
            },
            new BudgetEntry
            {
                Id = Guid.NewGuid(), Description = "Freizeit Budget",
                Amount = 300m, Account = leisure, AccountingPeriod = period,
            }
        );

        // Journal entries
        context.JournalEntries.AddRange(
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 25),
                Description = "Lohn Januar",
                Amount = 5500m, DebitAccount = bankAccount, CreditAccount = salary,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 28),
                Description = "Miete Januar",
                Amount = 1500m, DebitAccount = rent, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 15),
                Description = "Migros Wocheneinkauf",
                Amount = 87.50m, DebitAccount = groceries, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 2, 3),
                Description = "SBB GA Monatsrate",
                Amount = 195m, DebitAccount = transport, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 2, 10),
                Description = "Kino & Abendessen",
                Amount = 65m, DebitAccount = leisure, CreditAccount = creditCard,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 2, 25),
                Description = "Lohn Februar",
                Amount = 5500m, DebitAccount = bankAccount, CreditAccount = salary,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 2, 28),
                Description = "Miete Februar",
                Amount = 1500m, DebitAccount = rent, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 3, 5),
                Description = "Coop Grosseinkauf",
                Amount = 124.30m, DebitAccount = groceries, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 3, 12),
                Description = "Bargeldbezug",
                Amount = 200m, DebitAccount = cash, CreditAccount = bankAccount,
                Transaction = null,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 3, 15),
                Description = "Kreditkarte Abrechnung",
                Amount = 65m, DebitAccount = creditCard, CreditAccount = bankAccount,
                Transaction = null,
            }
        );

        await context.SaveChangesAsync();
    }
}
