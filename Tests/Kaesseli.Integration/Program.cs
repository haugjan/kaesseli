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

var accountingPeriodId = await mediator.Send(
    request: new AddAccountingPeriodCommand
    {
        FromInclusive = new DateOnly(year: 2023, month: 1, day: 1),
        ToInclusive = new DateOnly(year: 2023, month: 12, day: 31),
        Description = null
    });

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
await AddAccount(
    accountName: "Debitoren",
    AccountType.Asset,
    icon: "payments",
    iconColor: "red");

var equityId = await AddAccount(
                   accountName: "Eigenkapital",
                   AccountType.Liability,
                   icon: "person",
                   iconColor: "black");

await AddAccount(
    accountName: "Kreditoren",
    AccountType.Liability,
    icon: "person",
    iconColor: "black");

await AddAccount(
    accountName: "Lohn Jan",
    AccountType.Revenue,
    icon: "man",
    iconColor: "blue-3",
    budget: 131_000m,
    budgetDescription: "CeDe: 41'000, Digitec: 90'000");

await AddAccount(
    accountName: "Lohn Jasmine",
    AccountType.Revenue,
    icon: "woman",
    iconColor: "pink",
    budget: 49_740m);

await AddAccount(
    accountName: "Vergütung Vereine",
    AccountType.Revenue,
    icon: "sports_soccer",
    iconColor: "green");

await AddAccount(
    accountName: "Auswärts Essen",
    AccountType.Expense,
    icon: "restaurant",
    iconColor: "red",
    budget: 200);

await AddAccount(
    accountName: "ÖV",
    AccountType.Expense,
    icon: "train",
    iconColor: "green",
    budget: 3600,
    budgetDescription: "Abo Jan: 2'3000, Abo Leonie: 600, 2xHalbtax: 340");

await AddAccount(
    accountName: "Streaming",
    AccountType.Expense,
    icon: "live_tv",
    iconColor: "orange",
    budget: 1500,
    budgetDescription: "Spotifiy, Netflix, Youtube, ChatGPT: je 300, Sky: 180, res: Käufe/Miete");

await AddAccount(
    accountName: "Serafe",
    AccountType.Expense,
    icon: "tv",
    iconColor: "green",
    budget: 335);

await AddAccount(
    accountName: "Coiffeur",
    AccountType.Expense,
    icon: "content_cut",
    iconColor: "orange",
    budget: 2400);

await AddAccount(
    accountName: "Ferien",
    AccountType.Expense,
    icon: "flight",
    iconColor: "red",
    budget: 15000);

await AddAccount(
    accountName: "Geschenke",
    AccountType.Expense,
    icon: "redeem",
    iconColor: "red",
    budget: 100);

await AddAccount(
    accountName: "Krankenkasse",
    AccountType.Expense,
    icon: "sick",
    iconColor: "green",
    budget: 11700,
    budgetDescription: "KPT: 11'000, Sanitas: 700");

await AddAccount(
    accountName: "Arzt",
    AccountType.Expense,
    icon: "vaccines",
    iconColor: "red",
    budget: 3000);

await AddAccount(
    accountName: "Gesundheit",
    AccountType.Expense,
    icon: "medication",
    iconColor: "red",
    budget: 150);

await AddAccount(
    accountName: "Haushalt Basic",
    AccountType.Expense,
    icon: "home",
    iconColor: "green",
    budget: 1730 * 12);

await AddAccount(
    accountName: "Haushalt Plus",
    AccountType.Expense,
    icon: "apartment",
    iconColor: "red",
    budget: 300 * 12);

await AddAccount(
    accountName: "Hausrat",
    AccountType.Expense,
    icon: "bathtub",
    iconColor: "red",
    budget: 50);

await AddAccount(
    accountName: "Hobby",
    AccountType.Expense,
    icon: "fitness_center",
    iconColor: "orange",
    budget: 1500,
    budgetDescription: "Fitness, Red Ants etc.");

await AddAccount(
    accountName: "Möbel",
    AccountType.Expense,
    icon: "chair",
    iconColor: "red",
    budget: 1200);

await AddAccount(
    accountName: "Velo",
    AccountType.Expense,
    icon: "pedal_bike",
    iconColor: "orange",
    budget: 250);

await AddAccount(
    accountName: "Kleider Jan",
    AccountType.Expense,
    icon: "checkroom",
    iconColor: "blue-3",
    budget: 1200);

await AddAccount(
    accountName: "Kleider Jasmine",
    AccountType.Expense,
    icon: "checkroom",
    iconColor: "pink",
    budget: 1200);
await AddAccount(
    accountName: "Kleider Leonie",
    AccountType.Expense,
    icon: "checkroom",
    iconColor: "green",
    budget: 1000);
await AddAccount(
    accountName: "Kleider Enea",
    AccountType.Expense,
    icon: "checkroom",
    iconColor: "green",
    budget: 1000);

await AddAccount(
    accountName: "Hypothek",
    AccountType.Expense,
    icon: "living",
    iconColor: "green",
    budget: 11000,
    budgetDescription: "Annahme 2%");

await AddAccount(
    accountName: "Nebenkosten",
    AccountType.Expense,
    icon: "grass",
    iconColor: "green",
    budget: 12800);

await AddAccount(
    accountName: "Werterneuerung",
    AccountType.Expense,
    icon: "build",
    iconColor: "red",
    budget: 3000);

await AddAccount(
    accountName: "Sackgeld Jan",
    AccountType.Expense,
    icon: "wallet",
    iconColor: "blue-3",
    budget: 220);
await AddAccount(
    accountName: "Sackgeld Jasmine",
    AccountType.Expense,
    icon: "wallet",
    iconColor: "pink",
    budget: 220);
await AddAccount(
    accountName: "Sackgeld Leonie",
    AccountType.Expense,
    icon: "wallet",
    iconColor: "green",
    budget: 40);

await AddAccount(
    accountName: "Sackgeld Enea",
    AccountType.Expense,
    icon: "wallet",
    iconColor: "green",
    budget: 40);


await AddAccount(
    accountName: "Steuern",
    AccountType.Expense,
    icon: "flag",
    iconColor: "green",
    budget: 20000);

await AddAccount(
    accountName: "Strom",
    AccountType.Expense,
    icon: "bolt",
    iconColor: "orange",
    budget: 1600);

await AddAccount(
    accountName: "Telefon/Internet",
    AccountType.Expense,
    icon: "call",
    iconColor: "green",
    budget: 1600);
await AddAccount(
    accountName: "Zahnarzt",
    AccountType.Expense,
    icon: "health_and_safety",
    iconColor: "orange",
    budget: 1200);
await AddAccount(
    accountName: "Zeitung",
    AccountType.Expense,
    icon: "newspaper",
    iconColor: "green",
    budget: 200);
await AddAccount(
    accountName: "Optiker",
    AccountType.Expense,
    icon: "visibility",
    iconColor: "orange",
    budget: 1000);
await AddAccount(
    accountName: "Börni fix",
    AccountType.Expense,
    icon: "airport_shuttle",
    iconColor: "green",
    budget: 10_100);
await AddAccount(
    accountName: "Börni variabel",
    AccountType.Expense,
    icon: "airport_shuttle",
    iconColor: "red",
    budget: 1_550);

await AddAccount(
    accountName: "Minime fix",
    AccountType.Expense,
    icon: "directions_car",
    iconColor: "green",
    budget: 6_320);
await AddAccount(
    accountName: "Minime variabel",
    AccountType.Expense,
    icon: "directions_car",
    iconColor: "red",
    budget: 850);
await AddAccount(
    accountName: "Lebensversicherung",
    AccountType.Expense,
    icon: "apartment",
    iconColor: "green",
    budget: 1_100);

await AddAccount(
    accountName: "Säule 3a",
    AccountType.Expense,
    icon: "elderly",
    iconColor: "green",
    budget: 6768);

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
    amount: 14_347.93m,
    description: "Anfangsbestand",
    valueDate: new DateOnly(year: 2024, month: 01, day: 01),
    pfGiroId,
    equityId);

await AddJournalEntry(
    amount: 1_80m,
    description: "Anfangsbestand",
    valueDate: new DateOnly(year: 2024, month: 01, day: 01),
    pfSavingId,
    equityId);

// ReSharper restore StringLiteralTypo

Console.WriteLine(value: "Finish");
Console.ReadKey();

async Task<Guid> AddAccount(
    string accountName,
    AccountType accountType,
    string icon,
    string iconColor,
    decimal? budget = null,
    string? budgetDescription = null)
{
    var id = await mediator.Send(
                 request: new AddAccountCommand
                 {
                     Name = accountName,
                     Type = accountType,
                     Icon = icon,
                     IconColor = iconColor
                 });
    if(budget is not null)
        await AddBudgetEntry(budget.Value, description: budgetDescription ?? "Budget 2023", id);
    return id;
}

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