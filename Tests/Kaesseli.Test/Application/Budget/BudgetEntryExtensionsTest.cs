using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Budget;

public class BudgetEntryExtensionsTest
{
    [Fact]
    public void ToGetBudgetEntriesQueryResult_ReturnsCorrectQueryResult()
    {
        //Arrange
        var account = Account.Create("Name", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);
        var budgetEntry = BudgetEntry.Create(
            description: "Description",
            amount: 42,
            account: account,
            accountingPeriod: accountingPeriod
        );

        //Act
        var queryResult = budgetEntry.ToGetBudgetEntriesQueryResult();

        //Assert
        queryResult.AccountId.ShouldBe(budgetEntry.Account.Id);
        queryResult.AccountingPeriodId.ShouldBe(budgetEntry.AccountingPeriod.Id);
        queryResult.Id.ShouldBe(budgetEntry.Id);
        queryResult.Description.ShouldBe(budgetEntry.Description);
        queryResult.Amount.ShouldBe(budgetEntry.Amount);
    }
}
