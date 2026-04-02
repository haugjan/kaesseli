using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Common;
using Kaesseli.Infrastructure.Journal;
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
        var dateTimeService = new Mock<IDateTimeService>().Object;
        var envService = new Mock<IEnvironmentService>().Object;
        return new(options, dateTimeService, envService);
    }

    [Fact]
    public async Task GetJournalEntries_ShouldReturnFilteredEntries()
    {
        // Arrange
        var expectedPeriodId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetJournalEntriesDb")
            .Options;

        var firstEntry = new SmartFaker<JournalEntry>()
            .RuleFor(
                be => be.AccountingPeriod,
                value: new AccountingPeriod
                {
                    Id = expectedPeriodId,
                    Description = string.Empty,
                    FromInclusive = default,
                    ToInclusive = default,
                }
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
                expectedPeriodId,
                accountId: null,
                accountType: null,
                CancellationToken.None
            )
        ).ToArray();

        // Assert
        entries.Length.ShouldBe(1);
        entries.All(e => e.AccountingPeriod.Id == expectedPeriodId).ShouldBeTrue();
    }

    [Fact]
    public async Task AddJournalEntry_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "AddJournalEntryDb")
            .Options;

        var newEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            CreditAccount = new Account
            {
                Id = Guid.NewGuid(),
                Name = "CreditAccount",
                Type = AccountType.Asset,
                Icon = new AccountIcon("favorite", "blue"),
            },
            DebitAccount = new Account
            {
                Id = Guid.NewGuid(),
                Name = "DebitAccount",
                Type = AccountType.Asset,
                Icon = new AccountIcon("favorite", "blue"),
            },
            ValueDate = DateOnly.FromDateTime(DateTime.Now),
            Description = "Description",
            Amount = 11.11m,
            Transaction = null,
            AccountingPeriod = new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FromInclusive = default,
                ToInclusive = default,
                Description = string.Empty,
            },
        };

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
