using FluentAssertions;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Integration;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

public class GetTransactionSummariesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectTransactionSummaries()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var transactionSummaries = new SmartFaker<TransactionSummary>()
                                   .RuleFor(ts => ts.Transactions, value: new SmartFaker<Transaction>().Generate(count: 5))
                                   .Generate(count: 5);

        mockRepository.Setup(repo => repo.GetTransactionSummaries(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(transactionSummaries);

        var handler = new GetTransactionSummariesQueryHandler(mockRepository.Object);
        var query = new GetTransactionSummariesQuery();

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(transactionSummaries.Count);
        result.Should()
              .BeEquivalentTo(
                  transactionSummaries,
                  options => options
                             .Excluding(ts => ts.Account)
                             .Excluding(ts => ts.Transactions));

        mockRepository.Verify(
            repo => repo.GetTransactionSummaries(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
