using FluentAssertions;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.TestUtilities.Faker;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

public class TransactionSummaryExtensionsTests
{
    [Fact]
    public void ToGetTransactionSummary_TransferCorrectly()
    {
        //Arrange
        var transactionSummary = new SmartFaker<TransactionSummary>()
                                 .RuleFor(ts => ts.Transactions, value: new SmartFaker<Transaction>().Generate(count: 5))
                                 .Generate();

        //Act
        var getTransactionSummary = transactionSummary.ToGetTransactionSummary();

        //Assert
        getTransactionSummary.Should()
                             .BeEquivalentTo(
                                 transactionSummary,
                                 options => options
                                            .Excluding(ts => ts.Account)
                                            .Excluding(ts => ts.Transactions));
        getTransactionSummary.AccountName.Should().Be(transactionSummary.Account.Name);
        getTransactionSummary.NrOfTransactions.Should().Be(expected: transactionSummary.Transactions.Count());
    }

    [Fact]
    public void ToGetNextOpenTransactionResult_ReturnsToGetNextOpenTransactionResult()
    {
        //Arrange
        var transaction = new SmartFaker<Transaction>().Generate();
        var accounts = new SmartFaker<Account>().Generate(count: 5);

        //Act
        var current = transaction.ToGetNextOpenTransactionResult(accounts);

        //Assert
        current.Should().BeEquivalentTo(transaction, options => options.ExcludingMissingMembers());

    }

}