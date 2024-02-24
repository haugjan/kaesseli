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
        var request = new GetBudgetEntriesRequest
        {
            AccountId = null,
            FromDate = new DateOnly(year: 2000, month: 01, day: 01),
            ToDate = new DateOnly(year: 2000, month: 05, day: 01)
        };

        // Act
        var entries = (await repository.GetBudgetEntries(request, CancellationToken.None)).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 1);
        entries.All(e => e.ValueDate >= request.FromDate && e.ValueDate < request.ToDate)
               .Should()
               .BeTrue();
    }

    private static List<BudgetEntry> CreateBudgetEntries() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 2000, month: 01, day: 01),
            Description = "Description 1",
            Amount = 42.42m,
            Account = new Account { Id = Guid.NewGuid(), Name = "Account 1", Type = AccountType.Revenue }
        },

        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = new DateOnly(year: 2001, month: 01, day: 01),
            Description = "Description 2",
            Amount = 24.24m,
            Account = new Account { Id = Guid.NewGuid(), Name = "Account 2", Type = AccountType.Expense }
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
            Account = new Account { Id = Guid.NewGuid(), Name = "Account", Type = AccountType.Expense },
            ValueDate = DateOnly.FromDateTime(DateTime.Now),
            Description = "Description",
            Amount = 11.11m
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
                                            .Where(be => be.Id == newEntry.Id)
                                            .SingleAsync();
        addedEntry.Should().BeEquivalentTo(newEntry);
    }
}