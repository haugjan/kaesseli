using FluentAssertions;
using Kaesseli.Application.Utility;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.Infrastructure.Common;
using Kaesseli.Infrastructure.Integration;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Kaesseli.Test.Infrastructure.Integration;

public class TransactionRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options)
    {
        var dateTimeService = new Mock<IDateTimeService>().Object;
        var envService = new Mock<IEnvironmentService>().Object;
        return new(options, dateTimeService, envService);
    }

    [Fact]
    public async Task GetTransactionSummaries_ReturnsTransactionSummaries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetTransactionSummariesDb")
            .Options;

        var transactions = new SmartFaker<TransactionSummary>()
            .RuleFor(ts => ts.Transactions, value: new SmartFaker<Transaction>().Generate(count: 5))
            .Generate(count: 2);

        await using var setupContext = CreateContext(options);
        setupContext.TransactionSummaries.AddRange(transactions);
        await setupContext.SaveChangesAsync();

        var repository = new TransactionRepository(setupContext);

        // Act
        var entries = (await repository.GetTransactionSummaries(CancellationToken.None)).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 2);
        entries.Should().BeEquivalentTo(transactions);
    }

    [Fact]
    public async Task GetTransaction_ShouldReturnFilteredEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetTransactionsDb")
            .Options;

        var transactions = new SmartFaker<Transaction>().Generate(count: 2);

        await using var setupContext = CreateContext(options);
        setupContext.Transactions.AddRange(transactions);
        await setupContext.SaveChangesAsync();

        var repository = new TransactionRepository(setupContext);

        // Act
        var entries = (
            await repository.GetTransactions(
                transactions.Last().TransactionSummary!.Id,
                CancellationToken.None
            )
        ).ToArray();

        // Assert
        entries.Should().HaveCount(expected: 1);
        entries.Should().BeEquivalentTo(expectation: [transactions.Last()]);
    }

    [Fact]
    public async Task AddTransaction_ShouldAddEntry()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "AddTransactionDb")
            .Options;

        var transactionSummary = new SmartFaker<TransactionSummary>()
            .RuleFor(
                statement => statement.Transactions,
                value: new SmartFaker<Transaction>().Generate(count: 5)
            )
            .Generate();

        await using var context = CreateContext(options);
        var repository = new TransactionRepository(context);

        // Act
        var result = await repository.AddTransactionSummary(
            transactionSummary,
            CancellationToken.None
        );

        // Assert
        result.Should().BeEquivalentTo(transactionSummary);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext
            .TransactionSummaries.Where(be => be.Id == transactionSummary.Id)
            .Include(statement => statement.Transactions)
            .Include(statement => statement.Account)
            .SingleAsync();
        addedEntry
            .Should()
            .BeEquivalentTo(transactionSummary, opt => opt.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task GetNextOpenTransaction_ReturnsTransaction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetNextOpenTransactionDb")
            .Options;
        var context = CreateContext(options);
        var repository = new TransactionRepository(context);
        var transaction = new SmartFaker<Transaction>()
            .RuleFor(t => t.JournalEntries, value: Array.Empty<JournalEntry>())
            .Generate();
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetNextOpenTransaction(skip: 0, cancellationToken: default);

        // Assert
        Assert.NotNull(result);
    }
}
