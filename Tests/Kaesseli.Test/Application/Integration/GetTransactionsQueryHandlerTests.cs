using FluentAssertions;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Integration;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Integration;

public class GetTransactionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectTransactions()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var transactionSummaryGuid = Guid.NewGuid();
        var transactions1 = new SmartFaker<Transaction>()
            .RuleFor(
                tran => tran.TransactionSummary,
                value: new SmartFaker<TransactionSummary>()
                    .RuleFor(ts => ts.Id, transactionSummaryGuid)
                    .Generate()
            )
            .Generate(count: 5);
        var transactions2 = new SmartFaker<Transaction>().Generate(count: 3);

        mockRepository
            .Setup(repo => repo.GetTransactions(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (Guid transactionSummaryId, CancellationToken _) =>
                    transactions1
                        .Concat(transactions2)
                        .Where(tran => tran.TransactionSummary!.Id == transactionSummaryId)
            );

        var handler = new GetTransactions.Handler(mockRepository.Object);
        var query = new GetTransactions.Query(transactionSummaryGuid);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(transactions1.Count);
        result
            .Should()
            .BeEquivalentTo(
                transactions1,
                options =>
                    options.Excluding(t => t.TransactionSummary).Excluding(t => t.JournalEntries)
            );

        mockRepository.Verify(
            repo => repo.GetTransactions(transactionSummaryGuid, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
