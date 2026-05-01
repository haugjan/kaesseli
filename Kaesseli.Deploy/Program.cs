using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;
using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccountType = Kaesseli.Contracts.Accounts.AccountType;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
    .AddJsonFile(
        Path.Combine(AppContext.BaseDirectory, $"appsettings.{environment}.json"),
        optional: true
    )
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.user.json"), optional: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(TimeProvider.System);
services.AddInfrastructureServices(configuration);

await using var provider = services.BuildServiceProvider();

Console.WriteLine("Initializing CosmosDB database...");
using (var scope = provider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KaesseliContext>();
    await context.Database.EnsureCreatedAsync();
}
Console.WriteLine(
    "Database initialized successfully. Waiting for collections to become available..."
);
await Task.Delay(TimeSpan.FromSeconds(5));

if (environment == "Development")
{
    Console.WriteLine("Seeding development data...");
    await SeedDevelopmentDataAsync(provider);
    Console.WriteLine("Development data seeded successfully.");
}

static async Task SeedDevelopmentDataAsync(IServiceProvider serviceProvider)
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
    var period = AccountingPeriod.Create(
        $"Geschäftsjahr {currentYear}",
        new DateOnly(currentYear, 1, 1),
        new DateOnly(currentYear, 12, 31)
    );
    context.AccountingPeriods.Add(period);

    // Accounts
    var bankAccount = Account.Create(
        "Bankkonto",
        AccountType.Asset,
        "1000",
        "bank",
        new AccountIcon("AccountBalance", "#1976D2")
    );
    var cash = Account.Create(
        "Bargeld",
        AccountType.Asset,
        "1010",
        "cash",
        new AccountIcon("Wallet", "#4CAF50")
    );
    var creditCard = Account.Create(
        "Kreditkarte",
        AccountType.Liability,
        "2000",
        "credit-card",
        new AccountIcon("CreditCard", "#F44336")
    );
    var salary = Account.Create(
        "Lohn",
        AccountType.Revenue,
        "3000",
        "salary",
        new AccountIcon("Work", "#8BC34A")
    );
    var groceries = Account.Create(
        "Lebensmittel",
        AccountType.Expense,
        "4000",
        "groceries",
        new AccountIcon("ShoppingCart", "#FF9800")
    );
    var rent = Account.Create(
        "Miete",
        AccountType.Expense,
        "4010",
        "rent",
        new AccountIcon("Home", "#9C27B0")
    );
    var transport = Account.Create(
        "ÖV / Transport",
        AccountType.Expense,
        "4020",
        "transport",
        new AccountIcon("Train", "#00BCD4")
    );
    var leisure = Account.Create(
        "Freizeit",
        AccountType.Expense,
        "4030",
        "leisure",
        new AccountIcon("SportsEsports", "#E91E63")
    );

    context.Accounts.AddRange(
        bankAccount,
        cash,
        creditCard,
        salary,
        groceries,
        rent,
        transport,
        leisure
    );
    await context.SaveChangesAsync();

    // Budget entries (only Revenue/Expense accounts allowed)
    context.BudgetEntries.AddRange(
        BudgetEntry.Create("Monatslohn", 5500m, salary, period),
        BudgetEntry.Create("Lebensmittel Budget", 600m, groceries, period),
        BudgetEntry.Create("Monatsmiete", 1500m, rent, period),
        BudgetEntry.Create("GA / Halbtax", 200m, transport, period),
        BudgetEntry.Create("Freizeit Budget", 300m, leisure, period)
    );

    // Transaction Summaries & Transactions
    var bankSummary = TransactionSummary.Create(
        bankAccount,
        12000m,
        8547.20m,
        new DateOnly(currentYear, 1, 1),
        new DateOnly(currentYear, 3, 31),
        "CAMT-2026-Q1",
        new List<Transaction>()
    );
    context.TransactionSummaries.Add(bankSummary);

    var assignedTransactions = new List<Transaction>();
    var openTransactions = new List<Transaction>();

    // Zugeordnete Transaktionen (werden mit JournalEntries verknüpft)
    var txLohnJan = Transaction.Create(
        "LOHN JANUAR 2026 ARBEITGEBER AG",
        5500m,
        new DateOnly(currentYear, 1, 25),
        "Lohn Januar",
        "SAL-2026-01",
        new DateOnly(currentYear, 1, 25),
        "TRF",
        "Gehalt",
        "Arbeitgeber AG",
        null,
        bankSummary
    );
    var txMieteJan = Transaction.Create(
        "MIETE JANUAR 2026 VERMIETER IMMOBILIEN",
        -1500m,
        new DateOnly(currentYear, 1, 28),
        "Miete Januar",
        "MIETE-2026-01",
        new DateOnly(currentYear, 1, 28),
        "TRF",
        "Dauerauftrag",
        null,
        "Vermieter Immobilien AG",
        bankSummary
    );
    var txMigros = Transaction.Create(
        "MIGROS FILIALE BERN MARKTGASSE",
        -87.50m,
        new DateOnly(currentYear, 1, 15),
        "Migros Wocheneinkauf",
        "POS-20260115-001",
        new DateOnly(currentYear, 1, 15),
        "POS",
        "Einkauf",
        null,
        "Migros",
        bankSummary
    );
    assignedTransactions.AddRange([txLohnJan, txMieteJan, txMigros]);

    // Offene Transaktionen (noch nicht zugeordnet)
    openTransactions.AddRange([
        Transaction.Create(
            "COOP PRONTO TANKSTELLE BERN",
            -45.90m,
            new DateOnly(currentYear, 3, 18),
            "Coop Pronto Tankstelle",
            "POS-20260318-003",
            new DateOnly(currentYear, 3, 18),
            "POS",
            "Einkauf",
            null,
            "Coop Pronto",
            bankSummary
        ),
        Transaction.Create(
            "SPOTIFY AB STOCKHOLM",
            -29.00m,
            new DateOnly(currentYear, 3, 20),
            "Spotify Premium",
            "DD-20260320-001",
            new DateOnly(currentYear, 3, 20),
            "DD",
            "Lastschrift",
            null,
            "Spotify AB",
            bankSummary
        ),
        Transaction.Create(
            "SWISSCOM SCHWEIZ AG MOBILE ABO",
            -156.00m,
            new DateOnly(currentYear, 3, 22),
            "Swisscom Mobile Abo",
            "DD-20260322-002",
            new DateOnly(currentYear, 3, 22),
            "DD",
            "Lastschrift",
            null,
            "Swisscom (Schweiz) AG",
            bankSummary
        ),
        Transaction.Create(
            "ZAHNARZTPRAXIS DR MUELLER BERN",
            -320.00m,
            new DateOnly(currentYear, 3, 25),
            "Zahnarzt Dr. Müller",
            "TRF-20260325-001",
            new DateOnly(currentYear, 3, 25),
            "TRF",
            "Überweisung",
            null,
            "Dr. med. dent. Müller",
            bankSummary
        ),
        Transaction.Create(
            "MIGROS FILIALE BERN WANKDORF",
            -85.50m,
            new DateOnly(currentYear, 3, 28),
            "Migros Wocheneinkauf",
            "POS-20260328-002",
            new DateOnly(currentYear, 3, 28),
            "POS",
            "Einkauf",
            null,
            "Migros",
            bankSummary
        ),
    ]);

    await context.SaveChangesAsync();

    context.Transactions.AddRange(assignedTransactions);
    context.Transactions.AddRange(openTransactions);
    await context.SaveChangesAsync();

    // Journal entries
    context.JournalEntries.AddRange(
        JournalEntry.Create(
            new DateOnly(currentYear, 1, 25),
            "Lohn Januar",
            5500m,
            bankAccount,
            salary,
            period,
            txLohnJan
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 1, 28),
            "Miete Januar",
            1500m,
            rent,
            bankAccount,
            period,
            txMieteJan
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 1, 15),
            "Migros Wocheneinkauf",
            87.50m,
            groceries,
            bankAccount,
            period,
            txMigros
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 2, 3),
            "SBB GA Monatsrate",
            195m,
            transport,
            bankAccount,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 2, 10),
            "Kino & Abendessen",
            65m,
            leisure,
            creditCard,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 2, 25),
            "Lohn Februar",
            5500m,
            bankAccount,
            salary,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 2, 28),
            "Miete Februar",
            1500m,
            rent,
            bankAccount,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 3, 5),
            "Coop Grosseinkauf",
            124.30m,
            groceries,
            bankAccount,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 3, 12),
            "Bargeldbezug",
            200m,
            cash,
            bankAccount,
            period
        ),
        JournalEntry.Create(
            new DateOnly(currentYear, 3, 15),
            "Kreditkarte Abrechnung",
            65m,
            creditCard,
            bankAccount,
            period
        )
    );

    await context.SaveChangesAsync();
}
