using Kaesseli.Application.Accounts;
using Kaesseli.Application.Budget;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var host = builder.Build();
await host.StartAsync();
host.Services.InitializeDatabase();

var mediator = host.Services.GetService<IMediator>()!;
var rand = new Random();

if ((await mediator.Send(request: new GetAccountsQuery())).Any())
{
    Console.WriteLine(value: "Not an empty db. Exit.");
    Console.ReadKey();
    throw new Exception();
}

var accountingPeriodId = Guid.NewGuid();

// ReSharper disable StringLiteralTypo
var pfGiroId = await AddAccount(
                   accountName: "PostFinance Giro",
                   AccountType.Asset,
                   icon: "approval",
                   iconColor: "yellow");
var pfSavingId = await AddAccount(
                     accountName: "PostFinance Spar",
                     AccountType.Asset,
                     icon: "savings",
                     iconColor: "yellow");
_ = await AddAccount(
                     accountName: "Debitoren",
                     AccountType.Asset,
                     icon: "payments",
                     iconColor: "red");

var equityId = await AddAccount(
                   accountName: "Eigenkapital",
                   AccountType.Liability,
                   icon: "person",
                   iconColor: "black");

_ =  await AddAccount(
                   accountName: "Kreditoren",
                   AccountType.Liability,
                   icon: "person",
                   iconColor: "black");

var wages1Id = await AddAccount(
                   accountName: "Lohn Jan",
                   AccountType.Revenue,
                   icon: "man",
                   iconColor: "blue");

var wages2Id = await AddAccount(
                   accountName: "Lohn Jasmine",
                   AccountType.Revenue,
                   icon: "woman",
                   iconColor: "pink");
_ = await AddAccount(
        accountName: "Vergütung Vereine",
        AccountType.Revenue,
        icon: "sports_soccer",
        iconColor: "green");

_ = await AddAccount(
        accountName: "Auswärts Essen",
        AccountType.Revenue,
        icon: "restaurant",
        iconColor: "red");

_ = new List<Guid>
{
    await AddAccount(
        accountName: "ÖV",
        AccountType.Expense,
        icon: "train",
        iconColor: "red"),
   
    await AddAccount(
        accountName: "Streaming",
        AccountType.Expense,
        icon: "live_tv",
        iconColor: "blue"),

    //await AddAccount(
    //    accountName: "Aktivitäten",
    //    AccountType.Expense,
    //    icon: "pool",
    //    iconColor: "blue"),


    await AddAccount(
        accountName: "Billag",
        AccountType.Expense,
        icon: "tv",
        iconColor: "grey-9"),

    await AddAccount(
        accountName: "Coiffeur",
        AccountType.Expense,
        icon: "content_cut",
        iconColor: "brown"),
    
    await AddAccount(
        accountName: "Ferien",
        AccountType.Expense,
        icon: "flight",
        iconColor: "blue"),
    
    await AddAccount(
        accountName: "Geschenke",
        AccountType.Expense,
        icon: "redeem",
        iconColor: "red"),
    
    await AddAccount(
        accountName: "Krankenkasse",
        AccountType.Expense,
        icon: "sick",
        iconColor: "yellow"),
    
    await AddAccount(
        accountName: "Arzt",
        AccountType.Expense,
        icon: "vaccines",
        iconColor: "blue"),
    
    await AddAccount(
        accountName: "Gesundheit",
        AccountType.Expense,
        icon: "medication",
        iconColor: "red"),
    
    await AddAccount(
        accountName: "Haushalt Basic",
        AccountType.Expense,
        icon: "home",
        iconColor: "green"),
    
    await AddAccount(
        accountName: "Haushalt Plus",
        AccountType.Expense,
        icon: "apartment",
        iconColor: "green"),
    
    await AddAccount(
        accountName: "Hausrat",
        AccountType.Expense,
        icon: "bathtub",
        iconColor: "brown"),
    
    await AddAccount(
        accountName: "Hobby",
        AccountType.Expense,
        icon: "fitness_center",
        iconColor: "red"),
    
    await AddAccount(
        accountName: "Möbel",
        AccountType.Expense,
        icon: "chair",
        iconColor: "brown"),
    
    await AddAccount(
        accountName: "Velo",
        AccountType.Expense,
        icon: "pedal_bike",
        iconColor: "blue"),
    
    await AddAccount(
        accountName: "Kleider Jan",
        AccountType.Expense,
        icon: "checkroom",
        iconColor: "blue"),
    
    await AddAccount(
        accountName: "Kleider Jasmine",
        AccountType.Expense,
        icon: "checkroom",
        iconColor: "pink"),

    await AddAccount(
        accountName: "Kleider Leonie",
        AccountType.Expense,
        icon: "checkroom",
        iconColor: "green"),

    await AddAccount(
        accountName: "Kleider Enea",
        AccountType.Expense,
        icon: "checkroom",
        iconColor: "green"),

    await AddAccount(
        accountName: "Hypothek",
        AccountType.Expense,
        icon: "living",
        iconColor: "green"),

    await AddAccount(
        accountName: "Nebenkosten",
        AccountType.Expense,
        icon: "grass",
        iconColor: "green"),


    await AddAccount(
        accountName: "Werterneuerung",
        AccountType.Expense,
        icon: "build",
        iconColor: "grey-9"),

    await AddAccount(
        accountName: "Sackgeld Jan",
        AccountType.Expense,
        icon: "wallet",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Sackgeld Jasmine",
        AccountType.Expense,
        icon: "wallet",
        iconColor: "pink"),

    await AddAccount(
        accountName: "Sackgeld Leonie",
        AccountType.Expense,
        icon: "wallet",
        iconColor: "pink"),


    await AddAccount(
        accountName: "Sackgeld Enea",
        AccountType.Expense,
        icon: "wallet",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Sparen",
        AccountType.Expense,
        icon: "savings",
        iconColor: "green"),

    await AddAccount(
        accountName: "Steuern",
        AccountType.Expense,
        icon: "flag",
        iconColor: "red"),

    await AddAccount(
        accountName: "Strom",
        AccountType.Expense,
        icon: "bolt",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Telefon/Internet",
        AccountType.Expense,
        icon: "call",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Zahnarzt",
        AccountType.Expense,
        icon: "health_and_safety",
        iconColor: "red"),

    await AddAccount(
        accountName: "Zeitung",
        AccountType.Expense,
        icon: "newspaper",
        iconColor: "grey"),

    await AddAccount(
        accountName: "Temporär",
        AccountType.Expense,
        icon: "question_mark",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Optiker",
        AccountType.Expense,
        icon: "eyeglasses",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Auto Börni",
        AccountType.Expense,
        icon: "eyeglasses",
        iconColor: "blue"),

    await AddAccount(
        accountName: "Auto Minime",
        AccountType.Expense,
        icon: "eyeglasses",
        iconColor: "blue")


};

var allAccounts = (await mediator.Send(request: new GetAccountsQuery())).ToList();
var expenseAccounts = allAccounts.Where(account => account.TypeId == AccountType.Expense)
                                 .ToList();
var revenueAccounts = allAccounts.Where(account => account.TypeId == AccountType.Revenue)
                                 .ToList();

foreach (var expenseAccount in expenseAccounts)
{
    var amount = (decimal)Math.Round(value: rand.NextDouble() * 1200, digits: 2);
    await AddBudgetEntry(
        amount,
        description: "Budget",
        expenseAccount.Id);
}

foreach (var revenueAccount in revenueAccounts)
{
    var amount = (decimal)Math.Round(value: rand.NextDouble() * 3000, digits: 2);
    await AddBudgetEntry(
        amount,
        description: "Budget",
        revenueAccount.Id);
}

await AddJournalEntry(
    amount: 9_365.12m,
    description: "Anfangsbestand",
    valueDate: new DateOnly(year: 2024, month: 01, day: 01),
    pfGiroId,
    equityId);

await AddJournalEntry(
    amount: 10_356.10m,
    description: "Anfangsbestand",
    valueDate: new DateOnly(year: 2024, month: 01, day: 01),
    pfSavingId,
    equityId);

for (var month = 1; month < 13; month++)
{
    var valueDate = new DateOnly(year: 2024, month, day: 25);
    await AddJournalEntry(
        amount: 11_000m,
        description: $"Lohn {valueDate.ToString(format: "MMMM")}",
        valueDate,
        pfGiroId,
        wages1Id);
    await AddJournalEntry(
        amount: 4_000m,
        description: $"Lohn {valueDate.ToString(format: "MMMM")}",
        valueDate,
        pfGiroId,
        wages2Id);
    foreach (var account in expenseAccounts)
    {
        var expenseValueDate = new DateOnly(year: 2024, month, day: rand.Next(minValue: 1, maxValue: 29));
        await AddJournalEntry(
            amount: (decimal)Math.Round(value: rand.NextDouble() * 100, digits: 2),
            description: $"Ausgabe für {account.Name}",
            expenseValueDate,
            account.Id,
            pfGiroId);
    }
}

// ReSharper restore StringLiteralTypo

Console.WriteLine(value: "Finish");
Console.ReadKey();

async Task<Guid> AddAccount(
    string accountName,
    AccountType accountType,
    string icon,
    string iconColor) =>
    await mediator.Send(
        request: new AddAccountCommand
        {
            Name = accountName,
            Type = accountType,
            Icon = icon,
            IconColor = iconColor
        });

async Task AddBudgetEntry(
    decimal amount,
    string description,
    Guid accountId) =>
    await mediator.Send(
        request: new AddBudgetEntryCommand
        {
            Amount = amount,
            Description = description,
            AccountId = accountId,
            AccountingPeriodId = accountingPeriodId
        });

async Task AddJournalEntry(
    decimal amount,
    string description,
    DateOnly? valueDate,
    Guid debitAccountId,
    Guid creditAccountId) =>
    await mediator.Send(
        request: new AddJournalEntryCommand
        {
            Amount = amount,
            Description = description,
            ValueDate = valueDate,
            CreditAccountId = creditAccountId,
            DebitAccountId = debitAccountId,
            AccountingPeriodId = accountingPeriodId
        });