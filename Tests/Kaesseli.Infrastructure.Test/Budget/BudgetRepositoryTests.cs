using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kaesseli.Infrastructure.Test.Budget;

public class BudgetRepositoryTests
{
    private static readonly Guid ExpectedAccountPeriodId = Guid.NewGuid();

    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options) =>
        new(options);

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
        var request = new GetBudgetEntriesRequest { AccountId = null, AccountingPeriodId = ExpectedAccountPeriodId };

        // Act
        var entries = (await repository.GetBudgetEntries(request, CancellationToken.None)).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 1);
        entries.All(e => e.AccountingPeriod.Id == ExpectedAccountPeriodId)
               .Should()
               .BeTrue();
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            Description = "Description 1",
            Amount = 42.42m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Type = AccountType.Revenue,
                Icon = "favorite",
                IconColor = "blue"
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = ExpectedAccountPeriodId,
                FromInclusive = new DateOnly(
                    year: 2000,
                    month: 1,
                    day: 1),
                ToInclusive = new DateOnly(
                    year: 2000,
                    month: 12,
                    day: 31),
                Description = string.Empty
            }
        },

        new()
        {
            Id = Guid.NewGuid(),
            Description = "Description 2",
            Amount = 24.24m,
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Type = AccountType.Expense,
                Icon = "favorite",
                IconColor = "blue"
            },
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = new DateOnly(
                    year: 2001,
                    month: 1,
                    day: 1),
                ToInclusive = new DateOnly(
                    year: 2001,
                    month: 12,
                    day: 31),
                Description = string.Empty
            }
        }
    ];

    [Fact]
    public async Task AddBudgetEntry_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "AddBudgetEntryDb")
                      .Options;

        var newEntry = new BudgetEntry
        {
            Id = Guid.NewGuid(),
            Account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account",
                Type = AccountType.Expense,
                Icon = "favorite",
                IconColor = "blue"
            },
            Description = "Description",
            Amount = 11.11m,
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty
            }
        };

        await using var context = CreateContext(options);
        var repository = new BudgetRepository(context);

        // Act
        var result = await repository.AddBudgetEntry(newEntry, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(newEntry);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext.BudgetEntries
                                            .Include(be => be.Account)
                                            .Include(be => be.AccountingPeriod)
                                            .Where(be => be.Id == newEntry.Id)
                                            .SingleAsync();
        addedEntry.Should().BeEquivalentTo(newEntry);
    }
}