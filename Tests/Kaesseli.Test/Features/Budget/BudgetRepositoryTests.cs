using Kaesseli.Infrastructure;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class BudgetRepositoryTests
{
    private static readonly AccountingPeriod ExpectedAccountPeriod =
        AccountingPeriod.Create(
            "Test Period",
            new DateOnly(year: 2000, month: 1, day: 1),
            new DateOnly(year: 2000, month: 12, day: 31)
        );

    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options)
    {
        var timeProvider = TimeProvider.System;
        return new(options, timeProvider);
    }

    [Fact]
    public async Task GetBudgetEntries_ShouldReturnFilteredEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetBudgetEntriesDb")
            .Options;

        var budgetEntries = CreateBudgetEntries();

        await using var setupContext = CreateContext(options);
        setupContext.BudgetEntries.Add(entity: budgetEntries.First());
        setupContext.BudgetEntries.Add(entity: budgetEntries.Last());
        await setupContext.SaveChangesAsync();

        var repository = new BudgetRepository(setupContext);
        // Act
        var entries = (
            await repository.GetBudgetEntries(
                ExpectedAccountPeriod.Id,
                accountId: null,
                accountType: null,
                CancellationToken.None
            )
        ).ToArray();

        // Assert
        entries.Length.ShouldBe(1);
        entries.All(e => e.AccountingPeriod.Id == ExpectedAccountPeriod.Id).ShouldBeTrue();
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
        [
            BudgetEntry.Create(
                description: "Description 1",
                amount: 42.42m,
                account: Account.Create("Account 1", AccountType.Revenue, new AccountIcon("favorite", "blue")),
                accountingPeriod: ExpectedAccountPeriod
            ),
            BudgetEntry.Create(
                description: "Description 2",
                amount: 24.24m,
                account: Account.Create("Account 2", AccountType.Expense, new AccountIcon("favorite", "blue")),
                accountingPeriod: AccountingPeriod.Create(
                    "Test Period 2",
                    new DateOnly(year: 2001, month: 1, day: 1),
                    new DateOnly(year: 2001, month: 12, day: 31)
                )
            ),
        ];

    [Fact]
    public async Task SetBudgetCommand_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "SetBudgetDb")
            .Options;

        var newEntry = BudgetEntry.Create(
            description: "Description",
            amount: 11.11m,
            account: Account.Create("Account", AccountType.Expense, new AccountIcon("favorite", "blue")),
            accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
        );

        await using var context = CreateContext(options);
        var repository = new BudgetRepository(context);

        // Act
        var result = await repository.SetBudget(newEntry, CancellationToken.None);

        // Assert
        Assert.Equivalent(newEntry, result);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext
            .BudgetEntries.Include(be => be.Account)
            .Include(be => be.AccountingPeriod)
            .Where(be => be.Id == newEntry.Id)
            .SingleAsync();
        Assert.Equivalent(newEntry, addedEntry);
    }
}
