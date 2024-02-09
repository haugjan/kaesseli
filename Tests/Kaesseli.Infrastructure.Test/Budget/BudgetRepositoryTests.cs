using FluentAssertions;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using Kaesseli.Infrastructure.Budget;
using Kaesseli.Infrastructure.Common;
using Kaesseli.TestUtilities.Faker;
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

        var budgetEntry = new SmartFaker<BudgetEntry>().Generate(count: 2);
        var accountId = Guid.NewGuid();

        await using var setupContext = CreateContext(options);
        setupContext.BudgetEntries.Add(entity: budgetEntry.First());
        setupContext.BudgetEntries.Add(entity: budgetEntry.Last());
        await setupContext.SaveChangesAsync();

        var repository = new BudgetRepository(setupContext);
        var request = new SmartFaker<GetBudgetEntriesRequest>().Generate();

        // Act
        var entries = (await repository.GetBudgetEntries(request, CancellationToken.None)).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 1);
        entries.All(e => e.Account.Id == accountId && e.ValueDate >= request.FromDate && e.ValueDate < request.ToDate)
               .Should()
               .BeTrue();
    }

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
            Account = new Account { Id = Guid.NewGuid(), Name = "Account", Type = 0 },
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
        var addedEntry = await assertContext.BudgetEntries.FindAsync(newEntry.Id);
        addedEntry.Should().BeEquivalentTo(newEntry);
    }
}