using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class GetTransactionSummariesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectTransactionSummaries()
    {
        // Arrange
        var mockRepository = Substitute.For<ITransactionRepository>();
        var transactionSummaries = new SmartFaker<TransactionSummary>()
            .RuleFor(ts => ts.Transactions, value: new SmartFaker<Transaction>().Generate(count: 5))
            .Generate(count: 5);

        mockRepository
            .GetTransactionSummaries(Arg.Any<CancellationToken>())
            .Returns(transactionSummaries);

        var handler = new GetTransactionSummaries.Handler(mockRepository);

        // Act
        var result = (await handler.Handle(CancellationToken.None)).ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(transactionSummaries.Count);
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(transactionSummaries.Select(ts => ts.Id).ToArray());

        await mockRepository.Received(1)
            .GetTransactionSummaries(Arg.Any<CancellationToken>());
    }
}
