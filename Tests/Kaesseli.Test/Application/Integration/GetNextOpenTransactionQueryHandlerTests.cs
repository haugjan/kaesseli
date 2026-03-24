using FluentAssertions;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Integration;

public class GetNextOpenTransactionQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectResult_WhenTransactionExists()
    {
        // Arrange
        var mockTransactionRepository = new Mock<ITransactionRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository.Object,
            mockAccountRepository.Object
        );

        var expectedTransaction = new SmartFaker<Transaction>().Generate();

        var accounts = new SmartFaker<Account>().Generate(count: 5);

        mockTransactionRepository
            .Setup(r => r.GetNextOpenTransaction(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransaction);
        mockAccountRepository
            .Setup(r => r.GetAccounts(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        var request = new GetNextOpenTransaction.Query { Skip = 1 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result
            .Should()
            .BeEquivalentTo(expectedTransaction, options => options.ExcludingMissingMembers());
        Assert.Equal(expectedTransaction.TransactionSummary?.Account.Name, result!.AccountName);

        Assert.Equal(accounts.Count, actual: result.SuggestedAccounts.Count());
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTransactionDoesNotExist()
    {
        // Arrange
        var mockTransactionRepository = new Mock<ITransactionRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository.Object,
            mockAccountRepository.Object
        );

        mockTransactionRepository
            .Setup(r => r.GetNextOpenTransaction(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: null);

        var request = new GetNextOpenTransaction.Query { Skip = 1 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
