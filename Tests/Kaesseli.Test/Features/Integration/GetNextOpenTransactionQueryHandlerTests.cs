using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class GetNextOpenTransactionQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectResult_WhenTransactionExists()
    {
        // Arrange
        var mockTransactionRepository = Substitute.For<ITransactionRepository>();
        var mockAccountRepository = Substitute.For<IAccountRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository,
            mockAccountRepository
        );

        var expectedTransaction = new SmartFaker<Transaction>().Generate();

        var accounts = new SmartFaker<Account>().Generate(count: 5);

        mockTransactionRepository
            .GetNextOpenTransaction(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedTransaction);
        mockAccountRepository
            .GetAccounts(Arg.Any<CancellationToken>())
            .Returns(accounts);

        var request = new GetNextOpenTransaction.Query(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(expectedTransaction.Id);
        result.Amount.ShouldBe(expectedTransaction.Amount);
        result.Description.ShouldBe(expectedTransaction.Description);
        result.ValueDate.ShouldBe(expectedTransaction.ValueDate);
        Assert.Equal(expectedTransaction.TransactionSummary?.Account.Name, result!.AccountName);

        Assert.Equal(accounts.Count, actual: result.SuggestedAccounts.Count());
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTransactionDoesNotExist()
    {
        // Arrange
        var mockTransactionRepository = Substitute.For<ITransactionRepository>();
        var mockAccountRepository = Substitute.For<IAccountRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository,
            mockAccountRepository
        );

        mockTransactionRepository
            .GetNextOpenTransaction(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);

        var request = new GetNextOpenTransaction.Query(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
