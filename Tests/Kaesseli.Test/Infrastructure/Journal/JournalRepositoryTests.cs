using Kaesseli.Infrastructure;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Infrastructure.Journal;

public class JournalRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options)
    {
        var timeProvider = TimeProvider.System;
        return new(options, timeProvider);
    }

    [Fact]
    public async Task GetJournalEntries_ShouldReturnFilteredEntries()
    {
        // Arrange
        var expectedPeriod = AccountingPeriod.Create("Test Period", default, default);

        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetJournalEntriesDb")
            .Options;

        var firstEntry = new SmartFaker<JournalEntry>()
            .RuleFor(
                be => be.AccountingPeriod,
                value: expectedPeriod
            )
            .Generate();
        var secondEntry = new SmartFaker<JournalEntry>().Generate();

        await using var setupContext = CreateContext(options);
        setupContext.JournalEntries.Add(entity: firstEntry);
        setupContext.JournalEntries.Add(entity: secondEntry);
        await setupContext.SaveChangesAsync();

        var repository = new JournalRepository(setupContext);
        // Act
        var entries = (
            await repository.GetJournalEntries(
                expectedPeriod.Id,
                accountId: null,
                accountType: null,
                CancellationToken.None
            )
        ).ToArray();

        // Assert
        entries.Length.ShouldBe(1);
        entries.All(e => e.AccountingPeriod.Id == expectedPeriod.Id).ShouldBeTrue();
    }

    [Fact]
    public async Task AddJournalEntry_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "AddJournalEntryDb")
            .Options;

        var creditAccount = Account.Create("CreditAccount", AccountType.Asset, new AccountIcon("favorite", "blue"));
        var debitAccount = Account.Create("DebitAccount", AccountType.Asset, new AccountIcon("favorite", "blue"));
        var newEntry = JournalEntry.Create(
            valueDate: DateOnly.FromDateTime(DateTime.Now),
            description: "Description",
            amount: 11.11m,
            debitAccount: debitAccount,
            creditAccount: creditAccount,
            accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
        );

        await using var context = CreateContext(options);
        var repository = new JournalRepository(context);

        // Act
        var result = await repository.AddJournalEntry(newEntry, CancellationToken.None);

        // Assert
        Assert.Equivalent(newEntry, result);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext
            .JournalEntries.Include(be => be.DebitAccount)
            .Include(be => be.CreditAccount)
            .Include(be => be.AccountingPeriod)
            .Where(be => be.Id == newEntry.Id)
            .SingleAsync();
        Assert.Equivalent(newEntry, addedEntry);
    }
}
