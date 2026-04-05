using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

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
        result.ShouldNotBeNull();
        result.Length.ShouldBe(transactions1.Count);
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(transactions1.Select(t => t.Id).ToArray());

        mockRepository.Verify(
            repo => repo.GetTransactions(transactionSummaryGuid, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
