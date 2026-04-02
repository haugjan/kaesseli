using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using Kaesseli.Test.Faker;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Integration;

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
        getTransactionSummary.AccountName.ShouldBe(transactionSummary.Account.Name);
        getTransactionSummary.NrOfTransactions.ShouldBe(transactionSummary.Transactions.Count());
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
        current.Id.ShouldBe(transaction.Id);
        current.Amount.ShouldBe(transaction.Amount);
        current.Description.ShouldBe(transaction.Description);
        current.ValueDate.ShouldBe(transaction.ValueDate);
    }
}
