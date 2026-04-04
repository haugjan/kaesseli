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

        // Transaction Summaries & Transactions
        var bankSummary = new TransactionSummary
        {
            Id = Guid.NewGuid(), Account = bankAccount,
            BalanceBefore = 12000m, BalanceAfter = 8547.20m,
            ValueDateFrom = new DateOnly(currentYear, 1, 1),
            ValueDateTo = new DateOnly(currentYear, 3, 31),
            Reference = "CAMT-2026-Q1",
            Transactions = [],
        };
        context.TransactionSummaries.Add(bankSummary);

        var assignedTransactions = new List<Transaction>();
        var openTransactions = new List<Transaction>();

        // Zugeordnete Transaktionen (werden mit JournalEntries verknüpft)
        var txLohnJan = new Transaction
        {
            Id = Guid.NewGuid(), Amount = 5500m,
            ValueDate = new DateOnly(currentYear, 1, 25), BookDate = new DateOnly(currentYear, 1, 25),
            Description = "Lohn Januar", Reference = "SAL-2026-01",
            RawText = "LOHN JANUAR 2026 ARBEITGEBER AG", TransactionCode = "TRF",
            TransactionCodeDetail = "Gehalt", Debtor = "Arbeitgeber AG", Creditor = null,
            TransactionSummary = bankSummary, JournalEntries = null,
        };
        var txMieteJan = new Transaction
        {
            Id = Guid.NewGuid(), Amount = -1500m,
            ValueDate = new DateOnly(currentYear, 1, 28), BookDate = new DateOnly(currentYear, 1, 28),
            Description = "Miete Januar", Reference = "MIETE-2026-01",
            RawText = "MIETE JANUAR 2026 VERMIETER IMMOBILIEN", TransactionCode = "TRF",
            TransactionCodeDetail = "Dauerauftrag", Debtor = null, Creditor = "Vermieter Immobilien AG",
            TransactionSummary = bankSummary, JournalEntries = null,
        };
        var txMigros = new Transaction
        {
            Id = Guid.NewGuid(), Amount = -87.50m,
            ValueDate = new DateOnly(currentYear, 1, 15), BookDate = new DateOnly(currentYear, 1, 15),
            Description = "Migros Wocheneinkauf", Reference = "POS-20260115-001",
            RawText = "MIGROS FILIALE BERN MARKTGASSE", TransactionCode = "POS",
            TransactionCodeDetail = "Einkauf", Debtor = null, Creditor = "Migros",
            TransactionSummary = bankSummary, JournalEntries = null,
        };
        assignedTransactions.AddRange([txLohnJan, txMieteJan, txMigros]);

        // Offene Transaktionen (noch nicht zugeordnet)
        openTransactions.AddRange(
        [
            new Transaction
            {
                Id = Guid.NewGuid(), Amount = -45.90m,
                ValueDate = new DateOnly(currentYear, 3, 18), BookDate = new DateOnly(currentYear, 3, 18),
                Description = "Coop Pronto Tankstelle", Reference = "POS-20260318-003",
                RawText = "COOP PRONTO TANKSTELLE BERN", TransactionCode = "POS",
                TransactionCodeDetail = "Einkauf", Debtor = null, Creditor = "Coop Pronto",
                TransactionSummary = bankSummary, JournalEntries = null,
            },
            new Transaction
            {
                Id = Guid.NewGuid(), Amount = -29.00m,
                ValueDate = new DateOnly(currentYear, 3, 20), BookDate = new DateOnly(currentYear, 3, 20),
                Description = "Spotify Premium", Reference = "DD-20260320-001",
                RawText = "SPOTIFY AB STOCKHOLM", TransactionCode = "DD",
                TransactionCodeDetail = "Lastschrift", Debtor = null, Creditor = "Spotify AB",
                TransactionSummary = bankSummary, JournalEntries = null,
            },
            new Transaction
            {
                Id = Guid.NewGuid(), Amount = -156.00m,
                ValueDate = new DateOnly(currentYear, 3, 22), BookDate = new DateOnly(currentYear, 3, 22),
                Description = "Swisscom Mobile Abo", Reference = "DD-20260322-002",
                RawText = "SWISSCOM SCHWEIZ AG MOBILE ABO", TransactionCode = "DD",
                TransactionCodeDetail = "Lastschrift", Debtor = null, Creditor = "Swisscom (Schweiz) AG",
                TransactionSummary = bankSummary, JournalEntries = null,
            },
            new Transaction
            {
                Id = Guid.NewGuid(), Amount = -320.00m,
                ValueDate = new DateOnly(currentYear, 3, 25), BookDate = new DateOnly(currentYear, 3, 25),
                Description = "Zahnarzt Dr. Müller", Reference = "TRF-20260325-001",
                RawText = "ZAHNARZTPRAXIS DR MUELLER BERN", TransactionCode = "TRF",
                TransactionCodeDetail = "Überweisung", Debtor = null, Creditor = "Dr. med. dent. Müller",
                TransactionSummary = bankSummary, JournalEntries = null,
            },
            new Transaction
            {
                Id = Guid.NewGuid(), Amount = -85.50m,
                ValueDate = new DateOnly(currentYear, 3, 28), BookDate = new DateOnly(currentYear, 3, 28),
                Description = "Migros Wocheneinkauf", Reference = "POS-20260328-002",
                RawText = "MIGROS FILIALE BERN WANKDORF", TransactionCode = "POS",
                TransactionCodeDetail = "Einkauf", Debtor = null, Creditor = "Migros",
                TransactionSummary = bankSummary, JournalEntries = null,
            },
        ]);

        context.Transactions.AddRange(assignedTransactions);
        context.Transactions.AddRange(openTransactions);

        // Journal entries
        context.JournalEntries.AddRange(
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 25),
                Description = "Lohn Januar",
                Amount = 5500m, DebitAccount = bankAccount, CreditAccount = salary,
                Transaction = txLohnJan,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 28),
                Description = "Miete Januar",
                Amount = 1500m, DebitAccount = rent, CreditAccount = bankAccount,
                Transaction = txMieteJan,
            },
            new JournalEntry
            {
                Id = Guid.NewGuid(), AccountingPeriod = period,
                ValueDate = new DateOnly(currentYear, 1, 15),
                Description = "Migros Wocheneinkauf",
                Amount = 87.50m, DebitAccount = groceries, CreditAccount = bankAccount,
                Transaction = txMigros,
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
