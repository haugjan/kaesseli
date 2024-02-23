using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Journal;
using Kaesseli.Infrastructure.Common;
using Kaesseli.TestUtilities.Faker;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kaesseli.Infrastructure.Test.Journal;

public class JournalRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options) =>
        new(options);

    [Fact]
    public async Task GetJournalEntries_ShouldReturnFilteredEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "GetJournalEntriesDb")
                      .Options;

        var journalEntry = new SmartFaker<JournalEntry>()
                          .RuleFor(
                              be => be.ValueDate,
                              faker => new DateOnly(year: 2000 + faker.IndexFaker, month: 01, day: 01))
                          .Generate(count: 2);

        await using var setupContext = CreateContext(options);
        setupContext.JournalEntries.Add(entity: journalEntry.First());
        setupContext.JournalEntries.Add(entity: journalEntry.Last());
        await setupContext.SaveChangesAsync();

        var repository = new JournalRepository(setupContext);
        var request = new GetJournalEntriesRequest
        {
            CreditAccountId = null,
            DebitAccountId = null,
            FromDate = new DateOnly(year: 2000, month: 01, day: 01),
            ToDate = new DateOnly(year: 2000, month: 05, day: 01)
        };

        // Act
        var entries = (await repository.GetJournalEntries(request, CancellationToken.None)).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 1);
        entries.All(e => e.ValueDate >= request.FromDate && e.ValueDate < request.ToDate)
               .Should()
               .BeTrue();
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
            CreditAccount = new Account { Id = Guid.NewGuid(), Name = "CreditAccount", Type = AccountType.Asset },
            DebitAccount = new Account { Id = Guid.NewGuid(), Name = "DebitAccount", Type = AccountType.Asset },
            ValueDate = DateOnly.FromDateTime(DateTime.Now),
            Description = "Description",
            Amount = 11.11m
        };

        await using var context = CreateContext(options);
        var repository = new JournalRepository(context);

        // Act
        var result = await repository.AddJournalEntry(newEntry, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(newEntry);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext.JournalEntries
                                            .Include(be => be.DebitAccount)
                                            .Include(be => be.CreditAccount)
                                            .Where(be => be.Id == newEntry.Id)
                                            .SingleAsync();
        addedEntry.Should().BeEquivalentTo(newEntry);
    }

    [Fact]
    public async Task AddPreJournalEntry_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "AddPreJournalEntryDb")
                      .Options;

        var newEntry = new SmartFaker<PreJournalEntry>().Generate();

        await using var context = CreateContext(options);
        var repository = new JournalRepository(context);

        // Act
        var result = await repository.AddPreJournalEntry(newEntry, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(newEntry);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext.PreJournalEntries
                                            .Include(be => be.Account)
                                            .Where(be => be.Id == newEntry.Id)
                                            .SingleAsync();
        addedEntry.Should().BeEquivalentTo(newEntry);
    }
}