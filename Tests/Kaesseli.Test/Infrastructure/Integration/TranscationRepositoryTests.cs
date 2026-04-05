using Kaesseli.Infrastructure;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Infrastructure.Integration;

public class TransactionRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options)
    {
        var timeProvider = TimeProvider.System;
        var envService = new Mock<IEnvironmentService>().Object;
        return new(options, timeProvider, envService);
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
        entries.Length.ShouldBe(2);
        Assert.Equivalent(transactions, entries);
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
        entries.Length.ShouldBe(1);
        Assert.Equivalent(new[] { transactions.Last() }, entries);
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
        result.Id.ShouldBe(transactionSummary.Id);

        await using var assertContext = CreateContext(options);
        var addedEntry = await assertContext
            .TransactionSummaries.Where(be => be.Id == transactionSummary.Id)
            .Include(statement => statement.Transactions)
            .Include(statement => statement.Account)
            .SingleAsync();
        addedEntry.Id.ShouldBe(transactionSummary.Id);
        addedEntry.Reference.ShouldBe(transactionSummary.Reference);
        addedEntry.Transactions.Select(t => t.Id).ToArray()
            .ShouldBeEquivalentTo(transactionSummary.Transactions.Select(t => t.Id).ToArray());
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
